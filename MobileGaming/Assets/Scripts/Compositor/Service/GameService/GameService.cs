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
        private Level level;
        
        private Product currentProduct; // TODO - multiple products (product slot class list)
        
        private Interactable currentInteractable;

        private GameObject endGameCanvasGo;
        private TextMeshProUGUI currentProductText;
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
            AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("Cameras", LoadCameras);
        }

        private void LoadGame()
        {
            Interactable.ResetEvents();
            Interactable.OnRangeEnter += OnInteractableEnter;
            Interactable.OnRangeExit += OnInteractableExit;
            
            UpdateProductText();
            
            level.Run();
        }

        private void LoadCameras(GameObject camerasGo)
        {
            var cameras = Object.Instantiate(camerasGo).GetComponent<CameraComponents>();
            Release(camerasGo);

            AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("JoystickCanvas", LoadJoystickCanvas);

            void LoadJoystickCanvas(GameObject joystickCanvasGo)
            {
                var joystickCanvas = Object.Instantiate(joystickCanvasGo).GetComponent<JoystickComponents>();
                // TODO - apply sprites, then release
                //Release(joystickCanvasGo);
                
                AddressableHelper.LoadAssetAsyncWithCompletionHandler<GameObject>("SorcererController", LoadSorcererController);

                void LoadSorcererController(GameObject sorcererControllerGo)
                {
                    sorcererController = Object.Instantiate(sorcererControllerGo).GetComponent<SorcererController>();
                    // TODO - apply materials, then release
                    //Release(sorcererControllerGo);

                    sorcererController.perspCameraGo = cameras.perspCameraGo;
                    sorcererController.perspCam = cameras.perspCamera;
                    sorcererController.orthoCam = cameras.othoCamera;

                    sorcererController.joystickParentGo = joystickCanvas.parentGo;
                    sorcererController.joystickParentTr = joystickCanvas.parentTr;
                    sorcererController.joystickTr = joystickCanvas.joystickTr;
                    
                    sorcererController.SetVariables();

                    sorcererController.OnInteract += InteractWithInteractable;

                    levelParent = new GameObject().transform;
                    
                    currentProductText = sorcererController.currentProductText;
                    endGameText = sorcererController.endGameText;
                    endGameCanvasGo = sorcererController.endGameCanvasGo;
                    
                    sorcererController.endGameButton.onClick.AddListener(RestartGame);

                    currentLevel = settings.DefaultStartIndex;
                    NextLevel();
                    
                    LoadGame();
                }
            }
        }

        private void NextLevel(int _)
        {
            NextLevel();
        }

        private void NextLevel()
        {
            if (levelParent.childCount > 0)
            {
                for (int i = levelParent.childCount - 1; i >= 0; i--)
                {
                    Object.Destroy(levelParent.GetChild(i).gameObject);
                }
            }

            if (level != null)
            {
                level.OnEndLevel -= UpdateEndGameText;
                level.OnEndLevel -= NextLevel;
            }

            level = Object.Instantiate(settings.Levels[currentLevel],levelParent);
            level.transform.position = level.LevelPosition;
            
            level.SetUIComponents(sorcererController.scoreText,sorcererController.timeLeftText);
            level.OnEndLevel += UpdateEndGameText;
            level.OnEndLevel += NextLevel;

            currentLevel++;
            if (currentLevel >= settings.Levels.Length) currentLevel = 0;
        }

        private void InteractWithInteractable()
        {
            if(currentInteractable is null) return;
            
            //Debug.Log($"Interacting, product is {currentProduct}");
            currentInteractable.Interact(currentProduct,out currentProduct);
            //Debug.Log($"Interacted, product is now {currentProduct}");
            UpdateProductText();
        }

        private void UpdateProductText()
        {
            currentProductText.text = $"Current :\n{currentProduct}";
        }

        private void OnInteractableEnter(Interactable interactable)
        {
            currentInteractable = interactable;
        }
        
        private void OnInteractableExit(Interactable interactable)
        {
            if (currentInteractable == interactable) currentInteractable = null;
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