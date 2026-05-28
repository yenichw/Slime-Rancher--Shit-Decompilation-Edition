using UnityEngine;

public class GoldSlimeFlee : SlimeFlee
{
	private bool currentlyChomping;

	private double chompingUntil;

	private SlimeEat eat;

	private const float CHOMPING_MAX_DELAY = 1f / 60f;

	public override void Awake()
	{
		base.Awake();
		eat = GetComponent<SlimeEat>();
		eat.onStartChomp = OnStartChomp;
		eat.onProducePlortsComplete = OnPlortsProduced;
	}

	public override void OnDestroy()
	{
		eat.onStartChomp = null;
		eat.onProducePlortsComplete = null;
		eat = null;
		base.OnDestroy();
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (!IsFleeing() && !collider.isTrigger && PhysicsUtil.IsPlayerMainCollider(collider))
		{
			StartFleeing(collider.gameObject);
		}
	}

	private void OnStartChomp()
	{
		currentlyChomping = true;
		chompingUntil = timeDir.HoursFromNow(1f / 60f);
		slimeBody.velocity = Vector3.zero;
		slimeBody.angularVelocity = Vector3.zero;
	}

	private void OnPlortsProduced()
	{
		currentlyChomping = false;
	}

	public override void Action()
	{
		if (!currentlyChomping || timeDir.HasReached(chompingUntil))
		{
			base.Action();
		}
	}
}
