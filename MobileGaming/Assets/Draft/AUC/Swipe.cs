using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Swipe : MonoBehaviour
{
    public Scrollbar scrollbar;
    public float scrollPos;
    public float[] pos;
    public float dst;
    public bool isManually;
    public Slider pointer;

    public RectTransform[] icon;
    
    // Start is called before the first frame update
    void Start()
    {
        pos = new float[transform.childCount];
        dst = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = dst * i;
        }
        
        scrollPos = 0.5f;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && !isManually)
        {
            scrollPos = scrollbar.value;
        }
        else
        {
            for (var i = 0; i < pos.Length; i++)
            {
                if (scrollPos < pos[i] + (dst/2) && scrollPos > pos[i] - (dst/2))
                {
                    scrollbar.value = Mathf.Lerp(scrollbar.value, pos[i], 0.075f);
                    pointer.value = scrollbar.value;
                }
            }
        }
    }

    public void SwipeManually(float value)
    {
        var i = 0;
        switch (value)
        {
            case 0: i = 0; break;
            case 0.25f: i = 1; break;
            case 0.5f: i = 2; break;
            case 0.75f: i = 3; break;
            case 1f: i = 4; break;
        }
        
        StartCoroutine(SwipeTask(value,i));
    }
    
    private IEnumerator SwipeTask(float value, int pannel)
    {
        isManually = true;
        scrollPos = value;
        yield return new WaitForSeconds(0.1f);
        isManually = false;

        pointer.value = value;
        icon[pannel].transform.DOScale(1.25f, 0.125f);
        icon[pannel].transform.DOLocalMoveY(10, 0.15f);

        for (int i = 0; i < icon.Length; i++)
        {
            if (icon[i] == icon[pannel]) continue;
           
            icon[i].localPosition = Vector2.zero;
            icon[i].localScale = Vector3.one;
        }
    }
}
