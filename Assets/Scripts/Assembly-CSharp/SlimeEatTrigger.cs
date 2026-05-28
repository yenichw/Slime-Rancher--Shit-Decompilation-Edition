using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlimeEatTrigger : RegisteredActorBehaviour, RegistryUpdateable
{
	private class EatTarget
	{
		public GameObject gameObject;

		public bool isColliding;

		public double time;
	}

	private const float NEXT_CHOMP_COOLDOWN = 0.050000004f;

	private TimeDirector timeDirector;

	private SlimeEat eat;

	private AttackPlayer attack;

	private List<EatTarget> targets = new List<EatTarget>();

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		eat = GetComponentInParent<SlimeEat>();
		SlimeEat slimeEat = eat;
		slimeEat.onFinishChompSuccess = (SlimeEat.OnFinishChompSuccessDelegate)Delegate.Combine(slimeEat.onFinishChompSuccess, new SlimeEat.OnFinishChompSuccessDelegate(OnFinishChompSuccess));
		attack = GetComponentInParent<AttackPlayer>();
		if (attack != null)
		{
			AttackPlayer attackPlayer = attack;
			attackPlayer.onFinishChompSuccess = (AttackPlayer.OnFinishChompSuccessDelegate)Delegate.Combine(attackPlayer.onFinishChompSuccess, new AttackPlayer.OnFinishChompSuccessDelegate(OnFinishChompSuccess));
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SlimeEat slimeEat = eat;
		slimeEat.onFinishChompSuccess = (SlimeEat.OnFinishChompSuccessDelegate)Delegate.Remove(slimeEat.onFinishChompSuccess, new SlimeEat.OnFinishChompSuccessDelegate(OnFinishChompSuccess));
		if (attack != null)
		{
			AttackPlayer attackPlayer = attack;
			attackPlayer.onFinishChompSuccess = (AttackPlayer.OnFinishChompSuccessDelegate)Delegate.Remove(attackPlayer.onFinishChompSuccess, new AttackPlayer.OnFinishChompSuccessDelegate(OnFinishChompSuccess));
		}
	}

	public void RegistryUpdate()
	{
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			EatTarget eatTarget = targets[num];
			if (eatTarget.gameObject == null)
			{
				targets.RemoveAt(num);
			}
			else if (timeDirector.HasReached(eatTarget.time))
			{
				if (!eatTarget.isColliding)
				{
					targets.RemoveAt(num);
				}
				else if (eat.MaybeChomp(eatTarget.gameObject) || (attack != null && attack.MaybeChomp(eatTarget.gameObject)))
				{
					break;
				}
			}
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		SetColliding(collider, colliding: true);
	}

	public void OnTriggerExit(Collider collider)
	{
		SetColliding(collider, colliding: false);
		eat.CancelChomp(collider.gameObject);
		if (attack != null)
		{
			attack.CancelChomp(collider.gameObject);
		}
	}

	private void SetColliding(Collider collider, bool colliding)
	{
		if (eat.DoesEat(collider.gameObject) || (!(attack == null) && attack.DoesAttack(collider.gameObject)))
		{
			EatTarget eatTarget = FindTarget(collider.gameObject);
			if (eatTarget != null)
			{
				eatTarget.isColliding = colliding;
			}
			else if (colliding)
			{
				targets.Insert(0, new EatTarget
				{
					gameObject = collider.gameObject,
					time = timeDirector.WorldTime(),
					isColliding = true
				});
			}
		}
	}

	private EatTarget FindTarget(GameObject gameObject)
	{
		return targets.FirstOrDefault((EatTarget t) => t.gameObject == gameObject);
	}

	private void OnFinishChompSuccess(GameObject gameObject)
	{
		EatTarget eatTarget = FindTarget(gameObject);
		if (eatTarget != null)
		{
			eatTarget.time = timeDirector.HoursFromNow(0.050000004f);
		}
	}
}
