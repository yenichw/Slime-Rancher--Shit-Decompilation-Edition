using UnityEngine;

public class AttackPlayer : CollidableActorBehaviour, ControllerCollisionListener, Collidable, RegistryUpdateable
{
	public delegate void OnFinishChompSuccessDelegate(GameObject gameObject);

	public OnFinishChompSuccessDelegate onFinishChompSuccess;

	public bool shouldAttackPlayer;

	public int damagePerAttack = 20;

	private Chomper chomper;

	private SlimeFaceAnimator faceAnim;

	private Vacuumable vacuumable;

	private SlimeAudio slimeAudio;

	private static LRUCache<int, Identifiable> recentIds = new LRUCache<int, Identifiable>(200);

	public override void Awake()
	{
		vacuumable = GetComponent<Vacuumable>();
		chomper = GetComponent<Chomper>();
		faceAnim = GetComponent<SlimeFaceAnimator>();
		slimeAudio = GetComponent<SlimeAudio>();
	}

	public void OnControllerCollision(GameObject obj)
	{
		MaybeSpinAndChomp(obj, ignoreEmotions: false);
	}

	public void ProcessCollisionEnter(Collision col)
	{
		MaybeSpinAndChomp(col.gameObject, ignoreEmotions: false);
	}

	public void ProcessCollisionExit(Collision col)
	{
	}

	public bool MaybeSpinAndChomp(GameObject obj, bool ignoreEmotions)
	{
		if (shouldAttackPlayer && chomper.CanChomp())
		{
			Identifiable.Id id = ExtractOtherId(obj);
			if (id == Identifiable.Id.PLAYER)
			{
				base.transform.LookAt(obj.transform);
				chomper.StartChomp(obj, id, whileHeld: false, quick: true, null, FinishChomp);
			}
			return true;
		}
		return false;
	}

	public bool DoesAttack(GameObject other)
	{
		return ExtractOtherId(other) == Identifiable.Id.PLAYER;
	}

	public bool MaybeChomp(GameObject obj)
	{
		if (shouldAttackPlayer && chomper.CanChomp())
		{
			Identifiable.Id id = ExtractOtherId(obj);
			if (id == Identifiable.Id.PLAYER)
			{
				chomper.StartChomp(obj, id, whileHeld: false, quick: false, null, FinishChomp);
			}
			return true;
		}
		return false;
	}

	private Identifiable.Id ExtractOtherId(GameObject other)
	{
		int instanceID = other.GetInstanceID();
		if (recentIds.contains(instanceID))
		{
			Identifiable identifiable = recentIds.get(instanceID);
			return (!(identifiable == null)) ? identifiable.id : Identifiable.Id.NONE;
		}
		Identifiable component = other.GetComponent<Identifiable>();
		recentIds.put(instanceID, component);
		return (!(component == null)) ? component.id : Identifiable.Id.NONE;
	}

	public void RegistryUpdate()
	{
		if (shouldAttackPlayer && vacuumable != null && vacuumable.isHeld() && chomper.CanChomp())
		{
			GameObject player = SRSingleton<SceneContext>.Instance.Player;
			chomper.StartChomp(player, Identifiable.Id.PLAYER, whileHeld: true, quick: false, null, FinishChomp);
		}
	}

	public void CancelChomp(GameObject obj)
	{
		chomper.CancelChomp(obj);
	}

	private void FinishChomp(GameObject chomping, Identifiable.Id chompingId, bool whileHeld, bool wasLaunched)
	{
		slimeAudio.Play(slimeAudio.slimeSounds.attackCue);
		if (whileHeld)
		{
			SRSingleton<Overlay>.Instance.PlayChomp();
		}
		if (!(chomping == null))
		{
			faceAnim.SetTrigger("triggerChompClosed");
			DoDamage(chomping, immediateMode: false);
			if (onFinishChompSuccess != null)
			{
				onFinishChompSuccess(chomping);
			}
		}
	}

	private bool DoDamage(GameObject other, bool immediateMode)
	{
		if (other == null)
		{
			return true;
		}
		if (!immediateMode)
		{
			slimeAudio.Play(slimeAudio.slimeSounds.gulpCue);
		}
		if (other.GetInterfaceComponent<Damageable>().Damage(damagePerAttack, base.gameObject))
		{
			DeathHandler.Kill(other, DeathHandler.Source.SLIME_ATTACK_PLAYER, base.gameObject, "AttackPlayer.DoDamage");
			if (!immediateMode)
			{
				PlayOnDeathAudio(other);
			}
			return true;
		}
		return false;
	}

	private void PlayOnDeathAudio(GameObject other)
	{
		SlimeAudio componentInChildren = other.GetComponentInChildren<SlimeAudio>();
		if (componentInChildren != null && componentInChildren.slimeSounds.voiceDamageCue != null)
		{
			SECTR_AudioSystem.Play(componentInChildren.slimeSounds.voiceDamageCue, other.transform.position, loop: false);
		}
	}
}
