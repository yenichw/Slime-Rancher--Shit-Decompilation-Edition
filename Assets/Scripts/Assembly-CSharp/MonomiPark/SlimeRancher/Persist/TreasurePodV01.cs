using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class TreasurePodV01 : PersistedDataSet
	{
		public TreasurePod.State state;

		public List<Identifiable.Id> spawnQueue = new List<Identifiable.Id>();

		public override string Identifier => "SRTP";

		public override uint Version => 1u;

		public static TreasurePodV01 Load(BinaryReader reader)
		{
			TreasurePodV01 treasurePodV = new TreasurePodV01();
			treasurePodV.Load(reader.BaseStream);
			return treasurePodV;
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)state);
			PersistedDataSet.WriteList(writer, spawnQueue, (Identifiable.Id id) => (int)id);
		}

		protected override void LoadData(BinaryReader reader)
		{
			state = (TreasurePod.State)reader.ReadInt32();
			spawnQueue = PersistedDataSet.LoadList(reader, (int id) => (Identifiable.Id)id);
		}

		public static void AssertAreEqual(TreasurePodV01 expected, TreasurePodV01 actual)
		{
			TestUtil.AssertAreEqual(expected.spawnQueue, actual.spawnQueue, "spawnQueue");
		}
	}
}
