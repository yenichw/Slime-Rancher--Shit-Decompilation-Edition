using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class FirestormV01 : PersistedDataSet
	{
		public double endStormTime;

		public bool stormPreparing;

		public double nextStormTime;

		public override string Identifier => "SRF";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			endStormTime = reader.ReadDouble();
			stormPreparing = reader.ReadBoolean();
			nextStormTime = reader.ReadDouble();
		}

		public static FirestormV01 Load(BinaryReader reader)
		{
			FirestormV01 firestormV = new FirestormV01();
			firestormV.Load(reader.BaseStream);
			return firestormV;
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(endStormTime);
			writer.Write(stormPreparing);
			writer.Write(nextStormTime);
		}

		public static void AssertAreEqual(FirestormV01 expected, FirestormV01 actual)
		{
		}
	}
}
