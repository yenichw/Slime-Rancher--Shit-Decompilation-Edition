using UnityEngine;

public class DestroyTotemLinker : MonoBehaviour
{
	public void Start()
	{
		TotemLinker componentInChildren = GetComponentInChildren<TotemLinker>();
		if (componentInChildren != null)
		{
			Destroyer.Destroy(componentInChildren.gameObject, "DestroyTotemLinker.Start#1");
		}
		TotemLinkerHelper component = GetComponent<TotemLinkerHelper>();
		if (component != null)
		{
			Destroyer.Destroy(component, "DestroyTotemLinker.Start#2");
		}
	}
}
