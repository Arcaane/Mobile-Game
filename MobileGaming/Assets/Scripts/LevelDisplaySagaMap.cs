using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelDisplaySagaMap : MonoBehaviour
{
    [field:Header("Components")]
    [field:SerializeField] public ScriptableLevelInSagaMap LevelScriptable { get; private set; }
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    [Header("Config")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    public LevelDisplaySagaMap nextLevel;
    public LevelDisplaySagaMap NextLevel => nextLevel;
    
    [SerializeField] private GameObject[] starsBack;
    public GameObject[] stars;
    
    private void Start()
    {
        if (LevelScriptable == null) return; 
        if (LevelScriptable.Fake) return; 
        
        levelText.text = $"{LevelScriptable.CurrentLevel}";
        
        button.onClick.AddListener(ButtonClick);
    }
    
    public void UnlockLevel(bool value)
    {
        button.interactable = value;
        image.sprite = value ? unlockedSprite : lockedSprite;

        foreach (var go in starsBack)
        {
            go.SetActive(value);
        }
        
        if (LevelScriptable == null) return; 
        LevelScriptable.UnlockLevel(value);
        LevelScriptable.GetProgress();
        if(LevelScriptable.Fake) button.interactable = false;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive((value && i < LevelScriptable.Stars) || LevelScriptable.Fake);
        }
    }
    
    private void ButtonClick()
    {
        EventManager.Trigger(new OpenLevelSagaMapEvent(LevelScriptable));
    }
}

public class OpenLevelSagaMapEvent
{
    public ScriptableLevelInSagaMap ScriptableLevelInSagaMap { get; }

    public OpenLevelSagaMapEvent(ScriptableLevelInSagaMap scriptableLevelInSagaMap)
    {
        ScriptableLevelInSagaMap = scriptableLevelInSagaMap;
    }
}
