using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartPannel : MonoBehaviour
{
    #region MyRegion
    [SerializeField] private Level myLevel;
    [SerializeField] private GameObject startLevelUI;
    [SerializeField] private TextMeshProUGUI chapterNumberText;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI oneStarNumberText;
    [SerializeField] private TextMeshProUGUI twoStarNumberText;
    [SerializeField] private TextMeshProUGUI threeStarNumberText;
    [SerializeField] private RectTransform panel;

    [SerializeField] private float inDuration;
    [SerializeField] private float outDuration;
    [SerializeField] private float viewDuration;

    [SerializeField] private Image[] starToActivate;
    
    private void Start()
    {
        SorcererController.Instance.hudCanvasGO.SetActive(false);
        SorcererController.Instance.menuCanvasGO.SetActive(false);
        
        Time.timeScale = 0f;
        
        chapterNumberText.text = $"Chapter {myLevel.currentChapter}";
        levelNumberText.text = $"Level {myLevel.currentLevel}";
        
        TimeSpan time = TimeSpan.FromSeconds(myLevel.levelDuration);
        timerText.text = time.ToString(@"mm\:ss");
        
        oneStarNumberText.text = $"{myLevel.scoreToWin}";
        twoStarNumberText.text = $"{myLevel.palier2}";
        threeStarNumberText.text = $"{myLevel.palier3}";

        var mySequence = DOTween.Sequence();
        mySequence.Append(panel.DOLocalMoveY(385, inDuration));
        mySequence.AppendInterval(viewDuration);
        mySequence.Append(panel.DOLocalMoveY(1003, outDuration));
        mySequence.AppendCallback(() => myLevel.canRun = true);
        mySequence.AppendCallback(() => myLevel.Run());
        mySequence.AppendCallback(() => SorcererController.Instance.hudCanvasGO.SetActive(true));
        mySequence.AppendCallback(() => gameObject.SetActive(false));

        mySequence.Play().SetUpdate(true);
    }
    
    #endregion
}
