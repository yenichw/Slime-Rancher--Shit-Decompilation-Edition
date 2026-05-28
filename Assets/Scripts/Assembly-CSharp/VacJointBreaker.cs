using UnityEngine;

public class VacJointBreaker : MonoBehaviour
{
	private const float FLING_REDUCTION_FACTOR = 0.5f;

	public void OnJointBreak(float breakForce)
	{
		Vacuumable component = GetComponent<Joint>().connectedBody.GetComponent<Vacuumable>();
		component.release();
		component.gameObject.GetComponent<Rigidbody>().velocity = component.gameObject.GetComponent<Rigidbody>().velocity * 0.5f;
		Destroyer.Destroy(base.gameObject, "VacJointBreaker.OnJointBreak");
	}
}
