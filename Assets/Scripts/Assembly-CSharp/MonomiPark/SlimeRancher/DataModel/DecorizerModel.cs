using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class DecorizerModel
	{
		public interface Participant
		{
			void InitModel(DecorizerModel model);

			void SetModel(DecorizerModel model);

			void OnDecorizerRemoved(Identifiable.Id id);
		}

		public class Settings
		{
			public Identifiable.Id selected;
		}

		private List<Participant> participants = new List<Participant>();

		public static readonly IEnumerable<HashSet<Identifiable.Id>> ITEM_CLASSES = new HashSet<Identifiable.Id>[3]
		{
			Identifiable.ECHO_CLASS,
			Identifiable.ECHO_NOTE_CLASS,
			Identifiable.ORNAMENT_CLASS
		};

		private ReferenceCount<Identifiable.Id> contents = new ReferenceCount<Identifiable.Id>(Identifiable.idComparer);

		private Dictionary<string, Settings> settings = new Dictionary<string, Settings>();

		public void AddParticipant(Participant participant)
		{
			participants.Add(participant);
		}

		public void Init()
		{
			participants.ForEach(delegate(Participant p)
			{
				p.InitModel(this);
			});
		}

		public void NotifyParticipants()
		{
			participants.ForEach(delegate(Participant p)
			{
				p.SetModel(this);
			});
		}

		public bool Add(Identifiable.Id id)
		{
			if (ITEM_CLASSES.Any((HashSet<Identifiable.Id> c) => c.Contains(id)))
			{
				contents.Increment(id);
				return true;
			}
			return false;
		}

		public bool Remove(Identifiable.Id id)
		{
			if (contents.GetCount(id) > 0)
			{
				contents.Decrement(id);
				participants.ForEach(delegate(Participant p)
				{
					p.OnDecorizerRemoved(id);
				});
				return true;
			}
			return false;
		}

		public int GetCount(Identifiable.Id id)
		{
			return contents.GetCount(id);
		}

		public Settings GetSettings(string id)
		{
			if (!settings.ContainsKey(id))
			{
				settings[id] = new Settings();
			}
			return settings[id];
		}

		public void Push(DecorizerV01 persistence)
		{
			contents = new ReferenceCount<Identifiable.Id>(persistence.contents, Identifiable.idComparer);
			settings = persistence.settings.ToDictionary((KeyValuePair<string, DecorizerSettingsV01> kv) => kv.Key, (KeyValuePair<string, DecorizerSettingsV01> kv) => new Settings
			{
				selected = kv.Value.selected
			});
		}

		public void Pull(out DecorizerV01 persistence)
		{
			persistence = new DecorizerV01
			{
				contents = contents.ToDictionary((KeyValuePair<Identifiable.Id, int> kv) => kv.Key, (KeyValuePair<Identifiable.Id, int> kv) => kv.Value, Identifiable.idComparer),
				settings = settings.ToDictionary((KeyValuePair<string, Settings> kv) => kv.Key, (KeyValuePair<string, Settings> kv) => new DecorizerSettingsV01
				{
					selected = kv.Value.selected
				})
			};
		}
	}
}
