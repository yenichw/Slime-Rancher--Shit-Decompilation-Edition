using UnityEngine;

public class BarrierController : MonoBehaviour
{
	private GameObject barrier;

	private const string BARRIER_NAME = "Barrier";

	public void SetIsOpen(bool isOpen)
	{
		if (barrier == null)
		{
			barrier = base.transform.Find("Barrier").gameObject;
		}
		if (barrier != null)
		{
			barrier.SetActive(!isOpen);
		}
	}
}
