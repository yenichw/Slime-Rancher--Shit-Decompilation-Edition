using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class HolidayDirector : MonoBehaviour, HolidayModel.Participant
{
	[Serializable]
	public class MonthAndDay : IEquatable<MonthAndDay>
	{
		public int month;

		public int day;

		public MonthAndDay(int month, int day)
		{
			this.month = month;
			this.day = day;
		}

		public bool Equals(MonthAndDay other)
		{
			if (month == other.month)
			{
				return day == other.day;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (month << 8) ^ day;
		}
	}

	[Serializable]
	public class OrnamentEntry
	{
		[Serializable]
		public class WeightEntry
		{
			public float weight;

			public Identifiable.Id id;
		}

		public MonthAndDay date;

		public List<WeightEntry> weights;

		public Dictionary<Identifiable.Id, float> weightDict = new Dictionary<Identifiable.Id, float>(Identifiable.idComparer);

		public void Init()
		{
			foreach (WeightEntry weight in weights)
			{
				weightDict[weight.id] = weight.weight;
			}
		}
	}

	public List<OrnamentEntry> ornaments = new List<OrnamentEntry>();

	private Dictionary<MonthAndDay, OrnamentEntry> ornamentDict = new Dictionary<MonthAndDay, OrnamentEntry>();

	private HolidayModel model;

	public void InitForLevel()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterHoliday(this);
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnSceneLoaded_EventGordos));
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Combine(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnSceneLoaded_EchoNoteGordo));
	}

	public void Awake()
	{
		foreach (OrnamentEntry ornament in ornaments)
		{
			ornament.Init();
			ornamentDict[ornament.date] = ornament;
		}
	}

	public void InitModel(HolidayModel model)
	{
	}

	public void SetModel(HolidayModel model)
	{
		this.model = model;
	}

	public IEnumerable<Identifiable.Id> GetCurrOrnament()
	{
		if (DateTime.Today.Year == 2017)
		{
			MonthAndDay key = new MonthAndDay(DateTime.Today.Month, DateTime.Today.Day);
			if (ornamentDict.ContainsKey(key))
			{
				yield return Randoms.SHARED.Pick(ornamentDict[key].weightDict, Identifiable.Id.NONE);
			}
		}
	}

	private void OnSceneLoaded_EventGordos(SceneContext ctx)
	{
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnSceneLoaded_EventGordos));
		if (Levels.isSpecial() || !ctx.GameModeConfig.GetModeSettings().enableEventGordos)
		{
			model.eventGordos.Clear();
			return;
		}
		IDateProvider dateProvider = SRSingleton<SystemContext>.Instance.DateProvider;
		DateTime currentDate = dateProvider.GetToday();
		Log.Debug("Current System Date For Events", "Date", currentDate.ToString("yyyy-MM-dd"));
		bool flag = false;
		int num = model.eventGordos.RemoveWhere((HolidayModel.EventGordo e) => !e.IsLiveAsOf(currentDate));
		flag = flag || num > 0;
		foreach (HolidayModel.EventGordo item in HolidayModel.EventGordo.INSTANCES.Where((HolidayModel.EventGordo e) => e.IsLiveAsOf(currentDate)))
		{
			GordoModel gordoModel = SRSingleton<SceneContext>.Instance.GameModel.GetGordoModel(item.objectId);
			if (gordoModel == null)
			{
				Log.Error("Failed to active EventGordo.", "event", item);
				SentrySdk.CaptureMessage("Failed to active EventGordo!");
			}
			else
			{
				bool flag2 = model.eventGordos.Add(item);
				gordoModel.EventGordoActivate(flag2);
				flag = flag || flag2;
			}
		}
		if (flag)
		{
			StartCoroutine(ResetCratesAfterFrame());
		}
	}

	private IEnumerator ResetCratesAfterFrame()
	{
		yield return new WaitForEndOfFrame();
		ZoneDirector.Zone currentZone = SRSingleton<SceneContext>.Instance.Player.GetComponent<PlayerZoneTracker>().GetCurrentZone();
		ZoneDirector zoneDirector = ZoneDirector.zones.Get(currentZone);
		if (zoneDirector != null)
		{
			zoneDirector.ResetCrates();
		}
	}

	private void OnSceneLoaded_EchoNoteGordo(SceneContext ctx)
	{
		SceneContext.onSceneLoaded = (SceneContext.SceneLoadDelegate)Delegate.Remove(SceneContext.onSceneLoaded, new SceneContext.SceneLoadDelegate(OnSceneLoaded_EchoNoteGordo));
		if (Levels.isSpecial() || !ctx.GameModeConfig.GetModeSettings().enableEchoNoteGordos)
		{
			model.eventEchoNoteGordos.Clear();
			return;
		}
		IDateProvider dateProvider = SRSingleton<SystemContext>.Instance.DateProvider;
		DateTime currentDate = dateProvider.GetToday();
		Log.Debug("Current System Date For Wiggly Events", "Date", currentDate.ToString("yyyy-MM-dd"));
		model.eventEchoNoteGordos.RemoveWhere((HolidayModel.EventEchoNoteGordo e) => !e.IsLiveAsOf(currentDate));
		foreach (HolidayModel.EventEchoNoteGordo item in HolidayModel.EventEchoNoteGordo.INSTANCES.Where((HolidayModel.EventEchoNoteGordo e) => e.IsLiveAsOf(currentDate)))
		{
			EchoNoteGordoModel echoNoteGordoModel = SRSingleton<SceneContext>.Instance.GameModel.GetEchoNoteGordoModel(item.objectId);
			if (echoNoteGordoModel == null)
			{
				Log.Error("Failed to active EchoNoteGordo.", "id", item.objectId);
				SentrySdk.CaptureMessage("Failed to active EchoNoteGordo!");
			}
			else
			{
				bool isFirstActivation = model.eventEchoNoteGordos.Add(item);
				echoNoteGordoModel.Activate(isFirstActivation);
			}
		}
	}
}
