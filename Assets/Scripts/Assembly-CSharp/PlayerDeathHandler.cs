using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDeathHandler : SRBehaviour, DeathHandler.Interface, PlayerModel.Participant
{
	public enum DeathType
	{
		DEFAULT = 0,
		SLIMULATIONS = 1,
		RESET_PLAYER_LOCATION = 2,
		EMERGENCY_RETURN = 3
	}

	public delegate void OnAmmoWillClear(PlayerState.AmmoMode mode, Ammo ammo, DeathType deathType);

	public delegate void OnPlayerDeath(DeathType deathType);

	public OnAmmoWillClear onAmmoWillClear;

	public OnPlayerDeath onPlayerDeath;

	public GameObject deathUIPrefab;

	public SECTR_AudioCue screenFadedCue;

	public const float DEATH_FADE_TIME = 1f;

	public const float DEATH_FF_DELAY_TIME = 5f;

	public const float DEATH_MSG_FADE = 0.5f;

	private const float MIN_DEATH_TIME = 390f;

	private bool deathInProgress;

	private PlayerModel model;

	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
	}

	public void InitModel(PlayerModel model)
	{
	}

	public void SetModel(PlayerModel model)
	{
		this.model = model;
	}

	public void OnDeath(DeathHandler.Source source, GameObject sourceGameObject, string stackTrace)
	{
		if (deathInProgress)
		{
			return;
		}
		deathInProgress = true;
		AnalyticsUtil.CustomEvent("PlayerDeath", new Dictionary<string, object>
		{
			{ "DamageType", source },
			{
				"DamageObject",
				AnalyticsUtil.GetEventData(sourceGameObject)
			},
			{
				"HasInventory",
				SRSingleton<SceneContext>.Instance.PlayerState.Ammo.Any((Identifiable.Id id) => !Identifiable.IsLiquid(id))
			}
		});
		RegionRegistry.RegionSetId currentRegionSetId = SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId();
		DeathType deathType = ((source == DeathHandler.Source.EMERGENCY_RETURN) ? DeathType.EMERGENCY_RETURN : ((currentRegionSetId == RegionRegistry.RegionSetId.SLIMULATIONS) ? DeathType.SLIMULATIONS : DeathType.DEFAULT));
		DeathType deathType2 = deathType;
		if (deathType2 == DeathType.SLIMULATIONS)
		{
			LockOnDeath deathLocker = GetComponent<LockOnDeath>();
			deathLocker.Freeze();
			GlitchTerminalAnimator.OnExit(delegate
			{
				StartCoroutine(ResetPlayer(deathType, 0f));
			}, delegate
			{
				deathLocker.Unfreeze();
				deathInProgress = false;
			}, base.gameObject.GetInstanceID());
			return;
		}
		TimeDirector timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		_ = SRSingleton<SceneContext>.Instance.PlayerState;
		GameModeSettings modeSettings = SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings();
		if (modeSettings.hoursTilDawnOnDeath)
		{
			float num = SRSingleton<SceneContext>.Instance.TimeDirector.CurrHour();
			if (num < 10f && num >= 6f)
			{
				SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.DEATH_BEFORE_10AM, 1);
			}
		}
		SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.GameIntStat.DEATHS, 1);
		MusicDirector musicDirector = SRSingleton<GameContext>.Instance.MusicDirector;
		musicDirector.RegisterSuppressor(this);
		if (screenFadedCue != null)
		{
			SECTR_AudioSource component = GetComponent<SECTR_AudioSource>();
			component.Cue = screenFadedCue;
			component.Play();
		}
		GetComponent<LockOnDeath>().LockUntil(Math.Max(modeSettings.hoursTilDawnOnDeath ? timeDirector.GetNextDawnAfterNextDusk() : timeDirector.HoursFromNow(modeSettings.hoursLostOnDeath), timeDirector.WorldTime() + 390.0), 5f, delegate
		{
			musicDirector.DeregisterSuppressor(this);
			deathInProgress = false;
		});
		StartCoroutine(ResetPlayer(deathType));
		StartCoroutine(DisplayDeathUI(deathType));
	}

	public void ResetPlayerLocation(float delayTime, UnityAction onComplete)
	{
		StartCoroutine(ResetPlayer(DeathType.RESET_PLAYER_LOCATION, delayTime, onComplete));
	}

	private IEnumerator ResetPlayer(DeathType deathType, float delayTime = 1f, UnityAction onComplete = null)
	{
		if (delayTime > 0f)
		{
			yield return new WaitForSeconds(delayTime);
		}
		base.gameObject.GetComponentInChildren<WeaponVacuum>().DropAllVacced();
		if (deathType != DeathType.SLIMULATIONS)
		{
			WakeUpDestination wakeUpDestination = GetWakeUpDestination(deathType);
			vp_FPPlayerEventHandler componentInChildren = base.gameObject.GetComponentInChildren<vp_FPPlayerEventHandler>();
			if (componentInChildren != null)
			{
				componentInChildren.Position.Set(wakeUpDestination.transform.position);
				componentInChildren.Rotation.Set(wakeUpDestination.transform.eulerAngles);
				SRSingleton<SceneContext>.Instance.Player.transform.position = wakeUpDestination.transform.position;
				SRSingleton<SceneContext>.Instance.Player.transform.rotation = wakeUpDestination.transform.rotation;
				model.SetCurrRegionSet(wakeUpDestination.GetRegionSetId());
			}
		}
		vp_FPController componentInChildren2 = base.gameObject.GetComponentInChildren<vp_FPController>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.Stop();
		}
		SRSingleton<SceneContext>.Instance.AmbianceDirector.ExitAllLiquid();
		GameModeSettings modeSettings = SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings();
		PlayerState playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		if (modeSettings.pctCurrencyLostOnDeath > 0f)
		{
			playerState.SpendCurrency(Mathf.FloorToInt((float)playerState.GetCurrency() * modeSettings.pctCurrencyLostOnDeath), forcedLoss: true);
		}
		playerState.SetHealth(playerState.GetMaxHealth());
		playerState.SetRad(0);
		playerState.SetEnergy(playerState.GetMaxEnergy());
		playerState.SetAmmoMode(PlayerState.AmmoMode.DEFAULT);
		if (deathType != DeathType.RESET_PLAYER_LOCATION)
		{
			foreach (KeyValuePair<PlayerState.AmmoMode, Ammo> item in playerState.GetAmmoDict())
			{
				if (onAmmoWillClear != null)
				{
					onAmmoWillClear(item.Key, item.Value, deathType);
				}
				item.Value.Clear();
			}
		}
		if (onPlayerDeath != null)
		{
			onPlayerDeath(deathType);
		}
		onComplete?.Invoke();
	}

	private IEnumerator DisplayDeathUI(DeathType deathType)
	{
		yield return new WaitForSeconds(1f);
		GameObject uiObj = UnityEngine.Object.Instantiate(deathUIPrefab);
		TMP_Text promptText = uiObj.transform.Find("PromptText").GetComponent<TMP_Text>();
		TMP_Text subText = uiObj.transform.Find("SubText").GetComponent<TMP_Text>();
		MessageBundle bundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui");
		promptText.text = bundle.Get("m.knocked_out1");
		subText.text = bundle.Get("m.knocked_out2");
		promptText.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
		subText.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
		TutorialDirector tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		tutDir.SuppressTutorials();
		tutDir.OnPlayerDeath(deathType);
		bool endedEarly = false;
		PlayerState.OnEndGame onEndGameFadeDelegate = delegate
		{
			Destroyer.Destroy(uiObj, "PlayerDeathHandler.DisplayDeathUI.onEndGameFadeDelegate");
			endedEarly = true;
		};
		SRSingleton<SceneContext>.Instance.PlayerState.onEndGame += onEndGameFadeDelegate;
		yield return new WaitForSeconds(4f);
		if (!endedEarly)
		{
			promptText.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true);
			subText.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true);
		}
		yield return new WaitForSeconds(0.5f);
		tutDir.UnsuppressTutorials();
		if (!endedEarly)
		{
			Destroyer.Destroy(uiObj, "PlayerDeathHandler.DisplayDeathUI");
		}
		SRSingleton<SceneContext>.Instance.PlayerState.onEndGame -= onEndGameFadeDelegate;
	}

	private WakeUpDestination GetWakeUpDestination(DeathType deathType)
	{
		if (deathType == DeathType.EMERGENCY_RETURN)
		{
			return SRSingleton<SceneContext>.Instance.GetWakeUpDestination();
		}
		RegionMember component = SRSingleton<SceneContext>.Instance.Player.GetComponent<RegionMember>();
		return SRSingleton<SceneContext>.Instance.GetWakeUpDestination(component);
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
