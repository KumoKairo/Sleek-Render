using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour, IPointerClickHandler
{
    public Text text;

    private const float UPDATE_EVERY = 0.3f;

    private float _accumulatedTime = 0f;
    private int _accumulatedFrames = 0;

    private string[] numbers = new string[61];

    void Start()
    {
        Application.targetFrameRate = 60;
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i.ToString();
        }

        _accumulatedTime = 0f;
        _accumulatedFrames = 0;
    }

    void Update()
    {
        _accumulatedTime += Time.deltaTime;
        _accumulatedFrames++;

        if (_accumulatedTime > UPDATE_EVERY)
        {
            float fps = _accumulatedFrames / _accumulatedTime;
            var intFps = Mathf.RoundToInt(fps);
            intFps = Mathf.Clamp(intFps, 0, 60);
            text.text = numbers[intFps];

            _accumulatedTime = 0f;
            _accumulatedFrames = 0;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        text.enabled = !text.enabled;
    }
}