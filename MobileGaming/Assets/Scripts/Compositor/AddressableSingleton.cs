using UnityEngine;
using static UnityEngine.AddressableAssets.Addressables;

public abstract class AddressableSingleton<T> : MonoBehaviour where T : AddressableSingleton<T>
{
    public static T Instance => instance != null ? instance : LoadInstance();
    private static T instance;

    private static T LoadInstance()
    {
        var op = LoadAssetAsync<GameObject>($"{typeof(T)}");
        instance = op.WaitForCompletion().GetComponent<T>();
        instance.FirstLoad();
        return instance;
    }

    protected abstract void FirstLoad();
}