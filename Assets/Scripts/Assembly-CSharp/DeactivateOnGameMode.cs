using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class DeactivateOnGameMode : SRBehaviour, GameModel.Participant //decompiled by yenichw(github)
{
	[Tooltip("List of game modes that will deactivate the object.")]
	public List<PlayerState.GameMode> blackList;

	private PlayerState.GameMode? mode;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterGameModelParticipant(this);
	}

	public void OnEnable()
	{
		if (mode.HasValue && blackList.Contains(mode.Value))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void InitModel(GameModel model)
	{
	}

	public void SetModel(GameModel model)
	{
		mode = model.currGameMode;
		OnEnable();
	}
}
