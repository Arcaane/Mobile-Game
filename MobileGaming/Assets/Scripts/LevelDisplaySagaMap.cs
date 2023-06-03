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
    
    public bool isLevelUnlock;

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
        isLevelUnlock = value;
        
        button.interactable = isLevelUnlock;
        image.sprite = isLevelUnlock ? unlockedSprite : lockedSprite;

        foreach (var go in starsBack)
        {
            go.SetActive(isLevelUnlock);
        }
        
        if (LevelScriptable == null) return; 
        LevelScriptable.GetProgress();
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
