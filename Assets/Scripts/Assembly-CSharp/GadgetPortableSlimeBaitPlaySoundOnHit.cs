using UnityEngine;

public class GadgetPortableSlimeBaitPlaySoundOnHit : SRBehaviour, ControllerCollisionListener
{
	private GadgetPortableSlimeBait parent;

	public void Awake()
	{
		parent = GetRequiredComponentInParent<GadgetPortableSlimeBait>();
	}

	public void OnCollisionEnter(Collision collision)
	{
		parent.OnHit(base.transform);
	}

	public void OnControllerCollision(GameObject gameObject)
	{
		parent.OnHit(base.transform);
	}
}
