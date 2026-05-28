using System.Collections.Generic;
using UnityEngine;

public class CameraDisabler : MonoBehaviour
{
	private Camera cam;

	private List<Component> blockers = new List<Component>();

	private LayerMask standardMask;

	private LayerMask uiOnlyMask;

	public void Start()
	{
		uiOnlyMask = LayerMask.GetMask("UI");
		cam = GetComponent<Camera>();
		standardMask = cam.cullingMask;
	}

	public void AddBlocker(Component comp)
	{
		blockers.Add(comp);
		if (blockers.Count > 0)
		{
			cam.cullingMask = uiOnlyMask;
		}
	}

	public void RemoveBlocker(Component comp)
	{
		blockers.Remove(comp);
		if (blockers.Count <= 0 && cam != null)
		{
			cam.cullingMask = standardMask;
		}
	}
}
