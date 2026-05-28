using UnityEngine;

public class TotemLinkerHelper : MonoBehaviour
{
	private TotemLinker totemLinker;

	public void Start()
	{
		TotemLinker[] componentsInChildren = GetComponentsInChildren<TotemLinker>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			totemLinker = componentsInChildren[0];
		}
	}

	public void Update()
	{
		totemLinker.UpdateEvenWhenInactive();
	}
}
