using UnityEngine;

public class vp_FPEarthquake : MonoBehaviour, EventHandlerRegistrable
{
	protected Vector3 m_CameraEarthQuakeForce;

	protected float m_Endtime;

	protected Vector2 m_Magnitude = Vector2.zero;

	private vp_FPPlayerEventHandler m_FPPlayer;

	private vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
			}
			return m_FPPlayer;
		}
	}

	protected virtual Vector3 OnValue_CameraEarthQuakeForce
	{
		get
		{
			return m_CameraEarthQuakeForce;
		}
		set
		{
			m_CameraEarthQuakeForce = value;
		}
	}

	protected virtual void OnEnable()
	{
		if (FPPlayer != null)
		{
			Register(FPPlayer);
		}
	}

	protected virtual void OnDisable()
	{
		if (FPPlayer != null)
		{
			Unregister(FPPlayer);
		}
	}

	protected void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			UpdateEarthQuake();
		}
	}

	protected void UpdateEarthQuake()
	{
		if (!FPPlayer.CameraEarthQuake.Active)
		{
			m_CameraEarthQuakeForce = Vector3.zero;
			return;
		}
		m_CameraEarthQuakeForce = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(1f), m_Magnitude.x * (Vector3.right + Vector3.forward) * Mathf.Min(m_Endtime - Time.time, 1f) * Time.timeScale);
		m_CameraEarthQuakeForce.y = 0f;
		if (Random.value < 0.3f * Time.timeScale)
		{
			m_CameraEarthQuakeForce.y = Random.Range(0f, m_Magnitude.y * 0.35f) * Mathf.Min(m_Endtime - Time.time, 1f);
		}
	}

	protected virtual void OnStart_CameraEarthQuake()
	{
		Vector3 vector = (Vector3)FPPlayer.CameraEarthQuake.Argument;
		m_Magnitude.x = vector.x;
		m_Magnitude.y = vector.y;
		m_Endtime = Time.time + vector.z;
		FPPlayer.CameraEarthQuake.AutoDuration = vector.z;
	}

	protected virtual void OnMessage_CameraBombShake(float impact)
	{
		FPPlayer.CameraEarthQuake.TryStart(new Vector3(impact * 0.5f, impact * 0.5f, 1f));
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterMessage<float>("CameraBombShake", OnMessage_CameraBombShake);
		eventHandler.RegisterActivity("CameraEarthQuake", OnStart_CameraEarthQuake, null, null, null, null, null);
		eventHandler.RegisterValue("CameraEarthQuakeForce", Get_CameraEarthQuakeForce, Set_CameraEarthQuakeForce);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterMessage<float>("CameraBombShake", OnMessage_CameraBombShake);
		eventHandler.UnregisterActivity("CameraEarthQuake", OnStart_CameraEarthQuake, null, null, null, null, null);
		eventHandler.UnregisterValue("CameraEarthQuakeForce", Get_CameraEarthQuakeForce, Set_CameraEarthQuakeForce);
	}

	protected virtual Vector3 Get_CameraEarthQuakeForce()
	{
		return m_CameraEarthQuakeForce;
	}

	protected virtual void Set_CameraEarthQuakeForce(Vector3 value)
	{
		m_CameraEarthQuakeForce = value;
	}
}
