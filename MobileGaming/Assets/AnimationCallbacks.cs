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
        
        SorcererController.Instance.hudCanvasGO.SetActive(true);
        
        Time.timeScale = 1f;
    }
}
