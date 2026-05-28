using UnityEngine;

public class VacDelaunchTrigger : MonoBehaviour
{
	private Vacuumable vacuumable;

	public void Start()
	{
		vacuumable = GetComponentInParent<Vacuumable>();
	}

	public void SetTriggerEnabled(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}

	public void Delaunch()
	{
		if (vacuumable == null)
		{
			vacuumable = GetComponentInParent<Vacuumable>();
		}
		if (vacuumable != null && vacuumable.delaunch())
		{
			Identifiable component = vacuumable.GetComponent<Identifiable>();
			if (component != null && Identifiable.IsSlime(component.id))
			{
				SRSingleton<SceneContext>.Instance.TutorialDirector.OnDelaunchedSlime();
			}
		}
	}
}
