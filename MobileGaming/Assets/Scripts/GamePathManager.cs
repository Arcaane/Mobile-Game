using System;
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
    }

    public void UpdateBoutonsColor()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].GetComponent<Image>().sprite = levels[i].isLevelLock ? boutonSprite[1] : boutonSprite[0];
        }
        
    }
}

