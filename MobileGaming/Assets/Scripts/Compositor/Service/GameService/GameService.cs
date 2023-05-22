using System;
using Addressables;
using Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.AddressableAssets.Addressables;
using Object = UnityEngine.Object;

namespace Service
{
    public class GameService : IGameService
    {
        [DependsOnService] private ISceneService sceneService;
        [DependsOnService] private IInputService inputService;
        private ScriptableSettings settings;

        private Transform levelParent;
        private int currentLevel;

        private GameObject _dialogueManagerGo;
        private SorcererController sorcererController;
        private MagicLinesManager magicLinesManager;

        private GameObject endGameCanvasGo;
        private TextMeshProUGUI endGameText;

        private static event Action<int> OnLoadLevel;

        public GameService(ScriptableSettings baseSettings)
        {
            settings = baseSettings;
            settings.SetAsGlobalSettings();
        }

        [ServiceInit]
        public void InitGame()
        {
            LoadEventSystem();
            
            LoadMenu();

            OnLoadLevel = LoadLevelI;

            //LoadAssets();
        }

        private void LoadEventSystem()
        {
            AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("EventSystem", DontDestroy);

            void DontDestroy(GameObject go)
            {
                var obj = Object.Instantiate(go);
                Object.DontDestroyOnLoad(obj);
                Release(go);
            }
        }

        private void LoadMenu()
        {
            sceneService.LoadScene(1);
        }
        
        public static void LoadLevel(int index)
        {
            OnLoadLevel?.Invoke(index);
        }

        public void LoadLevelI(int index)
        {
            if (_dialogueManagerGo != null)
            {
                AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("SorcererController", LoadSorcererController);
                return;
            }
            
            AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("DialogueCanvas", LoadDialogueManager);
            
            void LoadDialogueManager(GameObject dialogueManagerGo)
            {
                _dialogueManagerGo = Object.Instantiate(dialogueManagerGo);
                DialogueManager.SetInstance(_dialogueManagerGo.GetComponent<DialogueManager>());
                Release(dialogueManagerGo);
                
                AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("SorcererController", LoadSorcererController);
            }
            
            void LoadSorcererController(GameObject sorcererControllerGo)
            {
                sorcererController = Object.Instantiate(sorcererControllerGo).GetComponent<SorcererController>();
                magicLinesManager = sorcererController.GetComponent<MagicLinesManager>();
                // TODO - apply materials, then release
                //Release(sorcererControllerGo);
                
                endGameText = sorcererController.endGameText;
                endGameCanvasGo = sorcererController.endGameCanvasGo;
                    
                sorcererController.endGameButton.onClick.AddListener(RestartGame);

                currentLevel = index - 2;
                NextLevel();
            }
        }
        
        private void NextLevel(int _)
        {
            NextLevel();
        }
        
        private void NextLevel()
        {
            currentLevel++;
            if (currentLevel >= settings.LevelScenes.Length) currentLevel = 0;

            Level.OnLevelLoad += OnLevelLoaded;
            sceneService.LoadSceneAsync(settings.LevelScenes[currentLevel]);
        }

        private void OnLevelLoaded(Level level)
        {
            Debug.Log("Level loaded");
            level.SetUIComponents(sorcererController.scoreSlider ,sorcererController.timeLeftText);
            level.OnEndLevel += UpdateEndGameText;
            level.OnEndLevel += NextLevel;

            magicLinesManager.SetCameras(level.Camera);
            
            //level.Run();
        }

        private void UpdateEndGameText(int state)
        {
            endGameText.text = state == 0 ? "lose :c" : "win :)";
            endGameCanvasGo.SetActive(true);
        }

        private void RestartGame()
        {
            sceneService.LoadScene(0);
        }
    }
}