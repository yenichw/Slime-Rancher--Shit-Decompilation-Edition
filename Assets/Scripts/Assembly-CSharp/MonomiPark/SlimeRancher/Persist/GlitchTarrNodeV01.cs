using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchTarrNodeV01 : PersistedDataSet
	{
		public double activationTime;

		public override string Identifier => "SRGLITCH_TS";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			activationTime = reader.ReadDouble();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(activationTime);
		}

		public static void AssertAreEqual(GlitchTarrNodeV01 expected, GlitchTarrNodeV01 actual)
		{
		}
	}
}
