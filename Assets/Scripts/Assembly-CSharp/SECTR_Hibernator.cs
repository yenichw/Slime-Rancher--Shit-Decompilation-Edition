using UnityEngine;

[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("SECTR/Stream/SECTR Hibernator")]
public class SECTR_Hibernator : MonoBehaviour
{
	public delegate void HibernateCallback();

	private bool hibernating;

	private SECTR_Member cachedMember;

	[SECTR_ToolTip("Hibernate components on children as well as ones on this game object.")]
	public bool HibernateChildren = true;

	[SECTR_ToolTip("Disable Behavior components during hibernation.")]
	public bool HibernateBehaviors = true;

	[SECTR_ToolTip("Disable Collder components during hibernation.")]
	public bool HibernateColliders = true;

	[SECTR_ToolTip("Disable RigidBody components during hibernation.")]
	public bool HibernateRigidBodies = true;

	[SECTR_ToolTip("Hide Render components during hibernation.")]
	public bool HibernateRenderers = true;

	private Vector3 vel;

	private Vector3 angularVel;

	private bool kinematic;

	private SECTR_Sector.SectorSetId setId = SECTR_Sector.SectorSetId.UNSET;

	public bool isHibernating => hibernating;

	public event HibernateCallback Awoke;

	public event HibernateCallback Hibernated;

	public event HibernateCallback HibernateUpdate;

	private void OnEnable()
	{
		cachedMember = GetComponent<SECTR_Member>();
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.RegisterHibernator(this);
			RegisterMember();
			OnUpdate();
		}
	}

	private void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.DeregisterHibernator(this);
			DeregisterMember();
		}
	}

	private void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.DeregisterHibernator(this);
			DeregisterMember();
		}
	}

	private void RegisterMember()
	{
		SECTR_Member component = GetComponent<SECTR_Member>();
		if (component != null && SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.RegisterMember(component);
		}
	}

	private void DeregisterMember()
	{
		SECTR_Member component = GetComponent<SECTR_Member>();
		if (component != null && SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.DeregisterMember(component);
		}
	}

	public void OnUpdate()
	{
		if (setId == SECTR_Sector.SectorSetId.UNSET)
		{
			setId = SECTR_Sector.GetSectorSetForPos(base.transform.position);
		}
		bool flag = SECTR_Sector.IsCurrSectorSet(setId);
		bool flag2 = !flag;
		int count = cachedMember.sectors.Count;
		bool flag3 = count > 0 || !flag;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = cachedMember.sectors[i];
			if (sECTR_Sector.isFrozen)
			{
				flag2 = true;
			}
			if (!sECTR_Sector.isHibernating)
			{
				flag3 = false;
			}
		}
		if ((flag2 || flag3) && !hibernating)
		{
			_Hibernate();
		}
		else if (!(flag2 || flag3) && hibernating)
		{
			_WakeUp();
		}
		if (hibernating && this.HibernateUpdate != null)
		{
			this.HibernateUpdate();
		}
	}

	private void _WakeUp()
	{
		if (hibernating)
		{
			hibernating = false;
			RegisterMember();
			_UpdateComponents();
			if (this.Awoke != null)
			{
				this.Awoke();
			}
		}
	}

	private void _Hibernate()
	{
		if (!hibernating)
		{
			hibernating = true;
			DeregisterMember();
			_UpdateComponents();
			if (this.Hibernated != null)
			{
				this.Hibernated();
			}
		}
	}

	private void _UpdateComponents()
	{
		if (HibernateBehaviors)
		{
			Behaviour[] array = (HibernateChildren ? GetComponentsInChildren<Behaviour>() : GetComponents<Behaviour>());
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				Behaviour behaviour = array[i];
				if (behaviour.GetType() != typeof(SECTR_Hibernator) && behaviour.GetType() != typeof(SECTR_Member))
				{
					behaviour.enabled = !hibernating;
				}
			}
		}
		if (HibernateRigidBodies)
		{
			Rigidbody[] array2 = (HibernateChildren ? GetComponentsInChildren<Rigidbody>() : GetComponents<Rigidbody>());
			int num2 = array2.Length;
			for (int j = 0; j < num2; j++)
			{
				Rigidbody rigidbody = array2[j];
				if (hibernating)
				{
					vel = rigidbody.velocity;
					angularVel = rigidbody.angularVelocity;
					kinematic = rigidbody.isKinematic;
					rigidbody.Sleep();
					rigidbody.isKinematic = true;
				}
				else if (rigidbody.IsSleeping())
				{
					rigidbody.isKinematic = kinematic;
					rigidbody.WakeUp();
					rigidbody.velocity = vel;
					rigidbody.angularVelocity = angularVel;
				}
			}
		}
		if (HibernateColliders)
		{
			Collider[] array3 = (HibernateChildren ? GetComponentsInChildren<Collider>() : GetComponents<Collider>());
			int num3 = array3.Length;
			for (int k = 0; k < num3; k++)
			{
				array3[k].enabled = !hibernating;
			}
		}
		if (HibernateRenderers)
		{
			Renderer[] array4 = (HibernateChildren ? GetComponentsInChildren<Renderer>() : GetComponents<Renderer>());
			int num4 = array4.Length;
			for (int l = 0; l < num4; l++)
			{
				array4[l].enabled = !hibernating;
			}
		}
	}

	internal void RecheckHibernation()
	{
		OnUpdate();
	}
}
