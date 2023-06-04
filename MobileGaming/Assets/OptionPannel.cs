using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OptionPannel : MonoBehaviour
{
    private void OnEnable()
    {
        transform.DOScale(0, 0);
        transform.DOScale(1.25f, 0.15f).SetDelay(0.1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            transform.DOScale(1f, 0.1f);
        });
    }

    private void OnDisable()
    {
        transform.DOScale(0, 0);
    }
}
