using System.Linq;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    
    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;
    private static FpsCounter instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        frameDeltaTimeArray = new float[50];
    }
    
#if DEBUG
    private void Update()
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
#endif
}
