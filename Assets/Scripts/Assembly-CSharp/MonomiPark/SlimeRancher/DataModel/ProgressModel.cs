using System.Collections.Generic;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class ProgressModel
	{
		public interface Participant
		{
			void InitModel(ProgressModel model);

			void SetModel(ProgressModel model);
		}

		public Dictionary<ProgressDirector.ProgressType, int> progressDict = new Dictionary<ProgressDirector.ProgressType, int>(ProgressDirector.progressTypeComparer);

		public Dictionary<ProgressDirector.ProgressTrackerId, double> delayedProgressTimeDict = new Dictionary<ProgressDirector.ProgressTrackerId, double>(ProgressDirector.progressTrackerIdComparer);

		private Participant participant;

		public void SetParticipant(Participant participant)
		{
			this.participant = participant;
		}

		public void Init()
		{
			if (participant != null)
			{
				participant.InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			if (participant != null)
			{
				participant.SetModel(this);
			}
		}

		public void Reset()
		{
			progressDict.Clear();
			delayedProgressTimeDict.Clear();
		}

		public void Push(Dictionary<ProgressDirector.ProgressType, int> dict, Dictionary<ProgressDirector.ProgressTrackerId, double> delayedDict)
		{
			foreach (KeyValuePair<ProgressDirector.ProgressType, int> item in dict)
			{
				if (!progressDict.ContainsKey(item.Key) || progressDict[item.Key] < item.Value)
				{
					progressDict[item.Key] = item.Value;
				}
			}
			foreach (KeyValuePair<ProgressDirector.ProgressTrackerId, double> item2 in delayedDict)
			{
				if (delayedProgressTimeDict.ContainsKey(item2.Key))
				{
					delayedProgressTimeDict[item2.Key] = item2.Value;
				}
				else
				{
					Log.Warning("No delayed progress tracker for type, skipping: " + item2.Key);
				}
			}
		}

		public void Pull(out Dictionary<ProgressDirector.ProgressType, int> progressDict, out Dictionary<ProgressDirector.ProgressTrackerId, double> delayedProgressDict)
		{
			progressDict = new Dictionary<ProgressDirector.ProgressType, int>(this.progressDict, ProgressDirector.progressTypeComparer);
			delayedProgressDict = new Dictionary<ProgressDirector.ProgressTrackerId, double>(delayedProgressTimeDict, ProgressDirector.progressTrackerIdComparer);
		}

		public double GetDelayedProgressTime(ProgressDirector.ProgressTrackerId trackerId)
		{
			if (delayedProgressTimeDict.ContainsKey(trackerId))
			{
				return delayedProgressTimeDict[trackerId];
			}
			return double.PositiveInfinity;
		}

		public void SetDelayedProgressTime(ProgressDirector.ProgressTrackerId trackerId, double time)
		{
			delayedProgressTimeDict[trackerId] = time;
		}

		public bool HasProgress(ProgressDirector.ProgressType type)
		{
			if (progressDict.ContainsKey(type))
			{
				return progressDict[type] > 0;
			}
			return false;
		}

		public int GetProgress(ProgressDirector.ProgressType type)
		{
			if (progressDict.ContainsKey(type))
			{
				return progressDict[type];
			}
			return 0;
		}

		public void OnNewGameLoaded(PlayerState.GameMode currGameMode)
		{
			if (currGameMode == PlayerState.GameMode.TIME_LIMIT_V2)
			{
				progressDict = new Dictionary<ProgressDirector.ProgressType, int>(ProgressDirector.progressTypeComparer)
				{
					{
						ProgressDirector.ProgressType.UNLOCK_DOCKS,
						1
					},
					{
						ProgressDirector.ProgressType.UNLOCK_GROTTO,
						1
					},
					{
						ProgressDirector.ProgressType.UNLOCK_LAB,
						1
					},
					{
						ProgressDirector.ProgressType.UNLOCK_OVERGROWTH,
						1
					}
				};
			}
		}
	}
}
