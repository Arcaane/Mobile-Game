using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject darkmodeCanvasGo;
    [SerializeField] private Canvas darkmodeCanvas;
    
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Image[] starsHolder;
    [SerializeField] private Sprite starFill;
    [SerializeField] private Sprite starEpmty;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private RectTransform chroneNeedleTr;
    [SerializeField] private Vector3 needleRotation = new Vector3(0, 0, 10f);
    
    //[SerializeField] private GameObject pauseMenu;

    private SorcererController sorcererController;
    private MagicLinesData _magicLinesData;

    private Slider scoreSlider;
    private TextMeshProUGUI timerText;

    private void Start()
    {
        sorcererController = GetComponent<SorcererController>();
        _magicLinesData = GetComponent<MagicLinesData>();
        timerText = sorcererController.timeLeftText;
        scoreSlider = sorcererController.scoreSlider;
        darkmodeCanvasGo.SetActive(false);

        HideHud();
        
        EventManager.AddListener<LoadLevelEvent>(HideHudOnLevelInit);
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
            
            HideHud();
        }
        
        void UpdateScore(LevelScoreUpdatedEvent scoreUpdatedEvent)
        {
            scoreSlider.value = ((float)scoreUpdatedEvent.Score) / scoreUpdatedEvent.Palier3;
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