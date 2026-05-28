using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class PediaV02 : PersistedDataSet
	{
		public List<string> unlockedIds = new List<string>();

		public List<string> completedTuts = new List<string>();

		public int progressGivenForPediaCount;

		public override string Identifier => "SRPED";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			progressGivenForPediaCount = reader.ReadInt32();
			unlockedIds = PersistedDataSet.LoadList(reader, (string val) => val);
			completedTuts = PersistedDataSet.LoadList(reader, (string val) => val);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(progressGivenForPediaCount);
			PersistedDataSet.WriteList(writer, unlockedIds, (string val) => val);
			PersistedDataSet.WriteList(writer, completedTuts, (string val) => val);
		}

		public static PediaV02 Load(BinaryReader reader)
		{
			PediaV02 pediaV = new PediaV02();
			pediaV.Load(reader.BaseStream);
			return pediaV;
		}

		public static void AssertAreEqual(PediaV02 expected, PediaV02 actual)
		{
			TestUtil.AssertAreEqual(expected.unlockedIds, actual.unlockedIds, "unlockedIds");
			TestUtil.AssertAreEqual(expected.completedTuts, actual.completedTuts, "completedTuts");
		}
	}
}
