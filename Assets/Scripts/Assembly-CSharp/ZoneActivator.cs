using UnityEngine;

public class ZoneActivator : MonoBehaviour, TechActivator
{
	public ReverseGravityZone zone;

	public void Activate()
	{
		zone.SetOperating(!zone.GetOperating());
	}

	public GameObject GetCustomGuiPrefab()
	{
		return null;
	}
}
