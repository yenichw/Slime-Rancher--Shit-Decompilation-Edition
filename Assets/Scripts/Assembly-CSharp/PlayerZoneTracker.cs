using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using RichPresence;
using UnityEngine;

public class PlayerZoneTracker : MonoBehaviour
{
	private RegionMember member;

	private HashSet<ZoneDirector.Zone> lastZones;

	private PediaDirector pediaDir;

	private TutorialDirector tutDir;

	private AchievementsDirector achieveDir;

	private MusicDirector musicDir;

	private ProgressDirector progressDir;

	private PlayerState playerState;

	private Director richPresenceDir;

	public void Awake()
	{
		pediaDir = SRSingleton<SceneContext>.Instance.PediaDirector;
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		progressDir = SRSingleton<SceneContext>.Instance.ProgressDirector;
		playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		musicDir = SRSingleton<GameContext>.Instance.MusicDirector;
		richPresenceDir = SRSingleton<GameContext>.Instance.RichPresenceDirector;
		member = GetComponent<RegionMember>();
		member.regionsChanged += OnRegionsChanged;
	}

	public void Start()
	{
		OnEntered(GetAnyCurrZone(ZoneDirector.Zone.RANCH));
	}

	public void OnDisable()
	{
		member.regionsChanged -= OnRegionsChanged;
	}

	public void OnRegionsChanged(List<Region> leftRegions, List<Region> joinRegions)
	{
		HashSet<ZoneDirector.Zone> hashSet = ZoneDirector.Zones(member);
		if (lastZones != null)
		{
			foreach (ZoneDirector.Zone item in hashSet)
			{
				if (!lastZones.Contains(item))
				{
					OnEntered(item);
				}
			}
			foreach (ZoneDirector.Zone lastZone in lastZones)
			{
				if (!hashSet.Contains(lastZone))
				{
					OnExited(lastZone);
				}
			}
		}
		lastZones = hashSet;
		musicDir.OnRegionsChanged(member);
	}

	private void OnEntered(ZoneDirector.Zone zone)
	{
		switch (zone)
		{
		case ZoneDirector.Zone.REEF:
			pediaDir.MaybeShowPopup(PediaDirector.Id.REEF);
			break;
		case ZoneDirector.Zone.QUARRY:
			pediaDir.MaybeShowPopup(PediaDirector.Id.QUARRY);
			achieveDir.AddToStat(AchievementsDirector.IntStat.VISITED_QUARRY, 1);
			break;
		case ZoneDirector.Zone.MOSS:
			pediaDir.MaybeShowPopup(PediaDirector.Id.MOSS);
			achieveDir.AddToStat(AchievementsDirector.IntStat.VISITED_MOSS, 1);
			break;
		case ZoneDirector.Zone.DESERT:
			pediaDir.MaybeShowPopup(PediaDirector.Id.DESERT);
			achieveDir.AddToStat(AchievementsDirector.IntStat.VISITED_DESERT, 1);
			break;
		case ZoneDirector.Zone.RUINS:
			pediaDir.MaybeShowPopup(PediaDirector.Id.RUINS);
			achieveDir.AddToStat(AchievementsDirector.IntStat.VISITED_RUINS, 1);
			break;
		case ZoneDirector.Zone.WILDS:
			pediaDir.MaybeShowPopup(PediaDirector.Id.WILDS);
			break;
		case ZoneDirector.Zone.OGDEN_RANCH:
			progressDir.AddProgress(ProgressDirector.ProgressType.ENTER_ZONE_OGDEN_RANCH);
			pediaDir.MaybeShowPopup(PediaDirector.Id.OGDEN_RETREAT);
			break;
		case ZoneDirector.Zone.VALLEY:
			pediaDir.MaybeShowPopup(PediaDirector.Id.VALLEY);
			tutDir.RemoveHidden(TutorialDirector.Id.RACE_START);
			tutDir.MaybeShowPopup(TutorialDirector.Id.RACE_START);
			break;
		case ZoneDirector.Zone.MOCHI_RANCH:
			progressDir.AddProgress(ProgressDirector.ProgressType.ENTER_ZONE_MOCHI_RANCH);
			pediaDir.MaybeShowPopup(PediaDirector.Id.MOCHI_MANOR);
			break;
		case ZoneDirector.Zone.SLIMULATIONS:
			pediaDir.MaybeShowPopup(PediaDirector.Id.SLIMULATIONS_WORLD);
			tutDir.MaybeShowPopup(TutorialDirector.Id.SLIMULATIONS_START_1);
			tutDir.MaybeShowPopup(TutorialDirector.Id.SLIMULATIONS_START_2);
			break;
		case ZoneDirector.Zone.VIKTOR_LAB:
			progressDir.AddProgress(ProgressDirector.ProgressType.ENTER_ZONE_VIKTOR_LAB);
			pediaDir.MaybeShowPopup(PediaDirector.Id.VIKTOR_LAB);
			break;
		}
		playerState.OnEnteredZone(zone);
		richPresenceDir.SetRichPresence(new InZoneData(zone));
		AnalyticsUtil.CustomEvent("ZoneEntered", new Dictionary<string, object> { { "ZoneId", zone } });
	}

	private void OnExited(ZoneDirector.Zone zone)
	{
		playerState.OnExitedZone(zone);
	}

	private ZoneDirector.Zone GetAnyCurrZone(ZoneDirector.Zone defaultToZone)
	{
		return Randoms.SHARED.Pick(ZoneDirector.Zones(member), defaultToZone);
	}

	public ZoneDirector.Zone GetCurrentZone()
	{
		if (ZoneDirector.Zones(member).Count == 0)
		{
			return ZoneDirector.Zone.NONE;
		}
		ZoneDirector.Zone result = ZoneDirector.Zone.NONE;
		int num = int.MaxValue;
		foreach (ZoneDirector.Zone item in ZoneDirector.Zones(member))
		{
			if ((int)item < num)
			{
				result = item;
				num = (int)item;
			}
		}
		return result;
	}
}
