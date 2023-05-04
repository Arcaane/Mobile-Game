using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCallbacks : MonoBehaviour
{
    public GameObject[] goToDestruct;
    
    public void Destroy()
    {
        foreach (var t in goToDestruct)
        {
            Destroy(t);
        }
        
        SorcererController.Instance.hudCanvasGO.SetActive(false);
        SorcererController.Instance.menuCanvasGO.SetActive(false);

        Time.timeScale = 1f;
    }
}
