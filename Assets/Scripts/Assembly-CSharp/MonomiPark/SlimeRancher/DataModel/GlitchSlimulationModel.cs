using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchSlimulationModel
	{
		public Dictionary<string, GlitchTeleportDestinationModel> teleporters = new Dictionary<string, GlitchTeleportDestinationModel>();

		public Dictionary<string, GlitchImpostoDirectorModel> impostoDirectors = new Dictionary<string, GlitchImpostoDirectorModel>();

		private readonly GameModel parent;

		public GameModel.IdContainer<GlitchImpostoModel> impostos { get; private set; }

		public GameModel.IdContainer<GlitchTarrNodeModel> nodes { get; private set; }

		public GameModel.IdContainer<GlitchStorageModel> storage { get; private set; }

		public GlitchSlimulationModel(GameModel parent)
		{
			this.parent = parent;
			nodes = new GameModel.IdContainer<GlitchTarrNodeModel>(parent);
			impostos = new GameModel.IdContainer<GlitchImpostoModel>(parent);
			storage = new GameModel.IdContainer<GlitchStorageModel>(parent);
		}

		public void Init()
		{
			nodes.Init();
			storage.Init();
			foreach (GlitchTeleportDestinationModel value in teleporters.Values)
			{
				value.Init();
			}
			impostos.Init();
			foreach (GlitchImpostoDirectorModel value2 in impostoDirectors.Values)
			{
				value2.Init();
			}
		}

		public void NotifyParticipants()
		{
			nodes.NotifyParticipants();
			storage.NotifyParticipants();
			foreach (GlitchTeleportDestinationModel value in teleporters.Values)
			{
				value.NotifyParticipants();
			}
			impostos.NotifyParticipants();
			foreach (GlitchImpostoDirectorModel value2 in impostoDirectors.Values)
			{
				value2.NotifyParticipants();
			}
		}

		public void Register(GlitchTeleportDestinationModel.Participant participant)
		{
			GlitchTeleportDestinationModel glitchTeleportDestinationModel = new GlitchTeleportDestinationModel(participant);
			teleporters[participant.id] = glitchTeleportDestinationModel;
			if (!parent.expectingPush)
			{
				glitchTeleportDestinationModel.Init();
				glitchTeleportDestinationModel.NotifyParticipants();
			}
		}

		public void Unregister(GlitchTeleportDestinationModel.Participant participant)
		{
			teleporters.Remove(participant.id);
		}

		public void Register(GlitchImpostoDirectorModel.Participant participant)
		{
			GlitchImpostoDirectorModel glitchImpostoDirectorModel = new GlitchImpostoDirectorModel(participant);
			impostoDirectors[participant.id] = glitchImpostoDirectorModel;
			if (!parent.expectingPush)
			{
				glitchImpostoDirectorModel.Init();
				glitchImpostoDirectorModel.NotifyParticipants();
			}
		}

		public void Unregister(GlitchImpostoDirectorModel.Participant participant)
		{
			impostoDirectors.Remove(participant.id);
		}

		public void Push(GlitchSlimulationV02 persistence)
		{
			foreach (KeyValuePair<string, GlitchTeleportDestinationV01> teleporter in persistence.teleporters)
			{
				if (!teleporters.ContainsKey(teleporter.Key))
				{
					Log.Warning("Failed to get GlitchTeleportDestinationV01 for persistence id.", "id", teleporter.Key);
				}
				else
				{
					teleporters[teleporter.Key].Push(teleporter.Value);
				}
			}
			foreach (KeyValuePair<string, GlitchTarrNodeV01> node in persistence.nodes)
			{
				if (!nodes.ContainsKey(node.Key))
				{
					Log.Warning("Failed to get GlitchTarrNodeV01 for persistence id.", "id", node.Key);
				}
				else
				{
					nodes[node.Key].Push(node.Value);
				}
			}
			foreach (KeyValuePair<string, GlitchImpostoDirectorV01> impostoDirector in persistence.impostoDirectors)
			{
				if (!impostoDirectors.ContainsKey(impostoDirector.Key))
				{
					Log.Warning("Failed to get GlitchImpostoDirectorV01 for persistence id.", "id", impostoDirector.Key);
				}
				else
				{
					impostoDirectors[impostoDirector.Key].Push(impostoDirector.Value);
				}
			}
			foreach (KeyValuePair<string, GlitchImpostoV01> imposto in persistence.impostos)
			{
				if (!impostos.ContainsKey(imposto.Key))
				{
					Log.Warning("Failed to get GlitchImpostoV01 for persistence id.", "id", imposto.Key);
				}
				else
				{
					impostos[imposto.Key].Push(imposto.Value);
				}
			}
			foreach (KeyValuePair<string, GlitchStorageV01> item in persistence.storage)
			{
				if (!storage.ContainsKey(item.Key))
				{
					Log.Warning("Failed to get GlitchStorageV01 for persistence id.", "id", item.Key);
				}
				else
				{
					storage[item.Key].Push(item.Value);
				}
			}
		}

		public void Pull(out GlitchSlimulationV02 persistence)
		{
			persistence = new GlitchSlimulationV02();
			persistence.teleporters = teleporters.ToDictionary((KeyValuePair<string, GlitchTeleportDestinationModel> p) => p.Key, (KeyValuePair<string, GlitchTeleportDestinationModel> p) => p.Value.Pull());
			persistence.nodes = nodes.StaticInstances.ToDictionary((KeyValuePair<string, GlitchTarrNodeModel> p) => p.Key, (KeyValuePair<string, GlitchTarrNodeModel> p) => p.Value.Pull());
			persistence.impostoDirectors = impostoDirectors.ToDictionary((KeyValuePair<string, GlitchImpostoDirectorModel> p) => p.Key, (KeyValuePair<string, GlitchImpostoDirectorModel> p) => p.Value.Pull());
			persistence.impostos = impostos.StaticInstances.ToDictionary((KeyValuePair<string, GlitchImpostoModel> p) => p.Key, (KeyValuePair<string, GlitchImpostoModel> p) => p.Value.Pull());
			persistence.storage = storage.StaticInstances.ToDictionary((KeyValuePair<string, GlitchStorageModel> p) => p.Key, (KeyValuePair<string, GlitchStorageModel> p) => p.Value.Pull());
		}
	}
}
