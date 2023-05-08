using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Swipe : MonoBehaviour
{
    public Scrollbar scrollbar;
    public float scrollPos;
    public float[] pos;
    public float dst;
    public bool isManually;
    
    // Start is called before the first frame update
    void Start()
    {
        pos = new float[transform.childCount];
        dst = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = dst * i;
        }
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
            for (int i = 0; i < pos.Length; i++)
            {
                if (scrollPos < pos[i] + (dst/2) && scrollPos > pos[i] - (dst/2))
                {
                    scrollbar.value = Mathf.Lerp(scrollbar.value, pos[i], 0.075f);
                }
            }
        }
    }

    public void SwipeManually(float value) => StartCoroutine(SwipeTask(value));
    
    private IEnumerator SwipeTask(float value)
    {
        isManually = true;
        scrollPos = value;
        yield return new WaitForSeconds(0.1f);
        isManually = false;
    }
}
