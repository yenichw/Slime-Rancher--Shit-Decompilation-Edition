public class vp_SpeedPickup : vp_Pickup
{
	protected vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	protected override void Update()
	{
		UpdateMotion();
		if (m_Depleted && !m_Audio.isPlaying)
		{
			Remove();
		}
	}

	protected override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (m_Timer.Active)
		{
			return false;
		}
		player.SetState("MegaSpeed");
		vp_Timer.In(RespawnDuration, delegate
		{
			player.SetState("MegaSpeed", setActive: false);
		}, m_Timer);
		return true;
	}
}
