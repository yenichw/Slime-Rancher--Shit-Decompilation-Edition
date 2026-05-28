using System.Collections.Generic;
using UnityEngine;

public class CaveLightController : MonoBehaviour
{
	private Light controlledLight;

	private float defaultIntensity;

	private Dictionary<CaveTrigger, float> triggernessVals = new Dictionary<CaveTrigger, float>();

	public void Awake()
	{
		controlledLight = GetComponent<Light>();
		defaultIntensity = controlledLight.intensity;
	}

	public void SetTriggerness(CaveTrigger trigger, float triggerness)
	{
		triggernessVals[trigger] = triggerness;
	}

	public void Update()
	{
		float num = 0f;
		foreach (KeyValuePair<CaveTrigger, float> triggernessVal in triggernessVals)
		{
			if (triggernessVal.Key != null && triggernessVal.Key.enabled)
			{
				num = Mathf.Max(num, triggernessVal.Value);
			}
		}
		controlledLight.intensity = defaultIntensity * num;
		controlledLight.enabled = num > 0f;
	}
}
