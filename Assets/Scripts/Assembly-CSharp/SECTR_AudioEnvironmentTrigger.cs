using UnityEngine;

[AddComponentMenu("SECTR/Audio/SECTR Audio Environment Trigger")]
public class SECTR_AudioEnvironmentTrigger : SECTR_AudioEnvironment
{
	private Collider activator;

	private void OnEnable()
	{
		if ((bool)activator)
		{
			Activate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (activator == null)
		{
			Activate();
			activator = other;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (activator == other)
		{
			Deactivate();
			activator = null;
		}
	}
}
