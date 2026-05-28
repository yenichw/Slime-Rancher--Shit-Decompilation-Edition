using System.Collections.Generic;
using UnityEngine;

public class ForceStencilClear : MonoBehaviour
{
	private HashSet<GameObject> enablers;

	public void OnPreRender()
	{
		GL.Clear(clearDepth: true, clearColor: false, Color.black);
	}

	public void RegisterEnabler(GameObject enabler)
	{
		if (enablers == null)
		{
			enablers = new HashSet<GameObject>();
		}
		enablers.Add(enabler);
		base.enabled = true;
	}

	public void DeregisterEnabler(GameObject enabler)
	{
		if (enablers != null && enablers.Remove(enabler) && enablers.Count == 0)
		{
			base.enabled = false;
		}
	}
}
