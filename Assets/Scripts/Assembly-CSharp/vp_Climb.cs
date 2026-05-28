using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vp_Climb : vp_Interactable, EventHandlerRegistrable
{
	[Serializable]
	public class vp_ClimbingSounds
	{
		public AudioSource AudioSource;

		public List<AudioClip> MountSounds = new List<AudioClip>();

		public List<AudioClip> DismountSounds = new List<AudioClip>();

		public float ClimbingSoundSpeed = 4f;

		public Vector2 ClimbingPitch = new Vector2(1f, 1.5f);

		public List<AudioClip> ClimbingSounds = new List<AudioClip>();
	}

	public float MinimumClimbSpeed = 3f;

	public float ClimbSpeed = 16f;

	public float MountSpeed = 5f;

	public float DistanceToClimbable = 1f;

	public float MinVelocityToClimb = 7f;

	public float ClimbAgainTimeout = 1f;

	public bool MountAutoRotatePitch;

	public bool SimpleClimb = true;

	public float DismountForce = 0.2f;

	public vp_ClimbingSounds Sounds;

	protected int m_LastWeaponEquipped;

	protected bool m_IsClimbing;

	protected float m_CanClimbAgain;

	protected Vector3 m_CachedDirection = Vector3.zero;

	protected Vector2 m_CachedRotation = Vector2.zero;

	protected vp_Timer.Handle m_ClimbingSoundTimer = new vp_Timer.Handle();

	protected AudioClip m_SoundToPlay;

	protected AudioClip m_LastPlayedSound;

	private vp_FPPlayerEventHandler m_FPPlayer;

	private vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = m_Player as vp_FPPlayerEventHandler;
			}
			return m_FPPlayer;
		}
	}

	protected override void Start()
	{
		base.Start();
		m_CanClimbAgain = Time.time;
	}

	public override bool TryInteract(vp_PlayerEventHandler player)
	{
		if (!base.enabled)
		{
			return false;
		}
		if (!(player is vp_FPPlayerEventHandler))
		{
			return false;
		}
		if (Time.time < m_CanClimbAgain)
		{
			return false;
		}
		if (m_IsClimbing)
		{
			m_Player.Climb.TryStop();
			return false;
		}
		if (m_Player == null)
		{
			m_Player = player;
		}
		if (m_Player.Interactable.Get() != null)
		{
			return false;
		}
		if (m_Controller == null)
		{
			m_Controller = m_Player.GetComponent<vp_FPController>();
		}
		if (m_Player.Velocity.Get().magnitude > MinVelocityToClimb)
		{
			return false;
		}
		if (m_Camera == null)
		{
			m_Camera = m_Player.GetComponentInChildren<vp_FPCamera>();
		}
		if (Sounds.AudioSource == null)
		{
			Sounds.AudioSource = m_Player.GetComponent<AudioSource>();
		}
		if (m_Player != null)
		{
			Register(m_Player);
		}
		m_Player.Interactable.Set(this);
		return m_Player.Climb.TryStart();
	}

	protected virtual void OnStart_Climb()
	{
		m_Controller.PhysicsGravityModifier = 0f;
		m_Camera.SetRotation(m_Camera.Transform.eulerAngles, stopZoomAndSprings: false);
		m_Player.Jump.Stop();
		FPPlayer.InputAllowGameplay.Set(o: false);
		m_Player.Stop.Send();
		m_LastWeaponEquipped = m_Player.CurrentWeaponIndex.Get();
		m_Player.SetWeapon.TryStart(0);
		m_Player.Interactable.Set(null);
		PlaySound(Sounds.MountSounds);
		if (m_Controller.Transform.GetComponent<Collider>().enabled && m_Transform.GetComponent<Collider>().enabled)
		{
			Log.Error("UFPS Ignoring Collider 1");
			Physics.IgnoreCollision(m_Controller.Transform.GetComponent<Collider>(), m_Transform.GetComponent<Collider>(), ignore: true);
		}
		StartCoroutine("LineUp");
	}

	protected virtual void PlaySound(List<AudioClip> sounds)
	{
		if (Sounds.AudioSource == null || sounds == null || sounds.Count == 0)
		{
			return;
		}
		do
		{
			m_SoundToPlay = sounds[UnityEngine.Random.Range(0, sounds.Count)];
			if (m_SoundToPlay == null)
			{
				return;
			}
		}
		while (m_SoundToPlay == m_LastPlayedSound && sounds.Count > 1);
		if (sounds == Sounds.ClimbingSounds)
		{
			Sounds.AudioSource.pitch = UnityEngine.Random.Range(Sounds.ClimbingPitch.x, Sounds.ClimbingPitch.y) * Time.timeScale;
		}
		else
		{
			Sounds.AudioSource.pitch = 1f;
		}
		Sounds.AudioSource.PlayOneShot(m_SoundToPlay);
		m_LastPlayedSound = m_SoundToPlay;
	}

	protected virtual IEnumerator LineUp()
	{
		Vector3 startPosition = m_Player.Position.Get();
		Vector3 endPosition2 = GetNewPosition();
		Quaternion startingRotation = m_Camera.transform.rotation;
		Quaternion endRotation = Quaternion.LookRotation(-m_Transform.forward);
		bool flag = m_Controller.Transform.position.y > m_Transform.GetComponent<Collider>().bounds.center.y;
		if (flag)
		{
			endPosition2 += Vector3.down * m_Controller.CharacterController.height;
		}
		else
		{
			endPosition2 += m_Controller.Transform.up * (m_Controller.CharacterController.height / 2f);
		}
		endRotation = ((!flag || !(m_Transform.InverseTransformDirection(-FPPlayer.CameraLookDirection.Get()).z > 0f)) ? Quaternion.Euler(new Vector3(-45f, endRotation.eulerAngles.y, endRotation.eulerAngles.z)) : Quaternion.Euler(new Vector3(45f, endRotation.eulerAngles.y, endRotation.eulerAngles.z)));
		endPosition2 = new Vector3(m_Transform.GetComponent<Collider>().bounds.center.x, endPosition2.y, m_Transform.GetComponent<Collider>().bounds.center.z);
		endPosition2 += m_Transform.forward;
		float t = 0f;
		float duration = Vector3.Distance(m_Controller.Transform.position, endPosition2) / ((!flag) ? (MountSpeed / 1.25f) : MountSpeed);
		while (t < 1f)
		{
			t += Time.deltaTime / duration;
			Vector3 o = Vector3.Lerp(startPosition, endPosition2, t);
			m_Player.Position.Set(o);
			Quaternion quaternion = Quaternion.Slerp(startingRotation, endRotation, t);
			m_Player.Rotation.Set(new Vector2(MountAutoRotatePitch ? quaternion.eulerAngles.x : m_Player.Rotation.Get().x, quaternion.eulerAngles.y));
			yield return new WaitForEndOfFrame();
		}
		m_CachedDirection = m_Camera.Transform.forward;
		m_CachedRotation = m_Player.Rotation.Get();
		m_IsClimbing = true;
	}

	protected virtual void OnStop_Climb()
	{
		m_Player.Interactable.Set(null);
		FPPlayer.InputAllowGameplay.Set(o: true);
		m_Player.SetWeapon.TryStart(m_LastWeaponEquipped);
		Unregister(m_Player);
		m_CanClimbAgain = Time.time + ClimbAgainTimeout;
		if (m_Controller.Transform.GetComponent<Collider>().enabled && m_Transform.GetComponent<Collider>().enabled)
		{
			Log.Error("UFPS Ignoring Collider 2");
			Physics.IgnoreCollision(m_Controller.Transform.GetComponent<Collider>(), m_Transform.GetComponent<Collider>(), ignore: false);
		}
		PlaySound(Sounds.DismountSounds);
		Vector3 vector = m_Controller.Transform.forward * DismountForce;
		if (m_Transform.GetComponent<Collider>().bounds.center.y < m_Player.Position.Get().y)
		{
			vector *= 2f;
			vector.y = DismountForce * 0.5f;
		}
		else
		{
			vector = -vector * 0.5f;
		}
		m_Player.Stop.Send();
		m_Controller.AddForce(vector);
		m_IsClimbing = false;
		m_Player.SetState("Default");
		StartCoroutine("RestorePitch");
	}

	protected virtual IEnumerator RestorePitch()
	{
		float t = 0f;
		while (t < 1f && FPPlayer.InputRawLook.Get().y == 0f)
		{
			t += Time.deltaTime;
			m_Player.Rotation.Set(Vector2.Lerp(m_Player.Rotation.Get(), new Vector2(0f, m_Player.Rotation.Get().y), t));
			yield return new WaitForEndOfFrame();
		}
	}

	protected virtual bool CanStart_Interact()
	{
		if (m_IsClimbing)
		{
			m_Player.Climb.TryStop();
		}
		return true;
	}

	protected virtual void FixedUpdate()
	{
		Climbing();
	}

	protected virtual void Update()
	{
		InputJump();
	}

	protected virtual void OnStart_Dead()
	{
		FinishInteraction();
	}

	public override void FinishInteraction()
	{
		if (m_IsClimbing)
		{
			m_Player.Climb.TryStop();
		}
	}

	protected virtual void Climbing()
	{
		if (m_Player == null || !m_IsClimbing)
		{
			return;
		}
		m_Controller.PhysicsGravityModifier = 0f;
		m_Camera.RotationYawLimit = new Vector2(m_CachedRotation.y - 90f, m_CachedRotation.y + 90f);
		m_Camera.RotationPitchLimit = new Vector2(90f, -90f);
		Vector3 newPosition = GetNewPosition();
		Vector3 vector = Vector3.zero;
		float num = m_Player.Rotation.Get().x / 90f;
		float num2 = MinimumClimbSpeed / ClimbSpeed;
		if (Mathf.Abs(num) < num2)
		{
			num = ((num > 0f) ? num2 : (num2 * -1f));
		}
		if (num < 0f)
		{
			vector = Vector3.up * (0f - num);
		}
		else if (num > 0f)
		{
			vector = Vector3.down * num;
		}
		float num3 = ClimbSpeed;
		float num4 = (vector * m_Player.InputClimbVector.Get()).y;
		if (SimpleClimb)
		{
			vector = Vector3.up;
			num3 *= 0.75f;
			num4 = m_Player.InputClimbVector.Get();
		}
		if ((num4 > 0f && newPosition.y > GetTopOfCollider(m_Transform) - m_Controller.CharacterController.height * 0.25f) || (num4 < 0f && m_Controller.Grounded && m_Controller.GroundTransform.GetInstanceID() != m_Transform.GetInstanceID()))
		{
			m_Player.Climb.TryStop();
			return;
		}
		if (m_Player.InputClimbVector.Get() == 0f)
		{
			m_ClimbingSoundTimer.Cancel();
		}
		if (m_Player.InputClimbVector.Get() != 0f && !m_ClimbingSoundTimer.Active && Sounds.ClimbingSounds.Count > 0)
		{
			float num5 = Mathf.Abs(5f / vector.y * (Time.deltaTime * 5f) / Sounds.ClimbingSoundSpeed);
			vp_Timer.In(SimpleClimb ? (num5 * 3f) : num5, delegate
			{
				PlaySound(Sounds.ClimbingSounds);
			}, m_ClimbingSoundTimer);
		}
		newPosition += vector * num3 * Time.deltaTime * m_Player.InputClimbVector.Get();
		m_Player.Position.Set(Vector3.Slerp(m_Controller.Transform.position, newPosition, Time.deltaTime * num3));
	}

	protected virtual Vector3 GetNewPosition()
	{
		Vector3 vector = m_Controller.Transform.position;
		Physics.Raycast(new Ray(m_Controller.Transform.position, m_CachedDirection), out var hitInfo, DistanceToClimbable * 4f);
		if (hitInfo.collider != null && hitInfo.transform.GetInstanceID() == m_Transform.GetInstanceID() && (hitInfo.distance > DistanceToClimbable || hitInfo.distance < DistanceToClimbable))
		{
			vector = (vector - hitInfo.point).normalized * DistanceToClimbable + hitInfo.point;
		}
		return vector;
	}

	protected virtual void InputJump()
	{
		if (m_IsClimbing && !(m_Player == null) && (FPPlayer.InputGetButton.Send("Jump") || FPPlayer.InputGetButtonDown.Send("Interact")))
		{
			m_Player.Climb.TryStop();
			if (FPPlayer.InputGetButton.Send("Jump"))
			{
				m_Controller.AddForce(-m_Controller.Transform.forward * m_Controller.MotorJumpForce);
			}
		}
	}

	public static float GetTopOfCollider(Transform t)
	{
		return t.position.y + t.GetComponent<Collider>().bounds.size.y / 2f;
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterActivity("Interact", null, null, CanStart_Interact, null, null, null);
		eventHandler.RegisterActivity("Climb", OnStart_Climb, OnStop_Climb, null, null, null, null);
		eventHandler.RegisterActivity("Dead", OnStart_Dead, null, null, null, null, null);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterActivity("Interact", null, null, CanStart_Interact, null, null, null);
		eventHandler.UnregisterActivity("Climb", OnStart_Climb, OnStop_Climb, null, null, null, null);
		eventHandler.UnregisterActivity("Dead", OnStart_Dead, null, null, null, null, null);
	}
}
