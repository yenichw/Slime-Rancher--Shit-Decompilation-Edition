using System.Collections;
using UnityEngine;

public class GlitchTarrNodeDamage : DamagePlayerOnTouch_Trigger
{
	private bool hasCheckedFirstCollision;

	public override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(WaitForFixedUpdate());
	}

	public override void RegistryUpdate()
	{
		if (hasCheckedFirstCollision)
		{
			base.RegistryUpdate();
		}
	}

	private IEnumerator WaitForFixedUpdate()
	{
		hasCheckedFirstCollision = false;
		yield return new WaitForFixedUpdate();
		hasCheckedFirstCollision = true;
		if (damageGameObject != null)
		{
			nextTime = SRSingleton<SceneContext>.Instance.TimeDirector.HoursFromNow(SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch.tarrNodeSpawnDamagePreventionTime * (1f / 60f));
		}
	}
}
