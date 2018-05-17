using UnityEngine;
using UnityEngine.UI;
using SleekRender;

public class DisplayFPS : MonoBehaviour 
{
	public float updateInterval = 0.5f;
	public Text text;

	private SleekRenderPostProcess _pp;

	private float _secondsSinceLastDisplay;
	private int _countedFrames;
	private string[] _digitsCache;

	private void Start()
	{
		_pp = FindObjectOfType<SleekRenderPostProcess>();
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
			fps = Mathf.Min(60, fps);
			fps = Mathf.Max(0, fps);
			text.text = _digitsCache[fps];

			_secondsSinceLastDisplay = 0f;
			_countedFrames = 0;
		}
	}

	public void OnBloomToggle(bool isOn)
	{
		_pp.settings.bloomEnabled = isOn;
	}

	public void OnColorizeToggle(bool isOn)
	{
		_pp.settings.colorizeEnabled = isOn;
	}

	public void OnVignetteToggle(bool isOn)
	{
		_pp.settings.vignetteEnabled = isOn;
	}

	public void OnContrastAndBrightness(bool isOn)
	{
		_pp.settings.brightnessContrastEnabled = isOn;
	}

	public void AllToggle(bool isOn)
	{
		_pp.enabled = isOn;
	}
}
