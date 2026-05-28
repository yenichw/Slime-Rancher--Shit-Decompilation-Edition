using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFog : MonoBehaviour
{
	public Color fogColor;

	public FogMode fogMode;

	[Header("Linear Mode Properties")]
	public float fogEndDistance = 6f;

	public float fogStartDistance = 3f;

	[Header("Exponential Mode Properties")]
	public float fogDensity = 100f;

	private bool revertFogState;

	private Color revertFogColor;

	private float revertFogDensity;

	private FogMode revertFogMode;

	private float revertFogStart;

	private float revertFogEnd;

	private void OnPreRender()
	{
		revertFogState = RenderSettings.fog;
		revertFogColor = RenderSettings.fogColor;
		revertFogDensity = RenderSettings.fogDensity;
		revertFogMode = RenderSettings.fogMode;
		revertFogStart = RenderSettings.fogStartDistance;
		revertFogEnd = RenderSettings.fogEndDistance;
		RenderSettings.fog = true;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogMode = fogMode;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.fogEndDistance = fogEndDistance;
	}

	private void OnPostRender()
	{
		RenderSettings.fog = revertFogState;
		RenderSettings.fogColor = revertFogColor;
		RenderSettings.fogDensity = revertFogDensity;
		RenderSettings.fogMode = revertFogMode;
		RenderSettings.fogStartDistance = revertFogStart;
		RenderSettings.fogEndDistance = revertFogEnd;
	}
}
