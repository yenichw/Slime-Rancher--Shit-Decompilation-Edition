using UnityEngine;

public class DeactivateOnPediaLocked : SRBehaviour
{
	[Tooltip("Pedia entry that is required to be unlocked.")]
	public PediaDirector.Id id;

	public void OnEnable()
	{
		if (!SRSingleton<SceneContext>.Instance.PediaDirector.IsUnlocked(id))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
