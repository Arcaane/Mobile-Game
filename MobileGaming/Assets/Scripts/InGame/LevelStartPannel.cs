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

    public event Action OnAnimationOver;
    
    public void UpdateValues(Level level)
    {
        Time.timeScale = 0f;
        
        Debug.Log($"Chapter : {level.currentChapter}, level {level.currentLevel}");
        chapterNumberText.text = $"Chapter {level.currentChapter}";
        levelNumberText.text = $"Level {level.currentLevel}";
        
        var time = TimeSpan.FromSeconds(level.levelDuration);
        timerText.text = time.ToString(@"mm\:ss");
        
        oneStarNumberText.text = $"{level.scoreToWin}";
        twoStarNumberText.text = $"{level.palier2}";
        threeStarNumberText.text = $"{level.palier3}";
        
        Show();
    }

    private void Show()
    {
        var moveDownSequence = DOTween.Sequence();
        moveDownSequence.Append(panel.DOLocalMoveY(385, inDuration));
        moveDownSequence.AppendCallback(AllowSkip);
        moveDownSequence.AppendInterval(viewDuration);
        moveDownSequence.AppendCallback(RemoveSkipAndMoveUp);
        
        moveDownSequence.Play().SetUpdate(true);

        void AllowSkip()
        {
            InputService.OnPress += SkipSequence;
        }
        
        void RemoveSkipAndMoveUp()
        {
            InputService.OnPress -= SkipSequence;

            var moveUpAnimation = DOTween.Sequence();
            moveUpAnimation.Append(panel.DOLocalMoveY(1003, outDuration));
            moveUpAnimation.AppendCallback(EndAnimation);
            moveUpAnimation.Play().SetUpdate(true);
        }
        
        void SkipSequence(Vector2 _)
        {
            if(!moveDownSequence.IsPlaying()) return;
            moveDownSequence.Complete(true);
            RemoveSkipAndMoveUp();
        }

        void EndAnimation()
        {
            OnAnimationOver?.Invoke();
            gameObject.SetActive(false);
        }
    }
}

