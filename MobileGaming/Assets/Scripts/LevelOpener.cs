using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelOpener : MonoBehaviour
{
    public ScriptableLevelInSagaMap levelScriptable;
    
    public TextMeshProUGUI levelText;

    public int starsClaimedCount;
    
    [FormerlySerializedAs("isLevelLock")] public bool isLevelUnlock;

    public GameObject[] stars;
    
    private void Start()
    {
        if (levelScriptable == null) return; 
        
        levelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        levelText.text = $"{levelScriptable.currentLevel}";

        if (!isLevelUnlock)
        {
            GetComponent<Image>().sprite = isLevelUnlock ? GamePathManager.instance.boutonSprite[1] : GamePathManager.instance.boutonSprite[0];
            GetComponent<Button>().enabled = isLevelUnlock;
        }
        
        if (!isLevelUnlock) return;
        
        for (int i = 1; i < starsClaimedCount; i++)
        {
            stars[i-1].SetActive(true);
        }
    }

    public void LevelSelectionBuild()
    {
       GamePathManager.instance.levelSelection.BuildUI(levelScriptable);
       GamePathManager.instance.preScreenLevel.BuildUI(levelScriptable);
    }
}
