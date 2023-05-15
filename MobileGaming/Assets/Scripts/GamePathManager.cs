using UnityEngine;

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
    
    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }
}
