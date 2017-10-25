using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    public Text text;

    float deltaTime = 0.0f;

    private string[] numbers = new string[61];

    void Start()
    {
        Application.targetFrameRate = 60;
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i.ToString();
        }
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        float fps = 1.0f / deltaTime;
        var intFps = Mathf.RoundToInt(fps);
        intFps = Mathf.Clamp(intFps, 0, 60);
        text.text = numbers[intFps];
    }
}