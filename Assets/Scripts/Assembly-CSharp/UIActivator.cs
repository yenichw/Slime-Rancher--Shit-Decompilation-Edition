using UnityEngine;

public class UIActivator : MonoBehaviour
{
	public GameObject uiPrefab;

	public GameObject blockInExpoPrefab;

	public virtual bool CanActivate()
	{
		return true;
	}

	public virtual GameObject Activate()
	{
		GameObject obj = Object.Instantiate(uiPrefab);
		LandPlotUI component = obj.GetComponent<LandPlotUI>();
		if (component != null)
		{
			component.SetActivator(base.gameObject.GetComponentInParent<LandPlot>());
		}
		AccessDoorUI component2 = obj.GetComponent<AccessDoorUI>();
		if (component2 != null)
		{
			component2.SetAccessDoor(base.gameObject.GetComponentInParent<AccessDoor>());
		}
		obj.GetComponent<LocationalUI>()?.SetPosition(base.transform.position);
		return obj;
	}
}
