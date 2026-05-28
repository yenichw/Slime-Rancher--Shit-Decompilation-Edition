using System;
using UnityEngine;

public class vp_SimpleFiring : MonoBehaviour, EventHandlerRegistrable
{
	protected vp_PlayerEventHandler m_Player;

	protected virtual void Awake()
	{
		m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
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

	protected virtual void Update()
	{
		if (m_Player.Attack.Active)
		{
			m_Player.Fire.Try();
		}
	}

	public void Register(vp_EventHandler eventHandler)
	{
		throw new NotImplementedException();
	}

	public void Unregister(vp_EventHandler eventHandler)
	{
		throw new NotImplementedException();
	}
}
