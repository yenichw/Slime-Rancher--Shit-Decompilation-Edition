using System;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class ReactToShock : SRBehaviour
{
	[Flags]
	public enum PlortSounds
	{
		NONE = 0,
		SUCCESS = 1,
		FAILURE = 2
	}

	public Identifiable.Id plortId;

	public GameObject produceFX;

	[Tooltip("Duration, in game hours, to cooldown before a reaction is available again.")]
	public float cooldownHours;

	[Tooltip("FX to show while the cooldown is active. (optional)")]
	public GameObject cooldownFX;

	[Tooltip("Prefab to instantiate an electric field.")]
	public GameObject electricFieldPrefab;

	private GameObject electricField;

	private DamagePlayerOnTouch damagePlayer;

	[Tooltip("SFX played when the slime is hit and successfully produces a plort.")]
	public SECTR_AudioCue onHitSuccessCue;

	[Tooltip("SFX played when the slime is hit and fails to produce a plort.")]
	public SECTR_AudioCue onHitFailureCue;

	private double nextReactionTime;

	private GameObject cooldownFXInstance;

	private bool checkAppearance;

	private RegionMember regionMember;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	private GameObject plortObj;

	private TimeDirector timeDirector;

	private SlimeAppearance normalAppearance;

	private SlimeAppearance shockedAppearance;

	private static Vector3 LOCAL_PRODUCE_LOC = new Vector3(0f, 0.5f, 0f);

	public void Awake()
	{
		damagePlayer = GetComponent<DamagePlayerOnTouch>();
		regionMember = GetComponent<RegionMember>();
		slimeAppearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		slimeAppearanceApplicator.OnAppearanceChanged += UpdateAppearances;
		if (slimeAppearanceApplicator.Appearance != null)
		{
			UpdateAppearances(slimeAppearanceApplicator.Appearance);
		}
		plortObj = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(plortId);
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public void Update()
	{
		if (timeDirector.HasReached(nextReactionTime))
		{
			if (cooldownFXInstance != null)
			{
				SRBehaviour.RecycleAndStopFX(cooldownFXInstance);
				cooldownFXInstance = null;
			}
			damagePlayer.SetBlocked(blocked: true);
		}
		if (checkAppearance && timeDirector.HasReached(nextReactionTime))
		{
			slimeAppearanceApplicator.Appearance = normalAppearance;
			slimeAppearanceApplicator.ApplyAppearance();
			checkAppearance = false;
		}
	}

	public void DoShock(Identifiable.Id id)
	{
		switch (id)
		{
		case Identifiable.Id.VALLEY_AMMO_1:
			MaybeCreatePlorts(1);
			break;
		case Identifiable.Id.VALLEY_AMMO_2:
			MaybeCreatePlorts(2, PlortSounds.SUCCESS);
			break;
		case Identifiable.Id.VALLEY_AMMO_4:
			MaybeCreatePlorts(1, PlortSounds.SUCCESS);
			if (electricField == null)
			{
				electricField = UnityEngine.Object.Instantiate(electricFieldPrefab);
				electricField.transform.SetParent(base.transform, worldPositionStays: false);
			}
			electricField.GetComponent<QuicksilverElectricField>().ResetDeathTime();
			break;
		case Identifiable.Id.VALLEY_AMMO_3:
			break;
		}
	}

	public bool MaybeCreatePlorts(int count)
	{
		PlortSounds mask = PlortSounds.SUCCESS | PlortSounds.FAILURE;
		return MaybeCreatePlorts(count, mask);
	}

	public bool MaybeCreatePlorts(int count, PlortSounds mask)
	{
		if (timeDirector.HasReached(nextReactionTime))
		{
			for (int i = 0; i < count; i++)
			{
				Vector3 position = base.transform.TransformPoint(LOCAL_PRODUCE_LOC);
				SRBehaviour.InstantiateActor(plortObj, regionMember.setId, position, base.transform.rotation);
				if (!(produceFX != null))
				{
					continue;
				}
				RecolorSlimeMaterial[] componentsInChildren = SRBehaviour.SpawnAndPlayFX(produceFX, position, base.transform.rotation).GetComponentsInChildren<RecolorSlimeMaterial>();
				if (componentsInChildren != null && componentsInChildren.Length != 0)
				{
					SlimeAppearance.Palette appearancePalette = slimeAppearanceApplicator.GetAppearancePalette();
					RecolorSlimeMaterial[] array = componentsInChildren;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].SetColors(appearancePalette.Top, appearancePalette.Middle, appearancePalette.Bottom);
					}
				}
			}
			if (cooldownFX != null)
			{
				if (cooldownFXInstance != null)
				{
					SRBehaviour.RecycleAndStopFX(cooldownFXInstance);
				}
				cooldownFXInstance = SRBehaviour.SpawnAndPlayFX(cooldownFX, base.gameObject);
			}
			damagePlayer.SetBlocked(blocked: false);
			slimeAppearanceApplicator.Appearance = shockedAppearance;
			slimeAppearanceApplicator.ApplyAppearance();
			checkAppearance = true;
			PlaySFX(onHitSuccessCue, PlortSounds.SUCCESS, mask);
			nextReactionTime = timeDirector.HoursFromNow(cooldownHours);
			return true;
		}
		PlaySFX(onHitFailureCue, PlortSounds.FAILURE, mask);
		return false;
	}

	private void UpdateAppearances(SlimeAppearance baseAppearance)
	{
		if (!(baseAppearance == normalAppearance) && !(baseAppearance == shockedAppearance))
		{
			normalAppearance = baseAppearance;
			shockedAppearance = baseAppearance.ShockedAppearance;
			if (checkAppearance)
			{
				slimeAppearanceApplicator.Appearance = shockedAppearance;
				slimeAppearanceApplicator.ApplyAppearance();
			}
		}
	}

	private bool PlaySFX(SECTR_AudioCue cue, PlortSounds expected, PlortSounds mask)
	{
		if ((mask & expected) != 0)
		{
			SECTR_AudioSystem.Play(cue, base.transform.position, loop: false);
			return true;
		}
		return false;
	}
}
