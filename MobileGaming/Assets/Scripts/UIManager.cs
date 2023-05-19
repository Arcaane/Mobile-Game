using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelMenu;
    
    //[SerializeField] private GameObject pauseMenu;

    private SorcererController sorcererController;
    private MagicLinesManager magicLinesManager;

    private void Start()
    {
        sorcererController = GetComponent<SorcererController>();
        magicLinesManager = GetComponent<MagicLinesManager>();
    }

    public void EnablePauseMenu()
    {
        magicLinesManager.FinishLine();
        sorcererController.enabled = false;
        magicLinesManager.enabled = false;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void EnableInfoMenu()
    {
        magicLinesManager.FinishLine();
        sorcererController.enabled = false;
        magicLinesManager.enabled = false;
        levelMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        levelMenu.SetActive(false);
        Time.timeScale = 1f;
        sorcererController.enabled = false;
        magicLinesManager.enabled = false;
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