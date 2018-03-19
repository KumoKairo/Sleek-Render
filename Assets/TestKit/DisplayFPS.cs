using UnityEngine;
using UnityEngine.UI;

public class DisplayFPS : MonoBehaviour 
{
	public float updateInterval = 0.5f;
	private float _secondsSinceLastDisplay;
	private int _countedFrames;

	private string[] _digitsCache;

	private Text _text;

	private void Start()
	{
		var canvasGo = new GameObject("FpsCanvas");
		var canvas = canvasGo.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		var scaler = canvasGo.AddComponent<CanvasScaler>();
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

		var textGo = new GameObject("FpsText");
		textGo.transform.SetParent(canvasGo.transform, false);
		_text = textGo.AddComponent<Text>();
		_text.font = Resources.Load<Font>("Roboto-Black");
		_text.alignment = TextAnchor.LowerRight;
		_text.text = "--";
		var textRect = _text.transform as RectTransform;
		textRect.pivot = new Vector2(1f, 0f);
		textRect.anchorMax = new Vector2(1f, 0f);
		textRect.anchorMin = new Vector2(1f, 0f);
		textRect.anchoredPosition = new Vector2(-8f, 8f);

		_digitsCache = new string[61];
		for(int i = 0; i < _digitsCache.Length; i++)
		{
			_digitsCache[i] = i.ToString("D2");
		}
	}

	private void Update()
	{
		_countedFrames++;
		_secondsSinceLastDisplay += Time.deltaTime;

		if(_secondsSinceLastDisplay > updateInterval)
		{
			int fps = Mathf.RoundToInt(_countedFrames / _secondsSinceLastDisplay);
			Debug.Log(fps);
			fps = Mathf.Min(60, fps);
			fps = Mathf.Max(0, fps);
			_text.text = _digitsCache[fps + 1];

			_secondsSinceLastDisplay = 0f;
			_countedFrames = 0;
		}
	}
}
