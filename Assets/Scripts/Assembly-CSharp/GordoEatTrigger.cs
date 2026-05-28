using UnityEngine;

public class GordoEatTrigger : MonoBehaviour
{
	private GordoEat eat;

	public void Awake()
	{
		eat = GetComponentInParent<GordoEat>();
	}

	public void OnTriggerEnter(Collider col)
	{
		if (!col.isTrigger)
		{
			eat.MaybeEat(col);
		}
	}
}
