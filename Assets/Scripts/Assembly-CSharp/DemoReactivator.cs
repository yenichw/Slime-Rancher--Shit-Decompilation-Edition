using UnityEngine;

public class DemoReactivator : MonoBehaviour
{
	public float TimeDelayToReactivate = 3f;

	private void Start()
	{
		InvokeRepeating("Reactivate", TimeDelayToReactivate, TimeDelayToReactivate);
	}

	private void Reactivate()
	{
		base.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}
}
