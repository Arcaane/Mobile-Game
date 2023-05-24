using System;
using Addressables;
using Attributes;
using TMPro;
using UnityEngine;
using static UnityEngine.AddressableAssets.Addressables;
using Object = UnityEngine.Object;

namespace Service
{
    public class GameService : IGameService
    {
        [DependsOnService] private ISceneService sceneService;
        [DependsOnService] private IInputService inputService;
        [DependsOnService] private ILevelService levelService;
        [DependsOnService] private IMagicLineService magicLineService;
        private ScriptableSettings settings;

        private Transform levelParent;
        private int currentLevel;

        private GameObject _dialogueManagerGo;
        private SorcererController sorcererController;
        private MagicLinesData magicLinesData;

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

            LoadSorcererController();

            OnLoadLevel = LoadLevelI;
        }

        private void LoadSorcererController()
        {
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
                magicLinesData = sorcererController.GetComponent<MagicLinesData>();
                magicLineService.SetData(magicLinesData);
                
                endGameText = sorcererController.endGameText;
                endGameCanvasGo = sorcererController.endGameCanvasGo;
                    
                sorcererController.endGameButton.onClick.AddListener(RestartGame);
                
                SetListeners();
            }
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

        private void LoadLevelI(int index)
        {
            currentLevel = index - 2;
            NextLevel();
        }

        private void SetListeners()
        {
            EventManager.AddListener<LoadLevelEvent>(OnLevelLoaded);
            EventManager.AddListener<EndLevelEvent>(UpdateEndGameText);
        }
        
        private void NextLevel()
        {
            currentLevel++;
            if (currentLevel >= settings.LevelScenes.Length) currentLevel = 0;
            
            inputService.Disable();
            sceneService.LoadSceneAsync(settings.LevelScenes[currentLevel]);
        }

        private void OnLevelLoaded(LoadLevelEvent loadLevelEvent)
        {
            levelService.InitLevel(loadLevelEvent.Level);
        }

        private void UpdateEndGameText(EndLevelEvent endLevelEvent)
        {
            endGameText.text = endLevelEvent.State == 0 ? "lose :c" : "win :)";
            endGameCanvasGo.SetActive(true);
        }

        private void RestartGame()
        {
            endGameCanvasGo.SetActive(false);
            sceneService.LoadSceneAsync(1);
        }
    }
}