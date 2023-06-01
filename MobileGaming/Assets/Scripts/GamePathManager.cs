using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePathManager : MonoBehaviour
{
    private static GamePathManager Instance;

    public static GamePathManager instance
    {
        get => Instance;
        set => Instance = value;
    }

    public LevelSelectionContentHolder levelSelection;
    public LevelPreScreenContentHolder preScreenLevel;

    public LevelOpener[] levels;
    public int unlockedLevels = 1;

    public Sprite[] boutonSprite;
    Dictionary<string, int> progressionSave = new();
    

    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateBoutonsColor();

        if (!PlayerPrefs.HasKey("LevelUnlocked"))
            PlayerPrefs.SetInt("LevelUnlocked", 0);
        
        
        // Init progressionSave dico
        for (int i = 0; i < levels.Length; i++)
        {
            if (!PlayerPrefs.HasKey(levels[i].name)) PlayerPrefs.SetInt(levels[i].name, 0);
            
            progressionSave.Add(levels[i].name, PlayerPrefs.GetInt(levels[i].name));
            levels[i].starsClaimedCount = PlayerPrefs.GetInt(levels[i].name);
        }

        unlockedLevels = PlayerPrefs.GetInt("LevelUnlocked");
    }
    
    public void UpdateBoutonsColor()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].GetComponent<Image>().sprite = levels[i].isLevelUnlock ? boutonSprite[1] : boutonSprite[0];
        }
    }

    public void SaveLevel(int level)
    {
        if (levels[level] == null) return;
        PlayerPrefs.SetInt(levels[level].name, levels[level].starsClaimedCount);
        PlayerPrefs.SetInt("LevelUnlocked", unlockedLevels);
    }
}