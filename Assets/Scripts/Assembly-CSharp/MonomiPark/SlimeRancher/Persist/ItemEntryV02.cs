using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ItemEntryV02 : PersistedDataSet
	{
		public Identifiable.Id id;

		public int count;

		public override string Identifier => "SRIE";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			id = (Identifiable.Id)reader.ReadInt32();
			count = reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)id);
			writer.Write(count);
		}

		public static void AssertAreEqual(ItemEntryV02 expected, ItemEntryV02 actual)
		{
		}
	}
}
