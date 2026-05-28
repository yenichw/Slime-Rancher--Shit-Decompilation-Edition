using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class PlayerRanchnessTracker : MonoBehaviour
{
	private bool lastOnHomeRanch;

	private RegionMember member;

	private AchievementsDirector achieveDir;

	private TimeDirector timeDir;

	private TutorialDirector tutDir;

	public void Awake()
	{
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		tutDir = SRSingleton<SceneContext>.Instance.TutorialDirector;
		member = GetComponent<RegionMember>();
		member.regionsChanged += InitSectorsChanged;
	}

	public void OnDestroy()
	{
		member.regionsChanged -= InitSectorsChanged;
	}

	private void InitSectorsChanged(List<Region> left, List<Region> joined)
	{
		lastOnHomeRanch = CellDirector.IsOnHomeRanch(member);
		member.regionsChanged += delegate
		{
			bool flag = CellDirector.IsOnHomeRanch(member);
			if (!flag && lastOnHomeRanch)
			{
				achieveDir.SetStat(AchievementsDirector.GameDoubleStat.LAST_LEFT_RANCH, timeDir.WorldTime());
				tutDir.OnLeftRanch();
			}
			else if (flag && !lastOnHomeRanch)
			{
				achieveDir.SetStat(AchievementsDirector.GameDoubleStat.LAST_ENTERED_RANCH, timeDir.WorldTime());
				tutDir.OnEnteredRanch();
			}
			lastOnHomeRanch = flag;
		};
		member.regionsChanged -= InitSectorsChanged;
	}
}
