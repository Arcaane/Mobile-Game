using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelOpener : MonoBehaviour
{
    public ScriptableLevelInSagaMap levelScriptable;
    
    public TextMeshProUGUI levelText;

    public bool isLevelLock;
    
    private void OnEnable()
    {
        if (levelScriptable == null) return; 
        levelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        levelText.text = $"Level: {levelScriptable.currentLevel}";
    }

    public void AccessLevel()
    {
        if (isLevelLock) return;
        SceneManager.LoadScene("TestScene");
    }
    
    public void LevelSelectionBuild()
    {
       GamePathManager.instance.levelSelection.BuildUI(levelScriptable);
       GamePathManager.instance.preScreenLevel.BuildUI(levelScriptable);
    }
}
