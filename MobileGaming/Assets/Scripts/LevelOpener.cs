using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelOpener : MonoBehaviour
{
    public ScriptableLevelInSagaMap levelScriptable;
    
    public TextMeshProUGUI levelText;
    public int levelIndex;

    public bool isLevelLock;
    
    private void OnEnable()
    {
        levelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        levelText.text = $"Level : {levelIndex}";
    }

    public void AccessLevel()
    {
        if (isLevelLock) return;
        SceneManager.LoadScene("TestScene");
    }

    public void BuildScriptable()
    {
        
    }
}
