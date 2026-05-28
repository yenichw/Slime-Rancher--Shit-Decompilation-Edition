using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GadgetPortableSlimeBaitV01 : PersistedDataSet
	{
		public Identifiable.Id id;

		public override string Identifier => "SRPSB";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			id = (Identifiable.Id)reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)id);
		}

		public static void AssertAreEqual(GadgetPortableSlimeBaitV01 expected, GadgetPortableSlimeBaitV01 actual)
		{
			TestUtil.AssertNullness(expected, actual);
		}
	}
}
