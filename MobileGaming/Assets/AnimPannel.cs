using System;
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
    }

    private void Update()
    {
        if (isAnimPlayed) return; 
        isFullyVisible = anchor.IsFullyVisibleFrom();
        
        if (isFullyVisible)
        {
            transform.DOScale(1.15f, 0.35f).SetDelay(0.1f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                transform.DOScale(1, 0.185f);
            });

            isAnimPlayed = true;
        }
    }

    private void OnDisable()
    {
        isAnimPlayed = false;
        transform.DOScale(0, 0.01f);
    }
}
