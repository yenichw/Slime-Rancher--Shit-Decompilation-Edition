using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class DeactivateOnNotGameMode : SRBehaviour, GameModel.Participant
{
	[Tooltip("List of game modes that will activate the object.")]
	public List<PlayerState.GameMode> whiteList;

	private PlayerState.GameMode? mode;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterGameModelParticipant(this);
	}

	public void OnEnable()
	{
		if (mode.HasValue && !whiteList.Contains(mode.Value))
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
