using UnityEngine;

public class ActivateOnPlayerEnter : MonoBehaviour
{
	public GameObject toActivate;

	public void Awake()
	{
		toActivate.SetActive(value: false);
	}

	public void OnTriggerEnter(Collider collider)
	{
		Identifiable component = collider.GetComponent<Identifiable>();
		if (component != null && component.id == Identifiable.Id.PLAYER)
		{
			toActivate.SetActive(value: true);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		Identifiable component = collider.GetComponent<Identifiable>();
		if (component != null && component.id == Identifiable.Id.PLAYER)
		{
			toActivate.SetActive(value: false);
		}
	}
}
