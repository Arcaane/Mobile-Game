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
        initPos = new Vector3[goldPile.childCount];
        initRot = new Quaternion[goldPile.childCount];

        for (int i = 0; i < goldPile.childCount; i++)
        {
            initPos[i] = goldPile.GetChild(i).transform.position;
            initRot[i] = goldPile.GetChild(i).transform.rotation;
        }
    }

    [ContextMenu("OUGAA")]
    public void Button()
    {
        Reset();
 
        float delay = 0;

        for (int j = 0; j < goldPile.childCount; j++)
        {
            var objectTween = goldPile.GetChild(j);
            objectTween.DOScale(1f, 0.3f).SetDelay(delay).SetEase(Ease.OutBack);
            objectTween.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-154.5f, 349), 0.8f).SetDelay(delay + 0.5f).SetEase(Ease.InBack);
            objectTween.DORotate(Vector3.zero, 0.5f).SetDelay(delay + 0.5f).SetEase(Ease.Flash);
            objectTween.DOScale(0, 0.3f).SetDelay(delay + 1.8f).SetEase(Ease.OutBack);
            delay += 0.1f;
        }

        StartCoroutine(GetGoldText(10));
    }

    public void Reset()
    {
        for (int i = 0; i < goldPile.childCount; i++) 
        {
            goldPile.GetChild(i).transform.position = initPos[i];
            goldPile.GetChild(i).transform.rotation = initRot[i];
        }
    }

    IEnumerator GetGoldText(int coinCount)
    {
        yield return new WaitForSecondsRealtime(0.8f);

        var timer = 0.05f;
        for (int i = 0; i < coinCount; i++)
        {
            yield return new WaitForSecondsRealtime(timer);
        }
    }
}
