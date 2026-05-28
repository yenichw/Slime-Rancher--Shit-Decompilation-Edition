using UnityEngine;

[RequireComponent(typeof(vp_PlayerEventHandler))]
public abstract class vp_Controller : vp_Component
{
	public enum PushForceMode
	{
		Simplified = 0,
		Kinetic = 1
	}

	protected bool m_Grounded;

	protected bool m_GroundedNonMountain;

	protected RaycastHit m_GroundHit;

	protected Transform m_LastGroundHitTransform;

	protected Transform m_GroundHitTransform;

	protected float m_FallStartHeight = -99999f;

	protected float m_FallImpact;

	protected bool m_OnNewGround;

	protected bool m_WasFalling;

	public float PhysicsGravityModifier = 0.2f;

	protected float m_FallSpeed;

	public bool MotorFreeFly;

	public float PhysicsPushForce = 5f;

	public PushForceMode PhysicsPushMode;

	public float PhysicsPushInterval = 0.1f;

	public float PhysicsCrouchHeightModifier = 0.5f;

	public Vector3 m_Velocity = Vector3.zero;

	protected Vector3 m_PrevPosition = Vector3.zero;

	protected Vector3 m_PrevVelocity = Vector3.zero;

	protected float m_NextAllowedPushTime;

	protected Transform m_Platform;

	public Vector3 m_PositionOnPlatform = Vector3.zero;

	protected float m_LastPlatformAngle;

	public Vector3 m_LastPlatformPos = Vector3.zero;

	protected float m_MovingPlatformBodyYawDif;

	protected float m_NormalHeight;

	protected Vector3 m_NormalCenter = Vector3.zero;

	protected float m_CrouchHeight;

	protected Vector3 m_CrouchCenter = Vector3.zero;

	protected float m_TempGravityModifier;

	protected const float KINETIC_PUSHFORCE_MULTIPLIER = 15f;

	protected const float CHARACTER_CONTROLLER_SKINWIDTH = 0.08f;

	protected const float DEFAULT_RADIUS_MULTIPLIER = 0.25f;

	protected const float FALL_IMPACT_MULTIPLIER = 0.075f;

	protected const float NOFALL = -99999f;

	protected const float VEL_FALL_IMPACT_MULTIPLIER = 0.01f;

	protected const float MAX_FALL_SPEED_VALUE = 0.09f;

	private vp_PlayerEventHandler m_Player;

	public bool Grounded => m_Grounded;

	public float SkinWidth => 0.08f;

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

	private float FallDistance
	{
		get
		{
			if (m_FallStartHeight == -99999f)
			{
				return 0f;
			}
			return Mathf.Max(0f, m_FallStartHeight - base.Transform.position.y);
		}
	}

	protected virtual Transform OnValue_Platform
	{
		get
		{
			return m_Platform;
		}
		set
		{
			m_Platform = value;
		}
	}

	protected virtual Vector3 OnValue_Position
	{
		get
		{
			return base.Transform.position;
		}
		set
		{
			SetPosition(value);
		}
	}

	protected virtual float OnValue_FallSpeed
	{
		get
		{
			return m_FallSpeed;
		}
		set
		{
			m_FallSpeed = value;
		}
	}

	protected virtual Vector3 OnValue_Velocity
	{
		get
		{
			return m_Velocity;
		}
		set
		{
			m_Velocity = value;
		}
	}

	protected abstract float OnValue_Radius { get; }

	protected abstract float OnValue_Height { get; }

	protected override void Awake()
	{
		base.Awake();
		InitCollider();
	}

	protected override void Start()
	{
		base.Start();
		RefreshCollider();
	}

	protected override void Update()
	{
		base.Update();
		UpdatePlatformRotation();
	}

