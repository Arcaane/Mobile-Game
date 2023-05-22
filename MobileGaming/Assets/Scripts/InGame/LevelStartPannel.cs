using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartPannel : MonoBehaviour
{
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

    public void UpdateValues(Level level)
    {
        //SorcererController.Instance.hudCanvasGO.SetActive(false);
        //SorcererController.Instance.menuCanvasGO.SetActive(false);
        
        Time.timeScale = 0f;
        
        Debug.Log($"Chapter : {level.currentChapter}, level {level.currentLevel}");
        chapterNumberText.text = $"Chapter {level.currentChapter}";
        levelNumberText.text = $"Level {level.currentLevel}";
        
        var time = TimeSpan.FromSeconds(level.levelDuration);
        timerText.text = time.ToString(@"mm\:ss");
        
        oneStarNumberText.text = $"{level.scoreToWin}";
        twoStarNumberText.text = $"{level.palier2}";
        threeStarNumberText.text = $"{level.palier3}";
    }

    public void Show()
    {
        var mySequence = DOTween.Sequence();
        mySequence.Append(panel.DOLocalMoveY(385, inDuration));
        mySequence.AppendInterval(viewDuration);
        mySequence.Append(panel.DOLocalMoveY(1003, outDuration));
        //mySequence.AppendCallback(() => level.canRun = true);
        //mySequence.AppendCallback(() => level.Run());
        mySequence.AppendCallback(() => SorcererController.Instance.hudCanvasGO.SetActive(true));
        mySequence.AppendCallback(() => gameObject.SetActive(false));

        mySequence.Play().SetUpdate(true);
    }
}
