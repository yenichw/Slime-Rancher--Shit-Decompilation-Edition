using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using Noise;
using UnityEngine;

public class EconomyDirector : SRBehaviour, WorldModel.Participant
{
	[Serializable]
	public class ValueMap
	{
		public Identifiable accept;

		public float value;

		public float fullSaturation;
	}

	public delegate void DidUpdate();

	public delegate void OnRegisterSold(Identifiable.Id id);

	private class CurrValueEntry
	{
		public readonly float baseValue;

		public float currValue;

		public float prevValue;

		public float fullSaturation;

		public CurrValueEntry(float baseValue, float currValue, float prevValue, float fullSaturation)
		{
			this.baseValue = baseValue;
			this.currValue = currValue;
			this.prevValue = prevValue;
			this.fullSaturation = fullSaturation;
		}
	}

	public ValueMap[] baseValueMap;

	public float saturationSensitivity = 0.05f;

	public float saturationRecovery = 0.25f;

	public float dailyShutdownMins = 5f;

	private double nextUpdateTime;

	private TimeDirector timeDir;

	public DidUpdate didUpdateDelegate;

	public OnRegisterSold onRegisterSold;

	private WorldModel worldModel;

	private Dictionary<Identifiable.Id, CurrValueEntry> currValueMap = new Dictionary<Identifiable.Id, CurrValueEntry>(Identifiable.idComparer);

	public void InitForLevel()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		nextUpdateTime = 0.0;
		if (saturationRecovery < 0f || saturationRecovery > 1f)
		{
			throw new ArgumentException("Saturation Recovery must be [0-1]");
		}
		SRSingleton<SceneContext>.Instance.GameModel.RegisterWorldParticipant(this);
	}

	public void InitModel(WorldModel model)
	{
		model.econSeed = new Randoms().GetFloat(1000000f);
		ValueMap[] array = baseValueMap;
		foreach (ValueMap valueMap in array)
		{
			currValueMap[valueMap.accept.id] = new CurrValueEntry(valueMap.value, valueMap.value, valueMap.value, valueMap.fullSaturation);
			model.marketSaturation[valueMap.accept.id] = valueMap.fullSaturation * 0.5f;
		}
		ResetPrices(model, 0);
	}

	public void SetModel(WorldModel model)
	{
		worldModel = model;
	}

	public void ResetPrices(WorldModel worldModel, int day)
	{
		foreach (KeyValuePair<Identifiable.Id, CurrValueEntry> item in currValueMap)
		{
			if (nextUpdateTime > 0.0)
			{
				worldModel.marketSaturation[item.Key] *= 1f - saturationRecovery;
			}
			float targetValue = GetTargetValue(worldModel, item.Key, item.Value.baseValue, item.Value.fullSaturation, day);
			item.Value.prevValue = item.Value.currValue;
			item.Value.currValue = targetValue;
		}
		if (didUpdateDelegate != null)
		{
			didUpdateDelegate();
		}
	}

	public void Update()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().plortMarketDynamic && timeDir.HasReached(nextUpdateTime))
		{
			ResetPrices(worldModel, timeDir.CurrDay());
			nextUpdateTime = timeDir.GetNextHour(0f);
		}
	}

	public bool IsMarketShutdown()
	{
		if (SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().plortMarketDynamic)
		{
			return timeDir.CurrHour() * 60f < dailyShutdownMins;
		}
		return false;
	}

	public int? GetCurrValue(Identifiable.Id id)
	{
		if (currValueMap.ContainsKey(id))
		{
			return Mathf.RoundToInt(currValueMap[id].currValue);
		}
		return null;
	}

	public int? GetChangeInValue(Identifiable.Id id)
	{
		if (currValueMap.ContainsKey(id))
		{
			return Mathf.RoundToInt(currValueMap[id].currValue) - Mathf.RoundToInt(currValueMap[id].prevValue);
		}
		return null;
	}

	public void RegisterSold(Identifiable.Id id, int count)
	{
		worldModel.marketSaturation[id] += count;
		if (onRegisterSold != null)
		{
			onRegisterSold(id);
		}
	}

	private float GetTargetValue(WorldModel worldModel, Identifiable.Id id, float baseValue, float fullSaturation, float day)
	{
		if (!SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().plortMarketDynamic)
		{
			return baseValue * 1.5f;
		}
		float num = 1f + Mathf.Clamp01((fullSaturation - worldModel.marketSaturation[id]) / fullSaturation);
		float num2 = global::Noise.Noise.PerlinNoise(day, worldModel.econSeed, -10000f, 10f, 0.6f, 1f) + 0.7f;
		float num3 = global::Noise.Noise.PerlinNoise(day, worldModel.econSeed, id.GetHashCode() * 10000, 10f, 0.6f, 1f) + 0.7f;
		return baseValue * num * num2 * num3;
	}
}
