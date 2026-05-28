using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class DisableEffectsOnLowQuality : MonoBehaviour
{
	private DepthTextureMode lastDepthMode;

	public void Awake()
	{
		CheckQuality();
	}

	public void Update()
	{
		CheckQuality();
	}

	private void CheckQuality()
	{
		SSAOPro component = GetComponent<SSAOPro>();
		if (component.enabled != SRQualitySettings.AmbientOcclusion)
		{
			component.enabled = SRQualitySettings.AmbientOcclusion;
		}
		Bloom component2 = GetComponent<Bloom>();
		if (component2.enabled != SRQualitySettings.Bloom)
		{
			component2.enabled = SRQualitySettings.Bloom;
		}
		DepthTextureMode depthTextureMode = SRQualitySettings.GetDepthTextureMode();
		if (depthTextureMode != lastDepthMode)
		{
			GetComponent<Camera>().depthTextureMode = depthTextureMode;
			lastDepthMode = depthTextureMode;
		}
	}
}
