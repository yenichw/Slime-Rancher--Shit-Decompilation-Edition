using UnityEngine;

public class PollenContactDetector : MonoBehaviour
{
	private PollenCloudDestructor destructor;

	public void Awake()
	{
		destructor = GetComponentInParent<PollenCloudDestructor>();
	}

	public void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger && col.GetComponent<Rigidbody>() == null)
		{
			destructor.AddContact();
		}
	}

	public void OnTriggerExit(Collider col)
	{
		if (!col.isTrigger && col.GetComponent<Rigidbody>() == null)
		{
			destructor.RemoveContact();
		}
	}
}
