using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Audio/SECTR Trigger Source")]
public class SECTR_TriggerSource : SECTR_PointSource
{
	private Collider activator;

	public SECTR_TriggerSource()
	{
		Loop = false;
		PlayOnStart = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!IsPlaying && (bool)activator)
		{
			Play();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (activator == null)
		{
			Play();
			activator = other;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (activator == other)
		{
			Stop(stopImmediately: false);
			activator = null;
		}
	}
}
