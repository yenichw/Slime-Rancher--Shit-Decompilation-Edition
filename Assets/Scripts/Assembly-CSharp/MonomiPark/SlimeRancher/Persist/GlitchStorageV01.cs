using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchStorageV01 : PersistedDataSet
	{
		public Identifiable.Id id;

		public int count;

		public override string Identifier => "SRGLITCH_ST";

		public override uint Version => 1u;

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

		public static void AssertAreEqual(GlitchStorageV01 expected, GlitchStorageV01 actual)
		{
		}
	}
}
