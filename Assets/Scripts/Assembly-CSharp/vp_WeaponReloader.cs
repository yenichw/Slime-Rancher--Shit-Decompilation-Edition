using UnityEngine;

public class vp_WeaponReloader : MonoBehaviour, EventHandlerRegistrable
{
	protected vp_Weapon m_Weapon;

	protected vp_PlayerEventHandler m_Player;

	protected AudioSource m_Audio;

	public AudioClip SoundReload;

	public float ReloadDuration = 1f;

	protected virtual float OnValue_CurrentWeaponReloadDuration => ReloadDuration;

	protected virtual void Awake()
	{
		m_Audio = GetComponent<AudioSource>();
		m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
	}

	protected virtual void Start()
	{
		m_Weapon = base.transform.GetComponent<vp_Weapon>();
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

	protected virtual bool CanStart_Reload()
	{
		if (!m_Player.CurrentWeaponWielded.Get())
		{
			return false;
		}
		if (m_Player.CurrentWeaponMaxAmmoCount.Get() != 0 && m_Player.CurrentWeaponAmmoCount.Get() == m_Player.CurrentWeaponMaxAmmoCount.Get())
		{
			return false;
		}
		if (m_Player.CurrentWeaponClipCount.Get() < 1)
		{
			return false;
		}
		return true;
	}

	protected virtual void OnStart_Reload()
	{
		m_Player.Reload.AutoDuration = m_Player.CurrentWeaponReloadDuration.Get();
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(SoundReload);
		}
	}

	protected virtual void OnStop_Reload()
	{
		m_Player.RefillCurrentWeapon.Try();
	}

	protected virtual float Get_CurrentWeaponReloadDuration()
	{
		return ReloadDuration;
	}

	public void Register(vp_EventHandler eventHandler)
	{
		eventHandler.RegisterValue("CurrentWeaponReloadDuration", Get_CurrentWeaponReloadDuration, null);
		eventHandler.RegisterActivity("Reload", OnStart_Reload, OnStop_Reload, CanStart_Reload, null, null, null);
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		eventHandler.UnregisterValue("CurrentWeaponReloadDuration", Get_CurrentWeaponReloadDuration, null);
		eventHandler.UnregisterActivity("Reload", OnStart_Reload, OnStop_Reload, CanStart_Reload, null, null, null);
	}
}
