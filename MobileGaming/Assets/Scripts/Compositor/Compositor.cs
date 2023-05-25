using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Attributes;
using Exception;
using Service;
using UnityEngine;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Service.SceneService;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Compositor : MonoBehaviour
{
    [SerializeField] private float ticksPerSecond = 60;
    public static float TicksPerSecond { get; private set; }
    public static float DeltaTick => 1f / TicksPerSecond;
    [SerializeField] private ScriptableSettings settings;
    private double currentTime = 0;
    
    protected struct FieldEntry
    {
        public object dependant;
        public FieldInfo field;
    }

    protected readonly Dictionary<Type, IService> m_services = new Dictionary<Type, IService>();
    protected readonly Dictionary<Type, List<FieldEntry>> m_dependencySlots = new Dictionary<Type, List<FieldEntry>>();
    protected event Action OnTickAction;
    protected event Action OnUpdateAction;

#if UNITY_EDITOR
    [ContextMenu("Reload Assembly")]
    private void ReloadAssembly()
    {
        EditorUtility.RequestScriptReload();
    }
#endif

    private bool ResolveDependencies()
    {
        foreach (KeyValuePair<Type, List<FieldEntry>> slotsForType in m_dependencySlots)
        {
            Type type = slotsForType.Key;
            List<FieldEntry> slots = slotsForType.Value;

            if (!m_services.ContainsKey(type))
            {
                Debug.LogError($"Service type '{type}' is missing from the instantiated services!");
                return false;
            }

            IService serviceToInject = m_services[type];

            // here we perform the injection
            foreach (FieldEntry slot in slots)
            {
                slot.field.SetValue(slot.dependant, serviceToInject);
            }

            // this formulation can also check whether any certain service is not in use
            // and is safe for removal from instantiation
        }

        // with dependencies sorted, the dependency slot list can be emptied out (to not keep any
        // dangling references when a specific system goes offline
        m_dependencySlots.Clear();

        return true;
    }

    private void CollectDependencies(object newSystem)
    {
        CollectDependenciesOnType(newSystem.GetType(), newSystem);
    }

    private void CollectDependenciesOnType(Type type, object @obj)
    {
        // search through the fields for any ones marked [DependsOnService]
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                   BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var dependencies =
                (DependsOnServiceAttribute[])Attribute.GetCustomAttributes(field, typeof(DependsOnServiceAttribute));
            // if this field has no [DependsOnService] attribute, skip it
            if (dependencies.Length == 0)
                continue;

            // append this field to the list of fields that want that specific service
            foreach (DependsOnServiceAttribute _ in dependencies)
            {
                if (field.FieldType.IsInterface && typeof(IService).IsAssignableFrom(field.FieldType) &&
                    field.FieldType != typeof(IService))
                {
                    List<FieldEntry> dependentFields = m_dependencySlots.ContainsKey(field.FieldType)
                        ? m_dependencySlots[field.FieldType]
                        : new List<FieldEntry>();
                    dependentFields.Add(new FieldEntry
                    {
                        dependant = @obj,
                        field = field
                    });
                    m_dependencySlots[field.FieldType] = dependentFields;
                }
                else
                {
                    throw new NotAServiceException(@obj, field, type);
                }
            }
        }

        // search in base types until we find nothing (to catch private members of base types)
        if (type.BaseType != null)
        {
            CollectDependenciesOnType(type.BaseType, @obj);
        }
    }

    protected void AddService<T>(T service) where T : IService
    {
        if (m_services.ContainsKey(typeof(T)))
        {
            throw new DuplicateServiceException(typeof(T), m_services[typeof(T)], service);
        }

        CollectDependencies(service);
        m_services[typeof(T)] = service;
    }

    /// <summary>
    /// Goes through the list of services calling any void() methods marked with [ServiceInit],
    /// and calls these methods (for initializing message subscriptions and such)
    /// </summary>
    protected async UniTask InitializeServices()
    {
        // some services (e.g. EntitiesLooksService are registered to multiple service interfaces - make sure they only get visited & initialized once)
        var uniqueServices = new HashSet<IService>(m_services.Values);

        foreach (IService service in uniqueServices)
        {
            Type serviceType = service.GetType();
            foreach (MethodInfo methodInfo in serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public |
                                                                     BindingFlags.NonPublic))
            {
                ServiceInitAttribute[] initAttributes =
                    (ServiceInitAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(ServiceInitAttribute));
                if (initAttributes.Length > 0)
                {
                    foreach (ServiceInitAttribute _ in initAttributes)
                    {
                        if (methodInfo.GetParameters().Length > 0)
                        {
                            throw new ArgumentException(
                                "[ServiceInit] attribute can only be applied to methods with no arguments");
                        }

                        if (methodInfo.ReturnType == typeof(void))
                        {
                            methodInfo.Invoke(service, null);
                        }
                        else if (methodInfo.ReturnType == typeof(UniTaskVoid))
                        {
                            if ((AsyncStateMachineAttribute)methodInfo.GetCustomAttribute(
                                    typeof(AsyncStateMachineAttribute)) != null)
                            {
                                // if method is async, let's dispatch a UniTask without observing any return value
                                ((UniTaskVoid)methodInfo.Invoke(service, null)).Forget();
                            }
                            else
                            {
                                throw new ArgumentException(
                                    $"[ServiceInit] UniTaskVoid method {serviceType.Name}::{methodInfo.Name} needs to be tagged as async");
                            }
                        }
                        else if (methodInfo.ReturnType == typeof(UniTask))
                        {
                            if ((AsyncStateMachineAttribute)methodInfo.GetCustomAttribute(
                                    typeof(AsyncStateMachineAttribute)) != null)
                            {
                                // if method is async, let's dispatch & await it, in case it has dependencies
                                var task = (UniTask)methodInfo.Invoke(service, null);
                                await task;
                            }
                            else
                            {
                                throw new ArgumentException(
                                    $"[ServiceInit] UniTask method {serviceType.Name}::{methodInfo.Name} needs to be tagged as async");
                            }
                        }
                        else if (methodInfo.ReturnType == typeof(IEnumerator))
                        {
                            var enumerator = (IEnumerator)methodInfo.Invoke(service, null);

                            await UniTask.WaitUntil(() => !enumerator.MoveNext());
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"[ServiceInit] attribute can only be applied to methods with return types void, async UniTaskVoid or IEnumerator - {methodInfo.ReturnType.Name} {serviceType.Name}::{methodInfo.Name}(...) was not one of these");
                        }
                    }
                }


                OnTickAttribute[] tickAttributes = (OnTickAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(OnTickAttribute));
                
                if (tickAttributes.Length > 0)
                {
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        OnTickAction += InvokeMethod;

                        void InvokeMethod()
                        {
                            methodInfo.Invoke(service, null);
                        }
                    }
                }
                
                OnUpdateAttribute[] updateAttributes = (OnUpdateAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(OnUpdateAttribute));
                
                if (updateAttributes.Length > 0)
                {
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        OnUpdateAction += InvokeMethod;

                        void InvokeMethod()
                        {
                            methodInfo.Invoke(service, null);
                        }
                    }
                }
            }
        }
    }

    // Add Services Here
    private void CreateAndWireObjects()
    {
        TicksPerSecond = ticksPerSecond;
        
        AddService<ILevelService>(new LevelService());
        AddService<ISceneService>(new SceneService(true));
        AddService<IInputService>(new InputService(true));
        AddService<IMagicLineService>(new MagicLineService(false));
        AddService<IGameService>(new GameService(settings));
    }

    private void Awake()
    {
        InitCompositor().Forget();
    }

    private async UniTaskVoid InitCompositor()
    {
        bool composed = await Compose();

        // do the composition
        if (composed)
        {
            // keep this composition alive across scene reloads
            DontDestroyOnLoad(this);
        }
        else
        {
            Debug.LogError($"Composition in {gameObject.name} failed to sort dependencies - systems won't be started!");
        }
    }

    private void Update()
    {
        InvokeActions();
    }

    private void InvokeActions()
    {
        OnUpdateAction?.Invoke();
        
        currentTime += Time.deltaTime;
        
        
        if(ticksPerSecond != 0) if (currentTime < 1 / ticksPerSecond) return;
        
        currentTime = 0;
        OnTickAction?.Invoke();
    }

    protected virtual async UniTask<bool> Compose()
    {
        CreateAndWireObjects();

        if (!ResolveDependencies())
        {
            throw new System.Exception("Failed to resolve dependencies");
        }

        await InitializeServices();
        return true;
    }
}