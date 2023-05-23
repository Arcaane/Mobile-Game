using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    
    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        frameDeltaTimeArray = new float[50];
    }
    
    // Update is called once per frame
    void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = Mathf.RoundToInt(CalculationFPS()).ToString();
    }

    private float CalculationFPS()
    {
        var total = frameDeltaTimeArray.Sum();

        return frameDeltaTimeArray.Length / total;
    }
}
