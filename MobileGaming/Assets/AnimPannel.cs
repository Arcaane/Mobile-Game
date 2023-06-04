using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimPannel : MonoBehaviour
{
    public bool isAnimPlayed;
    private bool isFullyVisible;
    public RectTransform anchor;
    public Camera myCamera;
    
    private void Start()
    {
        isAnimPlayed = false;
        anchor = GetComponent<RectTransform>();
        myCamera = Camera.main;
        transform.DOScale(0, 0.1f);
    }

    private void Update()
    {
        if (isAnimPlayed) return; 
        isFullyVisible = anchor.IsFullyVisibleFrom();
        
        if (isFullyVisible)
        {
            transform.DOScale(1.25f, 0.15f).SetDelay(0.1f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                transform.DOScale(1f, 0.1f);
            });

            isAnimPlayed = true;
        }
    }
}
