using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DLCV01 : PersistedDataSet
	{
		public enum Enum
		{

		}

		public List<Enum> activated = new List<Enum>();

		public override string Identifier => "SRDLC";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			activated = PersistedDataSet.LoadList(reader, (int id) => (Enum)id);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			PersistedDataSet.WriteList(writer, activated, (Enum id) => (int)id);
		}

		public static DLCV01 Load(BinaryReader reader)
		{
			DLCV01 dLCV = new DLCV01();
			dLCV.Load(reader.BaseStream);
			return dLCV;
		}

		public static void AssertAreEqual(DLCV01 expected, DLCV01 actual)
		{
			TestUtil.AssertAreEqual(expected.activated, actual.activated, "activated");
		}
	}
}
