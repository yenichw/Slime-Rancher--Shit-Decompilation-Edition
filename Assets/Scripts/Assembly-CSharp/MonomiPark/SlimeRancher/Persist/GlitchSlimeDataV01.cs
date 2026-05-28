using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class GlitchSlimeDataV01 : PersistedDataSet
	{
		public float exposureChance;

		public double deathTime;

		public override string Identifier => "SRAD_GS";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			exposureChance = reader.ReadSingle();
			deathTime = reader.ReadDouble();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(exposureChance);
			writer.Write(deathTime);
		}

		public static void AssertAreEqual(GlitchSlimeDataV01 expected, GlitchSlimeDataV01 actual)
		{
		}
	}
}
