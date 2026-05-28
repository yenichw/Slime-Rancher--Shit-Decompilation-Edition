using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
	public SECTR_AudioCue flashlightOn;

	public SECTR_AudioCue flashlightOff;

	private Light activateLight;

	public void Awake()
	{
		activateLight = GetComponent<Light>();
	}

	public void Update()
	{
		if (Time.timeScale != 0f && SRInput.Actions.light.WasPressed)
		{
			activateLight.enabled = !activateLight.enabled;
			AnalyticsUtil.CustomEvent("FlashlightToggled", new Dictionary<string, object> { { "FlashlightState", activateLight.enabled } });
			SECTR_AudioSystem.Play(activateLight.enabled ? flashlightOn : flashlightOff, Vector3.zero, loop: false);
		}
	}
}
