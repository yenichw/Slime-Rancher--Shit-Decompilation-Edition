using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class AmmoDataV02 : PersistedDataSet
	{
		public Identifiable.Id id;

		public SlimeEmotionDataV02 emotionData;

		public int count;

		public override string Identifier => "SRAD";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			id = (Identifiable.Id)reader.ReadInt32();
			count = reader.ReadInt32();
			emotionData = new SlimeEmotionDataV02();
			emotionData.Load(reader.BaseStream);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)id);
			writer.Write(count);
			emotionData.Write(writer.BaseStream);
		}

		public static void AssertAreEqual(AmmoDataV02 expected, AmmoDataV02 actual)
		{
			SlimeEmotionDataV02.AssertAreEqual(expected.emotionData, actual.emotionData);
		}

		public static void AssertAreEqual(List<AmmoDataV02> expected, List<AmmoDataV02> actual)
		{
			TestUtil.AssertAreEqual(expected, actual, delegate(AmmoDataV02 a, AmmoDataV02 b, string m)
			{
				AssertAreEqual(a, b);
			});
		}
	}
}
