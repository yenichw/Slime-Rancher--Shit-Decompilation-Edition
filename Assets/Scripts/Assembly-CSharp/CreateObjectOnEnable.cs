using UnityEngine;

public class CreateObjectOnEnable : SRBehaviour
{
	public GameObject toCreate;

	public bool attachToParent;

	public void OnEnable()
	{
		Object.Instantiate(toCreate, Vector3.zero, Quaternion.identity).transform.SetParent(attachToParent ? base.transform.parent : base.transform, worldPositionStays: false);
	}
}
