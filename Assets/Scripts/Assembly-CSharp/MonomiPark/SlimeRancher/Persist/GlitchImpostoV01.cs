using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchImpostoV01 : PersistedDataSet
	{
		public double? deactivateTime;

		public double cooldownTime;

		public override string Identifier => "SRGLITCH_IP";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			LoadNullable(reader, out deactivateTime);
			cooldownTime = reader.ReadDouble();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteNullable(writer, deactivateTime);
			writer.Write(cooldownTime);
		}

		public static void AssertAreEqual(GlitchImpostoV01 expected, GlitchImpostoV01 actual)
		{
		}
	}
}
