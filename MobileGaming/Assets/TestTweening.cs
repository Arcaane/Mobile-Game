using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TestTweening : MonoBehaviour
{
    [SerializeField] Transform goldPile;

    private Vector3[] initPos;
    private Quaternion[] initRot;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < goldPile.childCount; i++)
        {
            initPos[i] = goldPile.GetChild(i).position;
            initRot[i] = goldPile.GetChild(i).rotation;
        }
    }

    [ContextMenu("OUGAA")]
    public void Button(int i)
    {
        Reset();
 
        float delay = 0;
        goldPile.gameObject.SetActive(true);
        
        for (int j = 0; j < goldPile.childCount; j++)
        {
            goldPile.GetChild(i).DOScale(1f, 0.335f).SetDelay(delay).SetEase(Ease.OutBack);
            goldPile.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-103, 349), 1.2f).SetDelay(delay + 0.5f).SetEase(Ease.OutBack);
            goldPile.GetChild(i).DOScale(0, 0.3f).SetDelay(delay + 1f).SetEase(Ease.OutBack);
            delay += 0.2f;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < goldPile.childCount; i++)
        {
            goldPile.GetChild(i).position = initPos[i];
            goldPile.GetChild(i).rotation = initRot[i];
        }
    }
}
