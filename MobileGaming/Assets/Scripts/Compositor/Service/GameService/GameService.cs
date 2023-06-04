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
        private ScriptableItemDatabase itemDatabase;

        private Transform levelParent;

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
            itemDatabase = baseSettings.itemDB;
        }

        [ServiceInit]
        public void InitGame()
        {
            LoadEventSystem();
            
            LoadMenu();

            LoadSorcererController();

            OnLoadLevel = LoadLevelI;
            
            SetListeners();
            
            itemDatabase.GetProgress();
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
                
                sorcererController.endToGoMenuButton.onClick.AddListener(GoToMenu);
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
            endGameCanvasGo.SetActive(false);

            inputService.Disable();
            sceneService.LoadSceneAsync(index);
        }

        private void SetListeners()
        {
            EventManager.AddListener<LoadLevelEvent>(OnLevelLoaded);
            EventManager.AddListener<EndLevelEvent>(ReturnToMenu);
            EventManager.AddListener<EndLevelEvent>(UpdateScriptableLevelsOnLevelEnd);
            EventManager.AddListener<EndLevelEvent>(UpdateEndGameText);
            EventManager.AddListener<EndLevelEvent>(ObtainEndLevelRewards);
            EventManager.AddListener<EndLevelEvent>(UnlockNextLevel);
            
            itemDatabase.SetListeners();
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
            endGameText.text = endLevelEvent.Stars == 0 ? "lose :c" : "win :)";
            endGameButtonText.text = endLevelEvent.Stars == 0 ? "Try Again" : "Next Level"; 
            endGameSorcererImage.sprite = endLevelEvent.Stars == 0 ? _sorcererSprites[0] : _sorcererSprites[1];
            sorcererController.endGameButton.onClick.AddListener(endLevelEvent.Stars == 0 ?  RetryLevel : NextLevel);
            
            endGameCanvasGo.SetActive(true);
            
            void NextLevel()
            {
                var sceneIndex = endLevelEvent.ScriptableLevel.NextLevelScene;
                if (sceneIndex == 1)
                {
                    sceneService.LoadScene(1);
                    return;
                }
                LoadLevelI(sceneIndex);
            }
            
            void RetryLevel()
            {
                LoadLevelI(endLevelEvent.ScriptableLevel.LevelScene);
            }
        }

        private void UnlockNextLevel(EndLevelEvent endLevelEvent)
        {
            if(!endLevelEvent.SaveScore) return;
            itemDatabase.SetLevelUnlocked(endLevelEvent.ScriptableLevel.CurrentLevel+1);
        }
        
        private void UpdateScriptableLevelsOnLevelEnd(EndLevelEvent endLevelEvent)
        {
            var level = endLevelEvent.Level;
            if(!endLevelEvent.SaveScore) return;

            var score = endLevelEvent.Score;
            if (score < level.LevelScriptable.Score) score = level.LevelScriptable.Score;
        
            level.LevelScriptable.SetProgress(endLevelEvent.Stars,score);
        }

        private void ObtainEndLevelRewards(EndLevelEvent endLevelEvent)
        {
            var level = endLevelEvent.Level;
            if(!level.LevelScriptable.IsLastLevelOfChapter) return;

            var chapter = level.LevelScriptable.CurrentChapter;
           
            itemDatabase.AddChapterToGacha(chapter);
        }

        private void GoToMenu()
        {
            endGameCanvasGo.SetActive(false);
            sceneService.LoadSceneAsync(1);
        }
    }
}