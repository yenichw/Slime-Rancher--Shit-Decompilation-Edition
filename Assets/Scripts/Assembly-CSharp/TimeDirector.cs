using System;
using System.Collections;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class TimeDirector : SRBehaviour, WorldModel.Participant
{
	public delegate void OnUnpauseDelegate();

	public delegate void OnFastForwardChanged(bool isFastForwarding);

	private class PassedTimeDelegate
	{
		public double time;

		public Action action;
	}

	public float secsPerGameDay = 1440f;

	public float ffSecsPerGameDay = 5f;

	public const float START_HOUR = 9f;

	public Sprite daySprite;

	public Sprite nightSprite;

	public Sprite dawnSprite;

	public Sprite duskSprite;

	private float timeFactor;

	private float ffTimeFactor;

	private MessageBundle uiBundle;

	private float savedTimeScale;

	private int pauserCount;

	private int specialPauserCount;

	private static OnUnpauseDelegate onUnpauseDelegate;

	private vp_FPInput input;

	public const float SECS_PER_MIN = 60f;

	public const float MINS_PER_HOUR = 60f;

	public const float HOURS_PER_DAY = 24f;

	public const float MINS_PER_DAY = 1440f;

	public const float SECS_PER_DAY = 86400f;

	public const float SECS_PER_HOUR = 3600f;

	public const float MINS_PER_SEC = 1f / 60f;

	public const float HOURS_PER_SEC = 0.00027777778f;

	public const float HOURS_PER_MIN = 1f / 60f;

	public const float DAYS_PER_SEC = 1.1574074E-05f;

	private string dayFormatString;

	private WorldModel worldModel;

	private List<PassedTimeDelegate> passedTimeDelegates = new List<PassedTimeDelegate>();

	public event OnFastForwardChanged onFastForwardChanged;

	public void Awake()
	{
		if (pauserCount == 0)
		{
			Time.timeScale = 1f;
		}
		timeFactor = 86400f / secsPerGameDay;
		ffTimeFactor = 86400f / ffSecsPerGameDay;
		input = UnityEngine.Object.FindObjectOfType<vp_FPInput>();
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(InitBundles);
		SRSingleton<SceneContext>.Instance.GameModel.RegisterWorldParticipant(this);
	}

	public void InitModel(WorldModel worldModel)
	{
		worldModel.fastForwardUntil = null;
		worldModel.pauseWorldTime = false;
		ResetToStartTime(worldModel);
	}

	public void SetModel(WorldModel worldModel)
	{
		this.worldModel = worldModel;
	}

	private void ResetToStartTime(WorldModel worldModel)
	{
		worldModel.worldTime = 32400.0;
		worldModel.lastWorldTime = worldModel.worldTime;
	}

	public void InitBundles(MessageDirector msgDir)
	{
		uiBundle = msgDir.GetBundle("ui");
		if (uiBundle != null)
		{
			dayFormatString = uiBundle.Get("m.day");
		}
	}

	public void OnUnpause(OnUnpauseDelegate del)
	{
		if (Time.timeScale != 0f)
		{
			del();
		}
		else
		{
			onUnpauseDelegate = (OnUnpauseDelegate)Delegate.Combine(onUnpauseDelegate, del);
		}
	}

	public void ClearOnUnpause(OnUnpauseDelegate del)
	{
		if (onUnpauseDelegate != null)
		{
			onUnpauseDelegate = (OnUnpauseDelegate)Delegate.Remove(onUnpauseDelegate, del);
		}
	}

	public bool ExactlyOnePauser()
	{
		return pauserCount == 1;
	}

	public bool IsFastForwarding()
	{
		return worldModel.fastForwardUntil.HasValue;
	}

	public bool HasPauser()
	{
		return pauserCount > 0;
	}

	public void InitForLevel()
	{
		if (pauserCount == 0)
		{
			SetCursorInUnpausedState();
		}
	}

	public void OnApplicationFocus(bool focused)
	{
		if (focused && pauserCount == 0)
		{
			SetCursorInUnpausedState();
		}
	}

	private void SetCursorInUnpausedState()
	{
		if (Levels.isSpecial())
		{
			EnableCursor(input);
		}
		else
		{
			DisableCursor(input);
		}
	}

	public void Update()
	{
		if (Time.timeScale == 0f || Levels.isSpecialNonAlloc() || worldModel.pauseWorldTime)
		{
			return;
		}
		if (worldModel.fastForwardUntil.HasValue)
		{
			worldModel.worldTime += Time.deltaTime * ffTimeFactor;
			if (worldModel.worldTime >= worldModel.fastForwardUntil.Value)
			{
				worldModel.worldTime = worldModel.fastForwardUntil.Value;
				worldModel.fastForwardUntil = null;
				if (this.onFastForwardChanged != null)
				{
					this.onFastForwardChanged(isFastForwarding: false);
				}
			}
		}
		else
		{
			worldModel.worldTime += Time.deltaTime * timeFactor;
		}
	}

	public void LateUpdate()
	{
		for (int num = passedTimeDelegates.Count - 1; num >= 0; num--)
		{
			if (OnPassedTime(passedTimeDelegates[num].time))
			{
				passedTimeDelegates[num].action();
				passedTimeDelegates.RemoveAt(num);
			}
		}
		worldModel.lastWorldTime = worldModel.worldTime;
	}

	public double DeltaWorldTime()
	{
		if (worldModel.lastWorldTime.HasValue)
		{
			return worldModel.worldTime - worldModel.lastWorldTime.Value;
		}
		return 0.0;
	}

	public void Pause(bool pauseSFX = true, bool pauseSpecialScenes = false)
	{
		bool num = (pauserCount > 0 && !Levels.isSpecial()) || specialPauserCount > 0;
		pauserCount++;
		if (pauseSpecialScenes)
		{
			specialPauserCount++;
		}
		bool flag = (pauserCount > 0 && !Levels.isSpecial()) || specialPauserCount > 0;
		if (!num && flag)
		{
			savedTimeScale = Time.timeScale;
			Time.timeScale = 0f;
		}
		if (pauserCount > 0)
		{
			EnableCursor(input);
		}
	}

	public void DisableCursor(vp_FPInput input)
	{
		if (input != null)
		{
			input.MouseCursorForced = false;
		}
		vp_Utility.LockCursor = true;
	}

	public void EnableCursor(vp_FPInput input)
	{
		if (input != null)
		{
			input.MouseCursorForced = true;
		}
		vp_Utility.LockCursor = false;
	}

	public void Unpause(bool unpauseSFX = true, bool pauseSpecialScenes = false)
	{
		StartCoroutine(DelayedUnpause(unpauseSFX, pauseSpecialScenes));
	}

	private IEnumerator DelayedUnpause(bool unpauseSFX, bool pauseSpecialScenes = false)
	{
		yield return new WaitForEndOfFrame();
		bool flag = (pauserCount > 0 && !Levels.isSpecial()) || specialPauserCount > 0;
		pauserCount--;
		if (pauseSpecialScenes)
		{
			specialPauserCount--;
		}
		bool flag2 = (pauserCount > 0 && !Levels.isSpecial()) || specialPauserCount > 0;
		if (pauserCount < 0)
		{
			Log.Warning("Unpause() called while already unpaused.");
		}
		else if (flag && !flag2)
		{
			Time.timeScale = savedTimeScale;
			_ = unpauseSFX;
			if (onUnpauseDelegate != null)
			{
				onUnpauseDelegate();
				onUnpauseDelegate = null;
			}
		}
		if (pauserCount <= 0)
		{
			DisableCursor(input);
		}
	}

	public double WorldTime()
	{
		return worldModel.worldTime;
	}

	public double HoursFromNow(float hours)
	{
		return worldModel.worldTime + (double)(hours * 3600f);
	}

	public static double HoursFromTime(float hours, double time)
	{
		return time + (double)(hours * 3600f);
	}

	public double HoursFromNowOrStart(float hours)
	{
		return Math.Max((worldModel == null) ? 0.0 : worldModel.worldTime, 32400.0) + (double)(hours * 3600f);
	}

	public bool HasReached(double targetWorldTime)
	{
		return TimeUtil.HasReached(worldModel.worldTime, targetWorldTime);
	}

	public double TimeSince(double time)
	{
		return worldModel.worldTime - time;
	}

	public double HoursUntil(double targetWorldTime)
	{
		return (targetWorldTime - worldModel.worldTime) * 0.00027777778450399637;
	}

	public float CurrDayFraction()
	{
		return DayFraction(worldModel.worldTime);
	}

	public static float DayFraction(double time)
	{
		return (float)(time % 86400.0) * 1.1574074E-05f;
	}

	public int CurrDay()
	{
		return 1 + (int)Math.Floor(worldModel.worldTime * 1.1574074051168282E-05);
	}

	public int CurrDayAfterHour(float hour)
	{
		return 1 + (int)Math.Floor((worldModel.worldTime - (double)(hour * 3600f)) * 1.1574074051168282E-05);
	}

	public float CurrHour()
	{
		return CurrDayFraction() * 24f;
	}

	public float CurrHourOrStart()
	{
		return (float)(Math.Max(worldModel.worldTime, 32400.0) % 86400.0) * 0.00027777778f;
	}

	public int CurrTime()
	{
		return (int)Math.Floor(CurrDayFraction() * 1440f);
	}

	public string CurrDayString()
	{
		int num = CurrDay();
		return string.Format(dayFormatString, num);
	}

	public string CurrTimeString()
	{
		return FormatTime(CurrTime());
	}

	public Sprite CurrTimeIcon()
	{
		double num = CurrDayFraction();
		if (num < 0.20000000298023224 || num > 0.800000011920929)
		{
			return nightSprite;
		}
		if (num > 0.30000001192092896 && num < 0.699999988079071)
		{
			return daySprite;
		}
		if (num > 0.5)
		{
			return duskSprite;
		}
		return dawnSprite;
	}

	public double GetNextHour(float hour)
	{
		return GetHourAfter(0, hour);
	}

	public double GetNextHourAtLeastHalfDay(float hour)
	{
		float num = hour - CurrHourOrStart();
		if (num < 0f)
		{
			num += 24f;
		}
		return GetHourAfter((num < 12f) ? 1 : 0, hour);
	}

	public double GetNextDawn()
	{
		return GetHourAfter(0, 6f);
	}

	public double GetNextDawnAfterNextDusk()
	{
		float num = CurrHour();
		return GetHourAfter((!(num >= 6f) || !(num <= 18f)) ? 1 : 0, 6f);
	}

	public double GetHourAfter(int fullDays, float hour)
	{
		return GetHourAfter(worldModel.worldTime, fullDays, hour);
	}

	public static double GetHourAfter(double fromTime, int fullDays, float hour)
	{
		float num = 0f;
		float num2 = hour / 24f;
		float num3 = DayFraction(fromTime);
		num = ((!(num3 < num2)) ? (num2 - num3 + (float)fullDays + 1f) : (num2 - num3 + (float)fullDays));
		return fromTime + (double)(num * 86400f);
	}

	public void FastForwardTo(double fastForwardUntil)
	{
		worldModel.fastForwardUntil = (long)(fastForwardUntil + 0.5);
		if (this.onFastForwardChanged != null)
		{
			this.onFastForwardChanged(isFastForwarding: true);
		}
	}

	public bool IsAtStart()
	{
		if (worldModel != null)
		{
			return worldModel.worldTime == 32400.0;
		}
		return false;
	}

	public static double HoursFromStart(double hours)
	{
		return (9.0 + hours) * 3600.0;
	}

	public bool OnPassedHour(float hour)
	{
		if (!worldModel.lastWorldTime.HasValue || worldModel.lastWorldTime >= worldModel.worldTime)
		{
			return false;
		}
		double num = worldModel.worldTime * 0.00027777778450399637 % 24.0;
		double num2 = worldModel.lastWorldTime.Value * 0.00027777778450399637 % 24.0;
		if (!(num >= (double)hour) || !(num2 < (double)hour))
		{
			if (num < num2)
			{
				if (!(num2 < (double)hour))
				{
					return (double)hour <= num;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public bool OnPassedTime(double worldTime)
	{
		if (worldModel.lastWorldTime.HasValue && worldModel.lastWorldTime < worldTime)
		{
			return worldTime <= worldModel.worldTime;
		}
		return false;
	}

	public void OnPassedTime(double time, Action action)
	{
		if (HasReached(time))
		{
			action();
		}
		else
		{
			AddPassedTimeDelegate(time, action);
		}
	}

	public void AddPassedTimeDelegate(double time, Action action)
	{
		passedTimeDelegates.Add(new PassedTimeDelegate
		{
			time = time,
			action = action
		});
	}

	public void RemovePassedTimeDelegate(Action action)
	{
		for (int num = passedTimeDelegates.Count - 1; num >= 0; num--)
		{
			if (passedTimeDelegates[num].action == action)
			{
				passedTimeDelegates.RemoveAt(num);
			}
		}
	}

	public string FormatTime(int totalMins)
	{
		int val = totalMins / 60;
		int val2 = totalMins % 60;
		return uiBundle.Get("l.time_hours_mins", StringUtil.Pad(val, 2), StringUtil.Pad(val2, 2));
	}

	public string FormatTimeMinutes(int? minutes)
	{
		if (minutes.HasValue)
		{
			return FormatTime(minutes.Value);
		}
		return uiBundle.Get("l.time_hours_mins_unset");
	}

	public string FormatTimeSeconds(double? seconds)
	{
		if (seconds.HasValue)
		{
			int totalMins = Mathf.CeilToInt((float)seconds.Value * (1f / 60f));
			return FormatTime(totalMins);
		}
		return uiBundle.Get("l.time_hours_mins_unset");
	}
}
