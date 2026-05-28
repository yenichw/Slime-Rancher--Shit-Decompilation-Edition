using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class DecorizerSettingsV01 : PersistedDataSet
	{
		public Identifiable.Id selected;

		public override string Identifier => "SRDZRSETTINGS";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			selected = (Identifiable.Id)reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)selected);
		}

		public static void AssertAreEqual(DecorizerSettingsV01 expected, DecorizerSettingsV01 actual)
		{
		}
	}
}
