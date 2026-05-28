using System;
using System.Collections.Generic;
using System.Linq;

namespace MonomiPark.SlimeRancher.Persist
{
	public abstract class VersionedPersistedDataSet_Profile<T> : VersionedPersistedDataSet<T> where T : PersistedDataSet, new()
	{
		private List<uint> upgrades;

		public override string Identifier => "SRPF";

		public void RunUpgradeActions(AutoSaveDirector director)
		{
			if (upgrades == null)
			{
				return;
			}
			if (!director.SaveProfile())
			{
				throw new Exception("Failed to persist profile before running upgrade actions.");
			}
			foreach (VersionedPersistedDataSet_Profile.UpgradeAction item in from v in upgrades
				where VersionedPersistedDataSet_Profile.UpgradeActions.ContainsKey(v)
				select VersionedPersistedDataSet_Profile.UpgradeActions[v])
			{
				item(director);
			}
		}

		protected override void UpgradeFrom(T previous)
		{
			upgrades = upgrades ?? new List<uint>();
			upgrades.Add(previous.Version);
		}
	}
	public class VersionedPersistedDataSet_Profile
	{
		public delegate void UpgradeAction(AutoSaveDirector director);

		public static readonly Dictionary<uint, UpgradeAction> UpgradeActions = new Dictionary<uint, UpgradeAction> { 
		{
			6u,
			delegate(AutoSaveDirector director)
			{
				foreach (GameData.Summary item in from kv in director.AvailableGamesByGameName()
					select kv.Value.FirstOrDefault((GameData.Summary s) => !s.isInvalid) into s
					where s != null
					select s)
				{
					AnalyticsUtil.CustomEvent("SessionEnded", new Dictionary<string, object>
					{
						{ "Game.Id", item.name },
						{ "Game.Mode", item.gameMode },
						{
							"Time.WorldTime",
							AnalyticsUtil.GetEventData((float)item.day * 86400f, 0)
						}
					}, includeDefaultEventData: false);
				}
			}
		} };
	}
}
