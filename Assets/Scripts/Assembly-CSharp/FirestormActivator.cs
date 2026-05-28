using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class FirestormActivator : MonoBehaviour, WorldModel.Participant
{
	public enum Mode
	{
		IDLE = 0,
		PREPARING = 1,
		ACTIVE = 2
	}

	public SECTR_AudioCue firestormWind;

	public float minColumnDelayHrs = 0.05f;

	public float maxColumnDelayHrs = 0.1f;

	private SECTR_AudioCueInstance activeFirestormWind;

	private TimeDirector timeDir;

	private MusicDirector musicDir;

	private AmbianceDirector ambianceDir;

	private ProgressDirector progressDir;

	private RegionMember member;

	private Overlay overlay;

	private double nextPlayerCheck;

	private WorldModel worldModel;

	private const float MIN_STORM_PERIOD = 8f;

	private const float MAX_STORM_PERIOD = 15f;

	private const float MIN_INIT_STORM_PERIOD = 3f;

	private const float MAX_INIT_STORM_PERIOD = 5f;

	private const float MIN_STORM_LENGTH = 1.5f;

	private const float MAX_STORM_LENGTH = 2f;

	private const float ENTER_DESERT_STORM_DELAY_HRS = 24f;

	public const float END_STORM_RAMPDOWN = 0.5f;

	public const float PREPARE_DELAY = 0.5f;

	private const float PLAYER_CHECK_PERIOD = 1f / 12f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		musicDir = SRSingleton<GameContext>.Instance.MusicDirector;
		ambianceDir = SRSingleton<SceneContext>.Instance.AmbianceDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
	}

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterWorldParticipant(this);
	}

	public void Start()
	{
		member = GetComponent<RegionMember>();
		overlay = member.GetComponentInChildren<Overlay>();
		if (double.IsPositiveInfinity(worldModel.nextFirestormTime))
		{
			if (progressDir.HasProgress(ProgressDirector.ProgressType.UNLOCK_DESERT))
			{
				worldModel.nextFirestormTime = timeDir.HoursFromNowOrStart(Randoms.SHARED.GetInRange(8f, 15f) * 0.5f);
				return;
			}
			ProgressDirector progressDirector = progressDir;
			progressDirector.onProgressChanged = (ProgressDirector.OnProgressChanged)Delegate.Combine(progressDirector.onProgressChanged, new ProgressDirector.OnProgressChanged(CheckProgress));
		}
	}

	public void OnDestroy()
	{
		ProgressDirector progressDirector = progressDir;
		progressDirector.onProgressChanged = (ProgressDirector.OnProgressChanged)Delegate.Remove(progressDirector.onProgressChanged, new ProgressDirector.OnProgressChanged(CheckProgress));
	}

	public void InitModel(WorldModel model)
	{
		model.nextFirestormTime = double.PositiveInfinity;
	}

	public void SetModel(WorldModel model)
	{
		worldModel = model;
		worldModel.nextFirecolumnTime = ((worldModel.currFirestormMode == Mode.PREPARING) ? timeDir.HoursFromNow(0.5f) : timeDir.WorldTime());
	}

	private void CheckProgress()
	{
		if (progressDir.HasProgress(ProgressDirector.ProgressType.UNLOCK_DESERT) && double.IsPositiveInfinity(worldModel.nextFirestormTime))
		{
			worldModel.nextFirestormTime = timeDir.HoursFromNowOrStart(Randoms.SHARED.GetInRange(3f, 5f));
		}
	}

	public void Update()
	{
		MaybeShutdownFirestorm();
		MaybeStartFirestorm();
		MaybeTriggerNearbyColumns();
		MaybeUpdatePlayerState();
	}

	private void MaybeShutdownFirestorm()
	{
		if (timeDir.HasReached(worldModel.endFirestormTime))
		{
			worldModel.currFirestormMode = Mode.IDLE;
			worldModel.nextFirecolumnTime = double.PositiveInfinity;
		}
	}

	private void MaybeStartFirestorm()
	{
		if (timeDir.HasReached(worldModel.nextFirestormTime))
		{
			worldModel.nextFirecolumnTime = timeDir.HoursFromNow(0.5f);
			worldModel.currFirestormMode = Mode.PREPARING;
			worldModel.endFirestormTime = timeDir.HoursFromNow(Randoms.SHARED.GetInRange(1.5f, 2f));
			worldModel.endFirecolumnsTime = worldModel.endFirestormTime - 1800.0;
			worldModel.nextFirestormTime = timeDir.HoursFromNow(Randoms.SHARED.GetInRange(8f, 15f));
		}
	}

	private void MaybeTriggerNearbyColumns()
	{
		if (!timeDir.HasReached(worldModel.nextFirecolumnTime) || timeDir.HasReached(worldModel.endFirecolumnsTime))
		{
			return;
		}
		worldModel.currFirestormMode = Mode.ACTIVE;
		List<FireColumn> list = new List<FireColumn>();
		foreach (Region region in member.regions)
		{
			FirestormController component = region.GetComponent<FirestormController>();
			if (component != null)
			{
				component.AddColumnsToList(list);
			}
		}
		if (list.Count > 0)
		{
			Dictionary<FireColumn, float> dictionary = new Dictionary<FireColumn, float>();
			foreach (FireColumn item in list)
			{
				if (!item.IsInOasis() && !item.IsFireActive())
				{
					dictionary[item] = 1f / Mathf.Max(0.1f, (item.transform.position - base.transform.position).sqrMagnitude);
				}
			}
			FireColumn fireColumn = Randoms.SHARED.Pick(dictionary, null);
			if (fireColumn != null)
			{
				fireColumn.ActivateFire();
			}
		}
		worldModel.nextFirecolumnTime = timeDir.HoursFromNow(Randoms.SHARED.GetInRange(minColumnDelayHrs, maxColumnDelayHrs));
	}

	public void RequestPlayerStateUpdate()
	{
		nextPlayerCheck = 0.0;
	}

	private void MaybeUpdatePlayerState()
	{
		if (!timeDir.HasReached(nextPlayerCheck))
		{
			return;
		}
		bool flag = false;
		foreach (Region region in member.regions)
		{
			if (region.GetComponent<FirestormController>() != null)
			{
				flag = true;
				break;
			}
		}
		Mode mode = (flag ? worldModel.currFirestormMode : Mode.IDLE);
		if (mode != worldModel.currFirestormAmbianceMode)
		{
			overlay.SetEnableFirestorm(mode != Mode.IDLE);
			ambianceDir.SetFirestormActive(mode != Mode.IDLE);
			musicDir.SetFirestormMode(mode);
			switch (mode)
			{
			case Mode.ACTIVE:
				activeFirestormWind = SECTR_AudioSystem.Play(firestormWind, base.gameObject.transform, Vector3.zero, loop: true);
				break;
			case Mode.IDLE:
				activeFirestormWind.Stop(stopImmediately: false);
				break;
			}
			worldModel.currFirestormAmbianceMode = mode;
		}
		nextPlayerCheck = timeDir.HoursFromNow(1f / 12f);
	}
}
