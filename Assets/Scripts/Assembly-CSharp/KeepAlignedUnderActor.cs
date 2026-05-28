using UnityEngine;

public class KeepAlignedUnderActor : MonoBehaviour
{
	private Transform alignWith;

	private Rigidbody body;

	public void Start()
	{
		body = GetComponent<Rigidbody>();
	}

	public void AlignWith(Transform alignWith)
	{
		this.alignWith = alignWith;
	}

	public void FixedUpdate()
	{
		if (alignWith == null)
		{
			Destroyer.Destroy(base.gameObject, "KeepAlignedUnderActor.FixedUpdate");
		}
		else
		{
			body.MovePosition(alignWith.position);
		}
	}
}
