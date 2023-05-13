using Addressables;
using Addressables.Components;
using Attributes;
using TMPro;
using UnityEngine;
using static UnityEngine.AddressableAssets.Addressables;

namespace Service
{
    public class GameService : IGameService
    {
        [DependsOnService] private ISceneService sceneService;
        [DependsOnService] private IInputService inputService;
        private ScriptableSettings settings;

        private Transform levelParent;
        private int currentLevel;
        
        private SorcererController sorcererController;
        private MagicLinesManager magicLinesManager;

        private GameObject endGameCanvasGo;
        private TextMeshProUGUI endGameText;

        public GameService(ScriptableSettings baseSettings)
        {
            settings = baseSettings;
            settings.SetAsGlobalSettings();
        }

        [ServiceInit]
        public void InitGame()
        {
            LoadAssets();
        }

        private void LoadAssets()
        {
            //AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("Cameras", LoadCameras);
            
            AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("DialogueCanvas", LoadDialogueManager);
            
            void LoadDialogueManager(GameObject dialogueManagerGo)
            {
                var dialogueManager = Object.Instantiate(dialogueManagerGo);
                DialogueManager.SetInstance(dialogueManager.GetComponent<DialogueManager>());
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

                currentLevel = settings.DefaultStartIndex - 1;
                NextLevel();
            }
        }
        
        private void LoadCameras(GameObject camerasGo)
        {
            var cameras = Object.Instantiate(camerasGo).GetComponent<CameraComponents>();
            Release(camerasGo);
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
            level.SetUIComponents(sorcererController.scoreText,sorcererController.timeLeftText);
            level.OnEndLevel += UpdateEndGameText;
            level.OnEndLevel += NextLevel;

            magicLinesManager.SetCameras(level.Camera);
            
            level.Run();
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