using UnityEngine;

public class NotifyBiteComplete : MonoBehaviour
{
	private Chomper chomper;

	public void Awake()
	{
		chomper = GetComponentInParent<Chomper>();
	}

	public void DisableBiteModel()
	{
		chomper.BiteComplete();
	}
}
