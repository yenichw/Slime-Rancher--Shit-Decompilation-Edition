using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchSlimulationV01 : PersistedDataSet
	{
		public Dictionary<string, GlitchTeleportDestinationV01> teleporters;

		public Dictionary<string, GlitchTarrNodeV01> nodes;

		public Dictionary<string, GlitchImpostoDirectorV01> impostoDirectors;

		public Dictionary<string, GlitchImpostoV01> impostos;

		public Dictionary<long, GlitchSlimeDataV01> slimes;

		public override string Identifier => "SRGLITCH";

		public override uint Version => 1u;

		public GlitchSlimulationV01()
		{
			teleporters = new Dictionary<string, GlitchTeleportDestinationV01>();
			nodes = new Dictionary<string, GlitchTarrNodeV01>();
			slimes = new Dictionary<long, GlitchSlimeDataV01>();
			impostoDirectors = new Dictionary<string, GlitchImpostoDirectorV01>();
			impostos = new Dictionary<string, GlitchImpostoV01>();
		}

		protected override void LoadData(BinaryReader reader)
		{
			teleporters = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PersistedDataSet.LoadPersistable<GlitchTeleportDestinationV01>(r));
			nodes = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PersistedDataSet.LoadPersistable<GlitchTarrNodeV01>(r));
			slimes = LoadDictionary(reader, (BinaryReader r) => r.ReadInt64(), (BinaryReader r) => PersistedDataSet.LoadPersistable<GlitchSlimeDataV01>(r));
			impostoDirectors = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PersistedDataSet.LoadPersistable<GlitchImpostoDirectorV01>(r));
			impostos = LoadDictionary(reader, (BinaryReader r) => r.ReadString(), (BinaryReader r) => PersistedDataSet.LoadPersistable<GlitchImpostoV01>(r));
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteDictionary(writer, teleporters, delegate(BinaryWriter w, string key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, GlitchTeleportDestinationV01 val)
			{
				PersistedDataSet.WritePersistable(w, val);
			});
			WriteDictionary(writer, nodes, delegate(BinaryWriter w, string key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, GlitchTarrNodeV01 val)
			{
				PersistedDataSet.WritePersistable(w, val);
			});
			WriteDictionary(writer, slimes, delegate(BinaryWriter w, long key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, GlitchSlimeDataV01 val)
			{
				PersistedDataSet.WritePersistable(w, val);
			});
			WriteDictionary(writer, impostoDirectors, delegate(BinaryWriter w, string key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, GlitchImpostoDirectorV01 val)
			{
				PersistedDataSet.WritePersistable(w, val);
			});
			WriteDictionary(writer, impostos, delegate(BinaryWriter w, string key)
			{
				w.Write(key);
			}, delegate(BinaryWriter w, GlitchImpostoV01 val)
			{
				PersistedDataSet.WritePersistable(w, val);
			});
		}

		public static void AssertAreEqual(GlitchSlimulationV01 expected, GlitchSlimulationV01 actual)
		{
			if (TestUtil.AssertNullness(expected, actual))
			{
				TestUtil.AssertAreEqual(expected.teleporters, actual.teleporters, delegate(GlitchTeleportDestinationV01 a, GlitchTeleportDestinationV01 b)
				{
					GlitchTeleportDestinationV01.AssertAreEqual(a, b);
				}, "teleporters");
				TestUtil.AssertAreEqual(expected.nodes, actual.nodes, delegate(GlitchTarrNodeV01 a, GlitchTarrNodeV01 b)
				{
					GlitchTarrNodeV01.AssertAreEqual(a, b);
				}, "nodes");
				TestUtil.AssertAreEqual(expected.slimes, actual.slimes, delegate(GlitchSlimeDataV01 a, GlitchSlimeDataV01 b)
				{
					GlitchSlimeDataV01.AssertAreEqual(a, b);
				}, "slimes");
				TestUtil.AssertAreEqual(expected.impostoDirectors, actual.impostoDirectors, delegate(GlitchImpostoDirectorV01 a, GlitchImpostoDirectorV01 b)
				{
					GlitchImpostoDirectorV01.AssertAreEqual(a, b);
				}, "impostoDirectors");
				TestUtil.AssertAreEqual(expected.impostos, actual.impostos, delegate(GlitchImpostoV01 a, GlitchImpostoV01 b)
				{
					GlitchImpostoV01.AssertAreEqual(a, b);
				}, "impostos");
			}
		}
	}
}
