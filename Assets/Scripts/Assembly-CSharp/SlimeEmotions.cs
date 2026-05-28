using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class SlimeEmotions : RegisteredActorBehaviour, RegistryUpdateable, ActorModel.Participant
{
	public enum Emotion
	{
		HUNGER = 0,
		AGITATION = 1,
		FEAR = 2,
		NONE = 3
	}

	[Serializable]
	public class EmotionState : ISerializable
	{
		public Emotion emotion;

		public float currVal;

		public float defVal;

		public float sensitivity;

		public float recoveryPerGameHour;

		public bool enabled { get; private set; }

		public EmotionState(Emotion emotion, float currVal, float defVal, float sensitivity, float recoveryPerGameHour)
		{
			this.emotion = emotion;
			this.currVal = currVal;
			this.defVal = defVal;
			this.sensitivity = sensitivity;
			this.recoveryPerGameHour = recoveryPerGameHour;
			enabled = true;
		}

		public void SetEnabled(bool enabled)
		{
			if (this.enabled != enabled)
			{
				this.enabled = enabled;
				currVal = (this.enabled ? defVal : 0f);
			}
		}

		protected EmotionState(SerializationInfo info, StreamingContext context)
		{
			emotion = (Emotion)info.GetInt32("emotion");
			currVal = info.GetSingle("currVal");
			defVal = info.GetSingle("defVal");
			sensitivity = info.GetSingle("sensitivity");
			recoveryPerGameHour = info.GetSingle("recoveryPerGameHour");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("currVal", currVal);
			info.AddValue("defVal", defVal);
			info.AddValue("sensitivity", sensitivity);
			info.AddValue("recoveryPerGameHour", recoveryPerGameHour);
			info.AddValue("emotion", (int)emotion);
		}
	}

	public const float HUNGRY_CUTOFF = 0.666f;

	public const float STARVING_CUTOFF = 0.99f;

	public const float ANGRY_CUTOFF = 0.9f;

	public const float TERRIFIED_CUTOFF = 0.99f;

	public EmotionState initHunger = new EmotionState(Emotion.HUNGER, 0.5f, 1f, 1f, 0.5f);

	public EmotionState initAgitation = new EmotionState(Emotion.AGITATION, 0f, 0f, 1f, 0.333f);

	public EmotionState initFear = new EmotionState(Emotion.FEAR, 0f, 0f, 1f, 5f);

	private double lastUpdateTime;

	private TimeDirector timeDir;

	private RegionMember member;

	private List<MusicBoxRegion> musicBoxes = new List<MusicBoxRegion>();

	private float modHungerFactor = 1f;

	private const float STARVING_AGITATION_PER_HOUR = 0.416667f;

	private float FAVORITE_TOY_FACTOR = 0.5f;

	private float NON_FAVORITE_TOY_FACTOR = 0.25f;

	private float POLLEN_SOURCE_AGITATION_PER_HOUR = 0.416667f;

	private int nearbyFavoriteToyCount;

	private int nearbyToyCount;

	private int pollenSourceCount;

	private SlimeModel model;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		member = GetComponent<RegionMember>();
		lastUpdateTime = timeDir.HoursFromNowOrStart(0f);
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.ModDirector.RegisterModsListener(OnModsChanged);
		}
	}

	public override void Start()
	{
		base.Start();
		if (!Identifiable.IsTarr(Identifiable.GetId(base.gameObject)) && member.IsInRegion(RegionRegistry.RegionSetId.SLIMULATIONS))
		{
			SetEmotionEnabled(Emotion.HUNGER, enabled: false);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		musicBoxes.Clear();
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.ModDirector.UnregisterModsListener(OnModsChanged);
		}
	}

	public void InitModel(ActorModel model)
	{
		((SlimeModel)model).MaybeSetInitEmotions(initAgitation, initFear, initHunger);
	}

	public void SetModel(ActorModel model)
	{
		this.model = (SlimeModel)model;
	}

	private void OnModsChanged()
	{
		modHungerFactor = SRSingleton<SceneContext>.Instance.ModDirector.SlimeHungerFactor();
	}

	public void RegistryUpdate()
	{
		UpdateToTime(timeDir.WorldTime());
	}

	public void UpdateToTime(double worldTime)
	{
		float num = (float)((worldTime - lastUpdateTime) * 0.00027777778450399637);
		if (num <= 0f)
		{
			return;
		}
		EmotionState[] allEmotions = model.allEmotions;
		foreach (EmotionState emotionState in allEmotions)
		{
			if (emotionState.enabled)
			{
				float num2 = emotionState.recoveryPerGameHour + GetBaseRecoveryAdjustment(emotionState.emotion);
				float recoveryFactor = GetRecoveryFactor(emotionState.emotion);
				float num3 = ((!(num2 < 0f)) ? (num2 * recoveryFactor) : (num2 / recoveryFactor));
				if (emotionState.currVal > emotionState.defVal)
				{
					emotionState.currVal = Mathf.Max(emotionState.defVal, emotionState.currVal - num3 * num);
				}
				else if (emotionState.currVal < emotionState.defVal)
				{
					emotionState.currVal = Mathf.Min(emotionState.defVal, emotionState.currVal + num3 * num);
				}
				else if (emotionState.defVal == 0f && num3 < 0f)
				{
					emotionState.currVal = Mathf.Max(emotionState.defVal, emotionState.currVal - num3 * num);
				}
				else if (emotionState.defVal == 1f && num3 < 0f)
				{
					emotionState.currVal = Mathf.Min(emotionState.defVal, emotionState.currVal + num3 * num);
				}
				emotionState.currVal = Mathf.Clamp01(emotionState.currVal);
			}
		}
		lastUpdateTime = worldTime;
	}

	private float GetBaseRecoveryAdjustment(Emotion emotion)
	{
		if (emotion == Emotion.AGITATION)
		{
			float num = 0f;
			if (GetCurr(Emotion.HUNGER) >= 0.99f || GetCurr(Emotion.FEAR) >= 0.99f)
			{
				num -= 0.416667f;
			}
			if (pollenSourceCount > 0)
			{
				num -= POLLEN_SOURCE_AGITATION_PER_HOUR;
			}
			return num;
		}
		return 0f;
	}

	private float GetRecoveryFactor(Emotion emotion)
	{
		switch (emotion)
		{
		case Emotion.AGITATION:
		{
			float num = 1f;
			if (musicBoxes.Count > 0)
			{
				num += 1f;
			}
			if (nearbyFavoriteToyCount > 0)
			{
				num += FAVORITE_TOY_FACTOR;
			}
			else if (nearbyToyCount > 0)
			{
				num += NON_FAVORITE_TOY_FACTOR;
			}
			return num;
		}
		case Emotion.HUNGER:
			return modHungerFactor;
		default:
			return 1f;
		}
	}

	private EmotionState GetEmotion(Emotion emotion)
	{
		switch (emotion)
		{
		case Emotion.AGITATION:
			return model.emotionAgitation;
		case Emotion.FEAR:
			return model.emotionFear;
		case Emotion.HUNGER:
			return model.emotionHunger;
		default:
			return null;
		}
	}

	public bool Adjust(Emotion emotion, float adjust)
	{
		EmotionState emotion2 = GetEmotion(emotion);
		if (emotion2 != null && emotion2.enabled)
		{
			emotion2.currVal = Mathf.Clamp(emotion2.currVal + adjust, 0f, 1f);
			return true;
		}
		return false;
	}

	public void SetAll(SlimeEmotions other)
	{
		foreach (Emotion value in Enum.GetValues(typeof(Emotion)))
		{
			EmotionState emotion2 = GetEmotion(value);
			if (emotion2 != null && emotion2.enabled)
			{
				emotion2.currVal = other.GetEmotion(value).currVal;
			}
		}
	}

	public void SetAll(Dictionary<Emotion, float> other)
	{
		foreach (KeyValuePair<Emotion, float> item in other)
		{
			EmotionState emotion = GetEmotion(item.Key);
			if (emotion != null && emotion.enabled)
			{
				emotion.currVal = item.Value;
			}
		}
	}

	public IEnumerable<EmotionState> GetAll()
	{
		return model.allEmotions;
	}

	public float GetCurr(Emotion emotion)
	{
		if (emotion != Emotion.NONE)
		{
			return GetEmotion(emotion).currVal;
		}
		return 1f;
	}

	public float GetMax()
	{
		float num = 0f;
		EmotionState[] allEmotions = model.allEmotions;
		foreach (EmotionState emotionState in allEmotions)
		{
			num = Mathf.Max(num, emotionState.currVal);
		}
		return num;
	}

	public void AddMusicBox(MusicBoxRegion box)
	{
		musicBoxes.Add(box);
	}

	public void RemoveMusicBox(MusicBoxRegion box)
	{
		musicBoxes.Remove(box);
	}

	public void AddNearbyToy(bool isFavorite)
	{
		if (isFavorite)
		{
			nearbyFavoriteToyCount++;
		}
		else
		{
			nearbyToyCount++;
		}
	}

	public void RemoveNearbyToy(bool isFavorite)
	{
		if (isFavorite)
		{
			nearbyFavoriteToyCount = Math.Max(0, nearbyFavoriteToyCount - 1);
		}
		else
		{
			nearbyToyCount = Math.Max(0, nearbyToyCount - 1);
		}
	}

	public void AddPollenSource()
	{
		pollenSourceCount++;
	}

	public void RemovePollenSource()
	{
		pollenSourceCount = Math.Max(0, pollenSourceCount - 1);
	}

	public void SetEmotionEnabled(Emotion emotion, bool enabled)
	{
		GetEmotion(emotion)?.SetEnabled(enabled);
	}
}
