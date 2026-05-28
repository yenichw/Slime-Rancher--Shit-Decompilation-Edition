using UnityEngine;

public class vp_Weapon : vp_Component
{
	public new enum Type
	{
		Custom = 0,
		Firearm = 1,
		Melee = 2,
		Thrown = 3
	}

	public enum Grip
	{
		Custom = 0,
		OneHanded = 1,
		TwoHanded = 2,
		TwoHandedHeavy = 3
	}

	public GameObject Weapon3rdPersonModel;

	protected GameObject m_WeaponModel;

	public Vector3 PositionOffset = new Vector3(0.15f, -0.15f, -0.15f);

	public float PositionSpring2Stiffness = 0.95f;

	public float PositionSpring2Damping = 0.25f;

	protected vp_Spring m_PositionSpring2;

	public Vector3 RotationOffset = Vector3.zero;

	public float RotationSpring2Stiffness = 0.95f;

	public float RotationSpring2Damping = 0.25f;

	protected vp_Spring m_RotationSpring2;

	protected bool m_Wielded = true;

	protected vp_Timer.Handle m_Weapon3rdPersonModelWakeUpTimer = new vp_Timer.Handle();

	public int AnimationType = 1;

	public int AnimationGrip = 1;

	protected vp_PlayerEventHandler m_Player;

	protected Renderer m_Weapon3rdPersonModelRenderer;

	protected Vector3 m_RotationSpring2DefaultRotation = Vector3.zero;

	public bool Wielded
	{
		get
		{
			if (m_Wielded)
			{
				return base.Rendering;
			}
			return false;
		}
		set
		{
			m_Wielded = value;
		}
	}

