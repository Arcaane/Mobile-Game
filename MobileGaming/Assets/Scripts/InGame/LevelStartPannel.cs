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
        Debug.Log($"Chapter : {level.LevelScriptable.CurrentChapter}, level {level.LevelScriptable.CurrentLevel}");
        chapterNumberText.text = $"Chapter {level.LevelScriptable.CurrentChapter}";
        levelNumberText.text = $"Level {level.LevelScriptable.CurrentLevel}";
        
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
        moveDownSequence.AppendCallback(RemoveSkip);
        moveDownSequence.AppendCallback(MoveUp);
        
        moveDownSequence.Play().SetUpdate(true);

        void AllowSkip()
        {
            InputService.OnRelease += SkipSequence;
        }

        void RemoveSkip()
        {
            InputService.OnRelease -= SkipSequence;
        }
        
        void SkipSequence(Vector2 _)
        {
            InputService.OnRelease -= SkipSequence;
            
            if(!moveDownSequence.IsPlaying()) return;
            moveDownSequence.Complete(true);
        }

        void MoveUp()
        {
            var moveUpAnimation = DOTween.Sequence();
            moveUpAnimation.Append(panel.DOLocalMoveY(1003, outDuration));
            moveUpAnimation.AppendCallback(EndAnimation);
            moveUpAnimation.Play().SetUpdate(true);
        }

        void EndAnimation()
        {
            OnAnimationOver?.Invoke();
            gameObject.SetActive(false);
        }
    }
}

