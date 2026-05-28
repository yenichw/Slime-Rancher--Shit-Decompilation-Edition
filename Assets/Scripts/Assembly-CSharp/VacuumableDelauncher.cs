using UnityEngine;

public class VacuumableDelauncher : MonoBehaviour
{
	public void OnTriggerEnter(Collider col)
	{
		VacDelaunchTrigger component = col.gameObject.GetComponent<VacDelaunchTrigger>();
		if (component != null)
		{
			component.Delaunch();
		}
	}
}
