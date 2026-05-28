using System.Collections;
using UnityEngine;

public class PuzzleTeleportLock : PuzzleSlotLockable
{
	[Tooltip("The teleporter we control the activation on.")]
	public TeleportSource teleporter;

	public float activateDelay;

	public GameObject activateFX;

	public GameObject activateFXParent;

	public override void NotifySlotChanged(bool immediate = false)
	{
		if (ShouldUnlock())
		{
			if (immediate)
			{
				teleporter.ExternalActivate();
			}
			else
			{
				StartCoroutine(DelayedActivateSequence());
			}
		}
		base.NotifySlotChanged(immediate);
	}

	public IEnumerator DelayedActivateSequence()
	{
		SRBehaviour.InstantiateDynamic(activateFX, activateFXParent.transform.position, activateFXParent.transform.rotation);
		yield return new WaitForSeconds(activateDelay);
		teleporter.ExternalActivate();
	}
}
