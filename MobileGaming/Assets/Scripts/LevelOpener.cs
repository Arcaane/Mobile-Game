using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelOpener : MonoBehaviour
{
    public ScriptableLevelInSagaMap levelScriptable;
    
    public TextMeshProUGUI levelText;

    public bool isLevelLock;
    
    private void Start()
    {
        if (levelScriptable == null) return; 
        levelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        levelText.text = $"{levelScriptable.currentLevel}";

        if (!isLevelLock)
        {
            GetComponent<Image>().sprite = isLevelLock ? GamePathManager.instance.boutonSprite[1] : GamePathManager.instance.boutonSprite[0];
            GetComponent<Button>().enabled = isLevelLock;
        }
    }
    
    public void LevelSelectionBuild()
    {
       GamePathManager.instance.levelSelection.BuildUI(levelScriptable);
       GamePathManager.instance.preScreenLevel.BuildUI(levelScriptable);
    }
}
