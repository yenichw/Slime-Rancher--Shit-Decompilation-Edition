using UnityEngine;

public class FeralizeOnLargoTransformed : MonoBehaviour, OnTransformed
{
	public void OnTransformed()
	{
		Vacuumable component = GetComponent<Vacuumable>();
		SlimeFeral component2 = GetComponent<SlimeFeral>();
		if (component2 != null && component != null && component.size != 0)
		{
			component2.SetFeral();
		}
	}
}