	protected override void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			UpdateForces();
			FixedMove();
			UpdateCollisions();
			UpdatePlatformMove();
			UpdateVelocity();
		}
	}

	protected virtual void UpdatePlatformMove()
	{
		if (!(m_Platform == null))
		{
			m_PositionOnPlatform = m_Platform.InverseTransformPoint(m_Transform.position);
			m_LastPlatformPos = m_Platform.position;
		}
	}

	protected virtual void UpdatePlatformRotation()
	{
		if (!(m_Platform == null))
		{
			if (Player.IsLocal.Get())
			{
				m_MovingPlatformBodyYawDif = Mathf.Lerp(m_MovingPlatformBodyYawDif, Mathf.DeltaAngle(Player.Rotation.Get().y, Player.BodyYaw.Get()), Time.deltaTime * 1f);
			}
			Player.Rotation.Set(new Vector2(Player.Rotation.Get().x, Player.Rotation.Get().y - Mathf.DeltaAngle(m_Platform.eulerAngles.y, m_LastPlatformAngle)));
			m_LastPlatformAngle = m_Platform.eulerAngles.y;
			if (Player.IsLocal.Get())
			{
				Player.BodyYaw.Set(Player.BodyYaw.Get() - m_MovingPlatformBodyYawDif);
			}
		}
	}

	protected virtual void UpdateVelocity()
	{
		m_PrevVelocity = m_Velocity;
		m_Velocity = (base.transform.position - m_PrevPosition) / Time.deltaTime;
		m_PrevPosition = base.Transform.position;
	}

	public virtual void Stop()
	{
		Player.Move.Send(Vector3.zero);
		Player.InputMoveVector.Set(Vector2.zero);
		m_FallSpeed = 0f;
		m_FallStartHeight = -99999f;
	}

	protected virtual void InitCollider()
	{
	}

	protected virtual void RefreshCollider()
	{
	}

	public virtual void EnableCollider(bool enabled)
	{
	}

	protected virtual void StoreGroundInfo()
	{
		m_LastGroundHitTransform = m_GroundHitTransform;
		m_Grounded = false;
		m_GroundedNonMountain = false;
		m_GroundHitTransform = null;
		if (Physics.SphereCast(new Ray(base.Transform.position + Vector3.up * Player.Radius.Get(), Vector3.down), Player.Radius.Get(), out m_GroundHit, 0.18f, -675375893))
		{
			m_GroundHitTransform = m_GroundHit.transform;
			Collider collider = m_GroundHit.collider;
			if (collider != null)
			{
				Vacuumable vacuumable = null;
				if (vp_Layer.IsInMask((LayerMask)collider.gameObject.layer, -2206209))
				{
					vacuumable = collider.GetComponent<Vacuumable>();
				}
				m_Grounded = vacuumable == null || !vacuumable.isCaptive();
				m_GroundedNonMountain = m_Grounded && collider.gameObject.layer != 12;
			}
			else
			{
				m_Grounded = false;
			}
		}
		if (m_Velocity.y < 0f && m_GroundHitTransform == null && m_LastGroundHitTransform != null && !Player.Jump.Active)
		{
			SetFallHeight(base.Transform.position.y);
		}
	}

	private void SetFallHeight(float height)
	{
		if (m_FallStartHeight == -99999f && !m_Grounded && !(m_GroundHitTransform != null))
		{
			m_FallStartHeight = height;
		}
	}

	protected virtual void UpdateForces()
	{
		m_LastGroundHitTransform = m_GroundHitTransform;
		if (m_Grounded && m_FallSpeed <= 0f)
		{
			m_FallSpeed = Physics.gravity.y * ((PhysicsGravityModifier + m_TempGravityModifier) * 0.002f) * vp_TimeUtility.AdjustedTimeScale;
			return;
		}
		m_FallSpeed = Mathf.Min(0.09f, m_FallSpeed + Physics.gravity.y * ((PhysicsGravityModifier + m_TempGravityModifier) * 0.002f) * vp_TimeUtility.AdjustedTimeScale);
		if (m_Velocity.y < 0f && m_PrevVelocity.y >= 0f)
		{
			SetFallHeight(base.Transform.position.y);
		}
	}

	protected virtual void FixedMove()
	{
		StoreGroundInfo();
	}

	protected virtual void UpdateCollisions()
	{
		if (Player.Climb.Active)
		{
			m_FallStartHeight = -99999f;
		}
		m_FallImpact = 0f;
		m_OnNewGround = false;
		m_WasFalling = false;
		if (!(m_GroundHitTransform != null) || !(m_GroundHitTransform != m_LastGroundHitTransform))
		{
			return;
		}
		m_OnNewGround = true;
		if (m_LastGroundHitTransform == null)
		{
			m_WasFalling = true;
			if (m_FallStartHeight > base.Transform.position.y && m_Grounded)
			{
				m_FallImpact = Mathf.Max(0f, 0f - m_Velocity.y) * 0.01f;
				Player.FallImpact.Send(m_FallImpact);
			}
		}
		m_FallStartHeight = -99999f;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
	}

	public virtual void SetPosition(Vector3 position)
	{
		base.Transform.position = position;
		m_PrevPosition = position;
		vp_Timer.In(0f, delegate
		{
			m_PrevVelocity = vp_3DUtility.HorizontalVector(m_PrevVelocity);
		});
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection, PushForceMode pushForcemode, Vector3 point)
	{
		switch (pushForcemode)
		{
		case PushForceMode.Simplified:
			rigidbody.velocity += vp_3DUtility.HorizontalVector(new Vector3(moveDirection.x, 0f, moveDirection.z).normalized) * (PhysicsPushForce / rigidbody.mass);
			break;
		case PushForceMode.Kinetic:
			if (Vector3.Distance(vp_3DUtility.HorizontalVector(base.Transform.position), vp_3DUtility.HorizontalVector(point)) > Player.Radius.Get())
			{
				rigidbody.AddForceAtPosition(vp_3DUtility.HorizontalVector(moveDirection) * (PhysicsPushForce * 15f), point);
			}
			else
			{
				rigidbody.AddForceAtPosition(moveDirection * (PhysicsPushForce * 15f), point);
			}
			break;
		}
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection, PushForceMode pushForceMode)
	{
		PushRigidbody(rigidbody, moveDirection, pushForceMode, (pushForceMode == PushForceMode.Simplified) ? Vector3.zero : rigidbody.ClosestPointOnBounds(base.Collider.bounds.center));
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection)
	{
		PushRigidbody(rigidbody, moveDirection, PhysicsPushMode, (PhysicsPushMode == PushForceMode.Simplified) ? Vector3.zero : rigidbody.ClosestPointOnBounds(base.Collider.bounds.center));
	}

	public void PushRigidbody(Rigidbody rigidbody, Vector3 moveDirection, Vector3 point)
	{
		PushRigidbody(rigidbody, moveDirection, PhysicsPushMode, point);
	}

	protected virtual void OnMessage_Stop()
	{
		Stop();
	}

	protected virtual void OnStart_Crouch()
	{
		Player.Run.Stop();
		RefreshCollider();
	}

	protected virtual void OnStop_Crouch()
	{
		RefreshCollider();
	}

	protected virtual Transform Get_Platform()
	{
		return m_Platform;
	}

	protected virtual void Set_Platform(Transform value)
	{
		m_Platform = value;
	}

	protected virtual Vector3 Get_Position()
	{
		return base.Transform.position;
	}

	protected virtual void Set_Position(Vector3 value)
	{
		SetPosition(value);
	}

	protected virtual float Get_FallSpeed()
	{
		return m_FallSpeed;
	}

	protected virtual void Set_FallSpeed(float value)
	{
		m_FallSpeed = value;
	}

	protected virtual Vector3 Get_Velocity()
	{
		return m_Velocity;
	}

	protected virtual void Set_Velocity(Vector3 value)
	{
		m_Velocity = value;
	}

	protected abstract float Get_Radius();

	protected abstract float Get_Height();

	public void AdjustFallSpeed(float fallAdjust)
	{
		m_FallSpeed += fallAdjust;
	}

	public void SetTempGravityModifier(float tempGravityModifier)
	{
		m_TempGravityModifier = tempGravityModifier;
	}

	protected string Vector3ToString(Vector3 s)
	{
		return $"x={s.x}|y={s.y}|z={s.z}";
	}

	public override void Register(vp_EventHandler eventHandler)
	{
		base.Register(eventHandler);
		eventHandler.RegisterMessage("Stop", OnMessage_Stop);
		eventHandler.RegisterActivity("Crouch", OnStart_Crouch, OnStop_Crouch, null, null, null, null);
		eventHandler.RegisterValue("FallSpeed", Get_FallSpeed, Set_FallSpeed);
		eventHandler.RegisterValue("Height", Get_Height, null);
		eventHandler.RegisterValue("Platform", Get_Platform, Set_Platform);
		eventHandler.RegisterValue("Radius", Get_Radius, null);
		eventHandler.RegisterValue("Velocity", Get_Velocity, Set_Velocity);
	}

	public override void Unregister(vp_EventHandler eventHandler)
	{
		base.Unregister(eventHandler);
		eventHandler.UnregisterMessage("Stop", OnMessage_Stop);
		eventHandler.UnregisterActivity("Crouch", OnStart_Crouch, OnStop_Crouch, null, null, null, null);
		eventHandler.UnregisterValue("FallSpeed", Get_FallSpeed, Set_FallSpeed);
		eventHandler.UnregisterValue("Height", Get_Height, null);
		eventHandler.UnregisterValue("Platform", Get_Platform, Set_Platform);
		eventHandler.UnregisterValue("Radius", Get_Radius, null);
		eventHandler.UnregisterValue("Velocity", Get_Velocity, Set_Velocity);
	}
}
