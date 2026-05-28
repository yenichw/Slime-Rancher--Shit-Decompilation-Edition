using System;
using UnityEngine;

public class vp_PlayerRespawner : vp_Respawner, EventHandlerRegistrable
{
	private vp_PlayerEventHandler m_Player;

	public vp_PlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				m_Player = base.transform.GetComponent<vp_PlayerEventHandler>();
			}
			return m_Player;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		if (Player != null)
		{
			Register(Player);
		}
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		if (Player != null)
		{
			Unregister(Player);
		}
	}

	public override void Reset()
	{
		if (Application.isPlaying && !(Player == null))
		{
			Player.Position.Set(Placement.Position);
			Player.Rotation.Set(Placement.Rotation.eulerAngles);
			Player.Stop.Send();
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
