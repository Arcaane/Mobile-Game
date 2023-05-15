using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPreScreenContentHolder : MonoBehaviour
{
    // Images
    public Image sectionBackgroundImage;
    public Image levelItemToDisplayImage;
    public Image levelItemRewardImage;
    
    // Texts
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI upperSentenceText;
    public TextMeshProUGUI lowerSentenceText;

    private int currentLevelToLaunch;
    [SerializeField] private MainMenuManager mainMenu;
    
    public void LaunchLevel()
    {
        mainMenu.PlaceHolderGoToGameScene(currentLevelToLaunch);
    }
    
    public void BuildUI(ScriptableLevelInSagaMap _scriptableObject)
    {
        levelTitleText.text = _scriptableObject.title;
        upperSentenceText.text = _scriptableObject.areaText1;
        lowerSentenceText.text = _scriptableObject.areaText2;
        
        //sectionBackgroundImage.sprite = _scriptableObject.preScreenLevelBackground;
        levelItemToDisplayImage.sprite = _scriptableObject.potionToUseSprite;
        levelItemRewardImage.sprite = _scriptableObject.fragementReward;

        currentLevelToLaunch = _scriptableObject.currentLevel;  
    }
}
