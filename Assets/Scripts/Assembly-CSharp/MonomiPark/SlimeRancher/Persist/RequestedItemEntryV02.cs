using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class RequestedItemEntryV02 : PersistedDataSet
	{
		public Identifiable.Id id;

		public int count;

		public int progress;

		public override string Identifier => "SRRIE";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			id = (Identifiable.Id)reader.ReadInt32();
			count = reader.ReadInt32();
			progress = reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)id);
			writer.Write(count);
			writer.Write(progress);
		}

		public static void AssertAreEqual(RequestedItemEntryV02 expected, RequestedItemEntryV02 actual)
		{
		}
	}
}
