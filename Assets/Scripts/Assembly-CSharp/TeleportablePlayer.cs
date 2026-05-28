using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class TeleportablePlayer : MonoBehaviour, PlayerModel.Participant
{
	public WeaponVacuum weaponVacuum;

	public vp_FPPlayerEventHandler playerEventHandler;

	public SECTR_AudioCue teleportCue;

	private FirestormActivator firestormActivator;

	private PlayerModel model;

	public void Awake()
	{
		firestormActivator = GetComponent<FirestormActivator>();
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
	}

	public void InitModel(PlayerModel model)
	{
	}

	public void SetModel(PlayerModel model)
	{
		this.model = model;
	}

	public void TeleportTo(Vector3 position, RegionRegistry.RegionSetId regionSetId, Vector3? rotation = null, bool overlayEnabled = true, bool audioEnabled = true)
	{
		weaponVacuum.DropAllVacced();
		playerEventHandler.Position.Set(position);
		if (rotation.HasValue)
		{
			playerEventHandler.Rotation.Set(rotation.Value);
			vp_FPController component = GetComponent<vp_FPController>();
			component.m_Velocity = Vector3.zero;
			component.Stop();
		}
		if (overlayEnabled)
		{
			SRSingleton<Overlay>.Instance.PlayTeleport();
		}
		if (audioEnabled)
		{
			SECTR_AudioSource component2 = GetComponent<SECTR_AudioSource>();
			if (component2 != null)
			{
				component2.Cue = teleportCue;
				component2.Play();
			}
		}
		model.SetCurrRegionSet(regionSetId);
		firestormActivator.RequestPlayerStateUpdate();
	}

	public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
	{
	}

	public void TransformChanged(Vector3 pos, Quaternion rot)
	{
	}

	public void RegisteredPotentialAmmoChanged(Dictionary<PlayerState.AmmoMode, List<GameObject>> registeredPotentialAmmo)
	{
	}

	public void KeyAdded()
	{
	}
}
