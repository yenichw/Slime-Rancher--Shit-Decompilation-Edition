using UnityEngine;

public class ViewUnderwaterTrigger : MonoBehaviour
{
	private AmbianceDirector ambianceDir;

	private vp_FPPlayerEventHandler playerEvents;

	public void Awake()
	{
		ambianceDir = SRSingleton<SceneContext>.Instance.AmbianceDirector;
		playerEvents = GetComponentInParent<vp_FPPlayerEventHandler>();
	}

	public void OnTriggerEnter(Collider col)
	{
		LiquidSource component = col.GetComponent<LiquidSource>();
		if (component != null && component.CountsAsUnderwater())
		{
			ambianceDir.EnterWater();
			playerEvents.Underwater.TryStart();
		}
		if ((bool)col.GetComponent<JellySea>())
		{
			ambianceDir.EnterSea();
		}
	}

	public void OnTriggerExit(Collider col)
	{
		LiquidSource component = col.GetComponent<LiquidSource>();
		if (component != null && component.CountsAsUnderwater())
		{
			ambianceDir.ExitWater();
			playerEvents.Underwater.TryStop();
		}
		if ((bool)col.GetComponent<JellySea>())
		{
			ambianceDir.ExitSea();
		}
	}
}
