using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GordoV01 : PersistedDataSet
	{
		public int eatenCount;

		public List<Identifiable.Id> fashions = new List<Identifiable.Id>();

		public override string Identifier => "SRG";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			eatenCount = reader.ReadInt32();
			fashions = PersistedDataSet.LoadList(reader, (int v) => (Identifiable.Id)v);
		}

		public static GordoV01 Load(BinaryReader reader)
		{
			GordoV01 gordoV = new GordoV01();
			gordoV.Load(reader.BaseStream);
			return gordoV;
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(eatenCount);
			PersistedDataSet.WriteList(writer, fashions, (Identifiable.Id v) => (int)v);
		}

		public static void AssertAreEqual(GordoV01 expected, GordoV01 actual)
		{
			TestUtil.AssertAreEqual(expected.fashions, actual.fashions, "fashions");
		}
	}
}
