using System;
using System.Collections;
using System.Collections.Generic;
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
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = Mathf.RoundToInt(CalculationFPS()).ToString();
    }

    private float CalculationFPS()
    {
        float total = 0f;
        foreach (var deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }

        return frameDeltaTimeArray.Length / total;
    }
}
