using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelMenu;
    
    //[SerializeField] private GameObject pauseMenu;

    private SorcererController sorcererController;
    private MagicLinesData _magicLinesData;

    private void Start()
    {
        sorcererController = GetComponent<SorcererController>();
        _magicLinesData = GetComponent<MagicLinesData>();
        
        EventManager.AddListener<LoadLevelEvent>(HideHudOnLevelInit);

        void HideHudOnLevelInit(LoadLevelEvent _)
        {
            sorcererController.hudCanvasGO.SetActive(false);
            sorcererController.menuCanvasGO.SetActive(false);
        }
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