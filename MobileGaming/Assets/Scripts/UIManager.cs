using System;
using System.Collections;
using System.Diagnostics;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject darkmodeCanvasGo;
    [SerializeField] private Canvas darkmodeCanvas;
    
    [SerializeField] private Canvas hudCanvas;

    [Header("Stars Progression")]
    [SerializeField] private RectTransform starReferenceTr;
    [SerializeField] private Image[] starsHolder;
    [SerializeField] private Sprite starFill;
    [SerializeField] private Sprite starEmpty;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private RectTransform chroneNeedleTr;
    [SerializeField] private Vector3 needleRotation = new Vector3(0, 0, 10f);
    
    //[SerializeField] private GameObject pauseMenu;

    private SorcererController sorcererController;
    private MagicLinesData _magicLinesData;
    private Slider scoreSlider;
    private TextMeshProUGUI timerText;

    Vector3Int scorePalier;
    private void Start()
    {
        sorcererController = GetComponent<SorcererController>();
        _magicLinesData = GetComponent<MagicLinesData>();
        timerText = sorcererController.timeLeftText;
        scoreSlider = sorcererController.scoreSlider;
        darkmodeCanvasGo.SetActive(false);
        
        HideHud();
        
        EventManager.AddListener<LoadLevelEvent>(HideHudOnLevelInit);
        EventManager.AddListener<LoadLevelEvent>(PlaceStars);
        EventManager.AddListener<EndLevelEvent>(HideHudOnLevelEnd);
        EventManager.AddListener<LevelScoreUpdatedEvent>(UpdateScore);
        EventManager.AddListener<LevelTimeUpdatedEvent>(UpdateTime);
        EventManager.AddListener<ActivateDarkmodeEvent>(ActivateDarkmodeCanvas);
        
        EventManager.AddListener<LoadTutorialEvent>(HideTutorialElements);
        EventManager.AddListener<EndTutorialEvent>(ShowTutorialElements);

        void HideHud()
        {
            EnableTimer(true);
            sorcererController.hudCanvasGO.SetActive(false);
            sorcererController.menuCanvasGO.SetActive(false);
        }

        void HideHudOnLevelEnd(EndLevelEvent _)
        {
            HideHud();
        }

        
        void HideHudOnLevelInit(LoadLevelEvent loadLevelEvent)
        {
            darkmodeCanvas.worldCamera = loadLevelEvent.Level.Camera;
            darkmodeCanvas.planeDistance = 2f;
            hudCanvas.worldCamera = loadLevelEvent.Level.Camera;
            hudCanvas.planeDistance = 2f;
            levelText.text = $"Level {loadLevelEvent.Level.LevelScriptable.CurrentLevel}";

            for (int i = 0; i < starsHolder.Length; i++)
            {
                starsHolder[i].sprite = starEmpty;
                starsHolder[i].transform.DOLocalRotate(new Vector3(0, 0, 0), 0.01f);
                starsHolder[i].transform.DOScale(1, 0.1f); 
            }
            
            HideHud();
        }
        
        void PlaceStars(LoadLevelEvent loadLevelEvent)
        {
            var scriptable = loadLevelEvent.Level.LevelScriptable;
            var palier1 = scriptable.ScoreToWin;
            var palier2 = scriptable.Palier2;
            var palier3 = scriptable.Palier3;

            var refWidth = starReferenceTr.sizeDelta.x;
            
            starsHolder[0].rectTransform.localPosition = new Vector3(refWidth*((float)palier1/palier3),0,0);
            starsHolder[1].rectTransform.localPosition = new Vector3(refWidth*((float)palier2/palier3),0,0);
            starsHolder[2].rectTransform.localPosition = new Vector3(refWidth,0,0);
        }
        
        void UpdateScore(LevelScoreUpdatedEvent scoreUpdatedEvent)
        {
            scoreSlider.value = ((float)scoreUpdatedEvent.Score) / scoreUpdatedEvent.Palier3;
            
            if(scoreUpdatedEvent.Score >= scoreUpdatedEvent.Palier1) UpdateStarUI(0);
            if(scoreUpdatedEvent.Score >= scoreUpdatedEvent.Palier2) UpdateStarUI(1);
            if(scoreUpdatedEvent.Score >= scoreUpdatedEvent.Palier3) UpdateStarUI(2);
            
            void UpdateStarUI(int i)
            {
                if (starsHolder[i].sprite == starFill) return;
                
                starsHolder[i].sprite = starFill;
                starsHolder[i].transform.DOScale(1.8f, 0.55f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    starsHolder[i].transform.DOScale(1.15f, 0.25f);
                });
                starsHolder[i].transform.DOLocalRotate(new Vector3(0, 180, 0), 0.255f).SetDelay(0.25f);
            }
        }

        void UpdateTime(LevelTimeUpdatedEvent timeUpdatedEvent)
        {
            var duration = timeUpdatedEvent.MaxTime - timeUpdatedEvent.CurrentTime;
            var minutes = Mathf.Floor(duration / 60);
            var seconds = duration % 60;
            var inExtraTime = duration < 0;
            timerText.text = $"{(!inExtraTime ? $"{minutes:00}:{seconds:00}" : "Extra time !")}";
            if(inExtraTime) return;
            chroneNeedleTr.Rotate(needleRotation);
        }

        void ActivateDarkmodeCanvas(ActivateDarkmodeEvent darkmodeEvent)
        {
            darkmodeCanvasGo.SetActive(darkmodeEvent.Value);
        }

        void HideTutorialElements(LoadTutorialEvent _)
        {
            EnableTimer(false);
        }

        void ShowTutorialElements(EndTutorialEvent _)
        {
            EnableTimer(true);
        }
    }

    private void EnableTimer(bool value)
    {
        timerText.gameObject.SetActive(value);
    }

    public void EnablePauseMenu()
    {
        //_magicLinesData.FinishLine();
        sorcererController.enabled = false;
        _magicLinesData.enabled = false;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        EventManager.Trigger(new PauseGameEvent(true));
    }
    
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        //levelMenu.SetActive(false);
        Time.timeScale = 1f;
        sorcererController.enabled = true;
        _magicLinesData.enabled = true;
        EventManager.Trigger(new PauseGameEvent(false));
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        ResumeGame();
        EventManager.Trigger(new ExitLevelEvent());
    }
}

public class ExitLevelEvent
{
    public ExitLevelEvent()
    {
        
    }
}

public class PauseGameEvent
{
    public bool Value { get; }

    public PauseGameEvent(bool value)
    {
        Value = value;
    }
}