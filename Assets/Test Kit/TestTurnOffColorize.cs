using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTurnOffColorize : MonoBehaviour {

	public SleekRender.SleekRenderSettings settings;

	public void OnToggleChanged(bool newValue){
		settings.bloomEnabled = newValue;
	}
}
