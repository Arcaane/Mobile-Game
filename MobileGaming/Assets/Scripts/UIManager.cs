using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelMenu;
    [SerializeField] private GameObject darkmodeCanvasGo;
    [SerializeField] private Canvas darkmodeCanvas;
    [SerializeField] private Canvas hudCanvas;
    
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
            
            HideHud();
        }
        
        void UpdateScore(LevelScoreUpdatedEvent scoreUpdatedEvent)
        {
            scoreSlider.value = ((float)scoreUpdatedEvent.Score) / scoreUpdatedEvent.Palier3;
        }

        void UpdateTime(LevelTimeUpdatedEvent timeUpdatedEvent)
        {
            var time = timeUpdatedEvent.MaxTime - timeUpdatedEvent.CurrentTime;
            timerText.text = $"Time Left : {(time >= 0 ? time : "Extra time !"):f0}";
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

    public void EnableTimer(bool value)
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
    }

    public void EnableInfoMenu()
    {
        //_magicLinesData.FinishLine();
        sorcererController.enabled = false;
        _magicLinesData.enabled = false;
        levelMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        levelMenu.SetActive(false);
        Time.timeScale = 1f;
        sorcererController.enabled = false;
        _magicLinesData.enabled = false;
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(0);
    }
}