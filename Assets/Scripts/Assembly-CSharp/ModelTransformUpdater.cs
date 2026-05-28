using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ModelTransformUpdater : MonoBehaviour, PlayerModel.Participant
{
	private PlayerModel model;

	private vp_FPPlayerEventHandler playerEventHandler;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
		playerEventHandler = GetComponent<vp_FPPlayerEventHandler>();
	}

	public void InitModel(PlayerModel model)
	{
		model.position = base.transform.position;
		model.rotation = base.transform.rotation;
	}

	public void SetModel(PlayerModel model)
	{
		this.model = model;
	}

	public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
	{
	}

	public void TransformChanged(Vector3 pos, Quaternion rot)
	{
		playerEventHandler.Position.Set(pos);
		playerEventHandler.Rotation.Set(rot.eulerAngles);
	}

	public void RegisteredPotentialAmmoChanged(Dictionary<PlayerState.AmmoMode, List<GameObject>> registeredPotentialAmmo)
	{
	}

	public void KeyAdded()
	{
	}

	public void LateUpdate()
	{
		model.position = base.transform.position;
		model.rotation = base.transform.rotation;
	}
}
