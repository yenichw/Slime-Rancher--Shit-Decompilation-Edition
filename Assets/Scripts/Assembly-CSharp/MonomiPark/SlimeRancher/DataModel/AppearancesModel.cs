using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class AppearancesModel
	{
		public interface Participant
		{
			void InitModel(AppearancesModel model);

			void SetModel(AppearancesModel model);
		}

		public readonly Dictionary<Identifiable.Id, HashSet<SlimeAppearance.AppearanceSaveSet>> unlocks = new Dictionary<Identifiable.Id, HashSet<SlimeAppearance.AppearanceSaveSet>>();

		public readonly Dictionary<Identifiable.Id, SlimeAppearance.AppearanceSaveSet> selections = new Dictionary<Identifiable.Id, SlimeAppearance.AppearanceSaveSet>();

		private Participant participant;

		public AppearanceSelections AppearanceSelections { get; private set; }

		public AppearancesModel()
		{
			AppearanceSelections = new AppearanceSelections();
			AppearanceSelections.onAppearanceUnlocked += OnAppearanceUnlocked;
			AppearanceSelections.onAppearanceSelected += OnAppearanceSelected;
			AppearanceSelections.onAppearanceLocked += OnAppearanceLocked;
		}

		public void SetParticipant(Participant participant)
		{
			if (this.participant != null)
			{
				throw new Exception("Replacing participant on AppearanceModel");
			}
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

		private void OnAppearanceUnlocked(SlimeDefinition slime, SlimeAppearance appearance)
		{
			if (ShouldPersistSlimeAppearanceInfo(slime.IdentifiableId))
			{
				if (!unlocks.TryGetValue(slime.IdentifiableId, out var value))
				{
					value = new HashSet<SlimeAppearance.AppearanceSaveSet>();
					unlocks[slime.IdentifiableId] = value;
				}
				value.Add(appearance.SaveSet);
			}
		}

		private void OnAppearanceSelected(SlimeDefinition slime, SlimeAppearance appearance)
		{
			if (ShouldPersistSlimeAppearanceInfo(slime.IdentifiableId))
			{
				selections[slime.IdentifiableId] = appearance.SaveSet;
			}
		}

		private void OnAppearanceLocked(SlimeDefinition slime, SlimeAppearance appearance)
		{
			if (ShouldPersistSlimeAppearanceInfo(slime.IdentifiableId) && unlocks.TryGetValue(slime.IdentifiableId, out var value))
			{
				value.Remove(appearance.SaveSet);
			}
		}

		public void Push(AppearancesV01 persistence)
		{
			foreach (KeyValuePair<Identifiable.Id, List<SlimeAppearance.AppearanceSaveSet>> unlock in persistence.unlocks)
			{
				if (!unlocks.TryGetValue(unlock.Key, out var value))
				{
					value = new HashSet<SlimeAppearance.AppearanceSaveSet>();
					unlocks[unlock.Key] = value;
				}
				foreach (SlimeAppearance.AppearanceSaveSet item in unlock.Value)
				{
					value.Add(item);
				}
			}
			foreach (KeyValuePair<Identifiable.Id, SlimeAppearance.AppearanceSaveSet> selection in persistence.selections)
			{
				selections[selection.Key] = selection.Value;
			}
		}

		public AppearancesV01 Pull()
		{
			return new AppearancesV01
			{
				unlocks = unlocks.ToDictionary((KeyValuePair<Identifiable.Id, HashSet<SlimeAppearance.AppearanceSaveSet>> keyValuePair) => keyValuePair.Key, (KeyValuePair<Identifiable.Id, HashSet<SlimeAppearance.AppearanceSaveSet>> keyValuePair) => keyValuePair.Value.ToList()),
				selections = selections
			};
		}

		private static bool ShouldPersistSlimeAppearanceInfo(Identifiable.Id slimeId)
		{
			if (!Identifiable.IsLargo(slimeId))
			{
				return !Identifiable.IsTarr(slimeId);
			}
			return false;
		}
	}
}
