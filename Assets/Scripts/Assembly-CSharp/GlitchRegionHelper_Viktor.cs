using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GlitchRegionHelper_Viktor : SRSingleton<GlitchRegionHelper_Viktor>, AmbianceDirector.TimeOfDay
{
	[Tooltip("Reference to the GlitchTerminalActivator in the scene.")]
	public GlitchTerminalActivator activator;

	public override void Awake()
	{
		base.Awake();
		SRSingleton<SceneContext>.Instance.RegionRegistry.ManageWithRegionSet(base.gameObject, RegionRegistry.RegionSetId.VIKTOR_LAB);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.RegionRegistry != null)
		{
			SRSingleton<SceneContext>.Instance.RegionRegistry.ReleaseFromRegionSet(base.gameObject, RegionRegistry.RegionSetId.VIKTOR_LAB);
		}
	}

	public void OnEnable()
	{
		SRSingleton<SceneContext>.Instance.AmbianceDirector.Register(this);
	}

	public void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.AmbianceDirector != null)
		{
			SRSingleton<SceneContext>.Instance.AmbianceDirector.Deregister(this);
		}
	}

	public float GetCurrentDayFraction_Position()
	{
		return SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.ambianceTimeOfDay;
	}

	public float GetCurrentDayFraction_Color()
	{
		return SRSingleton<SceneContext>.Instance.TimeDirector.CurrDayFraction();
	}
}
