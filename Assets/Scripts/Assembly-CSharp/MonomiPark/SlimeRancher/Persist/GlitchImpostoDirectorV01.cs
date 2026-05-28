using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchImpostoDirectorV01 : PersistedDataSet
	{
		public double? hibernationTime;

		public override string Identifier => "SRGLITCH_ID";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			LoadNullable(reader, out hibernationTime);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteNullable(writer, hibernationTime);
		}

		public static void AssertAreEqual(GlitchImpostoDirectorV01 expected, GlitchImpostoDirectorV01 actual)
		{
		}
	}
}
