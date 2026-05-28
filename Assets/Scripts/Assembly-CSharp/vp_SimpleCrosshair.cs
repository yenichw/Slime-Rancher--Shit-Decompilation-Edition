using UnityEngine;

public class vp_SimpleCrosshair : MonoBehaviour, EventHandlerRegistrable
{
	public Texture m_ImageCrosshair;

	public bool HideOnFirstPersonZoom = true;

	public bool HideOnDeath = true;

	protected vp_FPPlayerEventHandler m_Player;

	protected virtual Texture OnValue_Crosshair
	{
		get
		{
			return m_ImageCrosshair;
		}
		set
		{
			m_ImageCrosshair = value;
		}
	}

	protected virtual void Awake()
	{
		m_Player = Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
	}

	protected virtual void OnEnable()
	{
		if (m_Player != null)
		{
			Register(m_Player);
		}
	}

	protected virtual void OnDisable()
	{
		if (m_Player != null)
		{
			Unregister(m_Player);
		}
	}

	private void OnGUI()
	{
		if (!(m_ImageCrosshair == null) && (!HideOnFirstPersonZoom || !m_Player.Zoom.Active || !m_Player.IsFirstPerson.Get()) && (!HideOnDeath || !m_Player.Dead.Active))
		{
			GUI.color = new Color(1f, 1f, 1f, 0.8f);
			GUI.DrawTexture(new Rect((float)Screen.width * 0.5f - (float)m_ImageCrosshair.width * 0.5f, (float)Screen.height * 0.5f - (float)m_ImageCrosshair.height * 0.5f, m_ImageCrosshair.width, m_ImageCrosshair.height), m_ImageCrosshair);
			GUI.color = Color.white;
		}
	}

	protected virtual Texture Get_Crosshair()
	{
		return m_ImageCrosshair;
	}

	protected virtual void Set_Crosshair(Texture value)
	{
		m_ImageCrosshair = value;
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterValue("Crosshair", Get_Crosshair, Set_Crosshair);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterValue("Crosshair", Get_Crosshair, Set_Crosshair);
	}
}