	protected vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_PlayerEventHandler)base.EventHandler;
			}
			return m_Player;
		}
	}

	public Renderer Weapon3rdPersonModelRenderer
	{
		get
		{
			if (m_Weapon3rdPersonModelRenderer == null && Weapon3rdPersonModel != null)
			{
				m_Weapon3rdPersonModelRenderer = Weapon3rdPersonModel.GetComponent<Renderer>();
			}
			return m_Weapon3rdPersonModelRenderer;
		}
	}

	public Vector3 RotationSpring2DefaultRotation
	{
		get
		{
			return m_RotationSpring2DefaultRotation;
		}
		set
		{
			m_RotationSpring2DefaultRotation = value;
		}
	}

	public Vector3 Recoil => m_RotationSpring2.State;

	protected virtual Vector3 OnValue_AimDirection => (Weapon3rdPersonModel.transform.position - Player.LookPoint.Get()).normalized;

	protected override void Awake()
	{
		base.Awake();
		RotationOffset = base.transform.localEulerAngles;
		PositionOffset = base.transform.position;
		base.Transform.localEulerAngles = RotationOffset;
		if (base.transform.parent == null)
		{
			Debug.LogError(string.Concat("Error (", this, ") Must not be placed in scene root. Disabling self."));
			vp_Utility.Activate(base.gameObject, activate: false);
		}
		else if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = false;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshWeaponModel();
	}

	protected override void OnDisable()
	{
		RefreshWeaponModel();
		Activate3rdPersonModel(active: false);
		base.OnDisable();
	}

	protected override void Start()
	{
		base.Start();
		m_PositionSpring2 = new vp_Spring(base.transform, vp_Spring.UpdateMode.PositionAdditiveSelf);
		m_PositionSpring2.MinVelocity = 1E-05f;
		m_RotationSpring2 = new vp_Spring(base.transform, vp_Spring.UpdateMode.RotationAdditiveGlobal);
		m_RotationSpring2.MinVelocity = 1E-05f;
		SnapSprings();
		Refresh();
		CacheRenderers();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Time.timeScale != 0f)
		{
			UpdateSprings();
		}
	}

	public virtual void AddForce2(Vector3 positional, Vector3 angular)
	{
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.AddForce(positional);
		}
		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.AddForce(angular);
		}
	}

	public virtual void AddForce2(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce2(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}

	protected virtual void UpdateSprings()
	{
		base.Transform.localPosition = Vector3.up;
		base.Transform.localRotation = Quaternion.identity;
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring2.FixedUpdate();
	}

	public override void Refresh()
	{
		if (Application.isPlaying)
		{
			if (m_PositionSpring2 != null)
			{
				m_PositionSpring2.Stiffness = new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
				m_PositionSpring2.Damping = Vector3.one - new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);
				m_PositionSpring2.RestState = Vector3.zero;
			}
			if (m_RotationSpring2 != null)
			{
				m_RotationSpring2.Stiffness = new Vector3(RotationSpring2Stiffness, RotationSpring2Stiffness, RotationSpring2Stiffness);
				m_RotationSpring2.Damping = Vector3.one - new Vector3(RotationSpring2Damping, RotationSpring2Damping, RotationSpring2Damping);
				m_RotationSpring2.RestState = m_RotationSpring2DefaultRotation;
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		m_Wielded = true;
		base.Rendering = true;
	}

	public virtual void SnapSprings()
	{
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.RestState = Vector3.zero;
			m_PositionSpring2.State = Vector3.zero;
			m_PositionSpring2.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.RestState = m_RotationSpring2DefaultRotation;
			m_RotationSpring2.State = m_RotationSpring2DefaultRotation;
			m_RotationSpring2.Stop(includeSoftForce: true);
		}
	}

	public virtual void StopSprings()
	{
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.Stop(includeSoftForce: true);
		}
	}

	public virtual void Wield(bool isWielding = true)
	{
		m_Wielded = isWielding;
		Refresh();
		base.StateManager.CombineStates();
	}

	public virtual void RefreshWeaponModel()
	{
		if (!(Player == null) && Player.IsFirstPerson != null)
		{
			base.Transform.localScale = (Player.IsFirstPerson.Get() ? Vector3.one : Vector3.zero);
			Activate3rdPersonModel(!Player.IsFirstPerson.Get());
			if (Player != null && Player.CurrentWeaponName != null && Player.CurrentWeaponName.Get != null && Player.CurrentWeaponName.Get() != base.name)
			{
				Activate3rdPersonModel(active: false);
			}
		}
	}

	protected virtual void Activate3rdPersonModel(bool active = true)
	{
		if (Weapon3rdPersonModel == null)
		{
			return;
		}
		if (active)
		{
			if (Weapon3rdPersonModelRenderer != null)
			{
				Weapon3rdPersonModelRenderer.enabled = true;
			}
			vp_Utility.Activate(Weapon3rdPersonModel);
			return;
		}
		if (Weapon3rdPersonModelRenderer != null)
		{
			Weapon3rdPersonModelRenderer.enabled = false;
		}
		vp_Timer.In(0.1f, delegate
		{
			if (Weapon3rdPersonModel != null)
			{
				vp_Utility.Activate(Weapon3rdPersonModel, activate: false);
			}
		}, m_Weapon3rdPersonModelWakeUpTimer);
	}

	protected virtual void OnStart_Dead()
	{
		if (!Player.IsFirstPerson.Get())
		{
			base.Rendering = false;
		}
	}

	protected virtual void OnStop_Dead()
	{
		if (!Player.IsFirstPerson.Get())
		{
			base.Rendering = true;
		}
	}

	protected virtual Vector3 Get_AimDirection()
	{
		return (Weapon3rdPersonModel.transform.position - Player.LookPoint.Get()).normalized;
	}

	protected virtual bool CanStart_Zoom()
	{
		if (Player.CurrentWeaponType.Get() == 2)
		{
			return false;
		}
		return true;
	}

	public override void Register(vp_EventHandler eventHandler)
	{
		base.Register(eventHandler);
		eventHandler.RegisterActivity("Zoom", null, null, CanStart_Zoom, null, null, null);
		eventHandler.RegisterActivity("Dead", OnStart_Dead, OnStop_Dead, null, null, null, null);
		eventHandler.RegisterValue("AimDirection", Get_AimDirection, null);
	}

	public override void Unregister(vp_EventHandler eventHandler)
	{
		base.Unregister(eventHandler);
		eventHandler.UnregisterActivity("Zoom", null, null, CanStart_Zoom, null, null, null);
		eventHandler.UnregisterActivity("Dead", OnStart_Dead, OnStop_Dead, null, null, null, null);
		eventHandler.UnregisterValue("AimDirection", Get_AimDirection, null);
	}
}
