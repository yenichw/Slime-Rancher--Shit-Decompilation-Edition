using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchTeleportDestinationV01 : PersistedDataSet
	{
		public double? activationTime;

		public override string Identifier => "SRGLITCH_TPD";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			LoadNullable(reader, out activationTime);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			WriteNullable(writer, activationTime);
		}

		public static void AssertAreEqual(GlitchTeleportDestinationV01 expected, GlitchTeleportDestinationV01 actual)
		{
		}
	}
}
