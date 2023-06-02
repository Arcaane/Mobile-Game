using System;
using Addressables;
using Attributes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        private TextMeshProUGUI endGameButtonText;
        private Image endGameSorcererImage;
        private Sprite[] _sorcererSprites;

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
                endGameSorcererImage = sorcererController.endGameImage;
                _sorcererSprites = sorcererController.endGameSorcererSprites;
                endGameButtonText = sorcererController.endGameButtonText;
                
                sorcererController.endToGoMenuButton.onClick.AddListener(RestartGame);
                
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
            currentLevel = index - 1;
            NextLevel();
        }

        private void SetListeners()
        {
            EventManager.AddListener<LoadLevelEvent>(OnLevelLoaded);
            EventManager.AddListener<EndLevelEvent>(ReturnToMenu);
            EventManager.AddListener<EndLevelEvent>(UpdateEndGameText);
        }
        
        private void OnLevelLoaded(LoadLevelEvent loadLevelEvent)
        {
            levelService.InitLevel(loadLevelEvent.Level);
        }

        private void ReturnToMenu(EndLevelEvent endLevelEvent)
        {
            if(endLevelEvent.SaveScore) return;
            sceneService.LoadScene(1);
        }

        private void UpdateEndGameText(EndLevelEvent endLevelEvent)
        {
            if(!endLevelEvent.SaveScore) return;
            endGameText.text = endLevelEvent.State == 0 ? "lose :c" : "win :)";
            endGameButtonText.text = endLevelEvent.State == 0 ? "Try Again" : "Next Level"; 
            endGameSorcererImage.sprite = endLevelEvent.State == 0 ? _sorcererSprites[0] : _sorcererSprites[1];
            sorcererController.endGameButton.onClick.AddListener(endLevelEvent.State == 0 ?  ReloadScene : NextLevel);
            
            // Afficher les étoiles gagnés
            if (GamePathManager.instance.levels[currentLevel].starsClaimedCount > endLevelEvent.State) 
                GamePathManager.instance.levels[currentLevel].starsClaimedCount = endLevelEvent.State;
            
            //GamePathManager.instance.levels[currentLevel] fonctionne pas, on peut faire un dico de <scene,leveldata> ou <int,leveldata> avec les data qui ont le score et les etoiles 
            Debug.Log($"Level {GamePathManager.instance.levels[currentLevel]}, étoiles gagnés : {endLevelEvent.State}");
            
            // Débloque le niveau suivant
            if (endLevelEvent.State == 1 && !GamePathManager.instance.levels[currentLevel + 1].isLevelUnlock)
            {
                GamePathManager.instance.unlockedLevels++;
                Debug.Log($"Level {GamePathManager.instance.levels[currentLevel + 1]} unlocked");
            }

            if (endLevelEvent.State > 0)
            {
                GamePathManager.instance.SaveLevel(currentLevel);
            }
            endGameCanvasGo.SetActive(true);
        }

        private void RestartGame()
        {
            endGameCanvasGo.SetActive(false);
            sceneService.LoadSceneAsync(1);
        }

        private void ReloadScene()
        {
            endGameCanvasGo.SetActive(false);
            sceneService.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
        
        private void NextLevel()
        {
            endGameCanvasGo.SetActive(false);
            currentLevel++;
            if (currentLevel >= settings.LevelScenes.Length) currentLevel = 0;
            
            inputService.Disable();
            sceneService.LoadSceneAsync(settings.LevelScenes[currentLevel]);
        }
    }
}