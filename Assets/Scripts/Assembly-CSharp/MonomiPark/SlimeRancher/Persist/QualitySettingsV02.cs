using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class QualitySettingsV02 : PersistedDataSet
	{
		public int lighting;

		public int textures;

		public int antialiasing;

		public int shadows;

		public int particles;

		public int modelDetail;

		public int waterDetail;

		public bool ambientOcclusion;

		public bool bloom;

		public override string Identifier => "SRQUALITY";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			lighting = reader.ReadInt32();
			textures = reader.ReadInt32();
			antialiasing = reader.ReadInt32();
			shadows = reader.ReadInt32();
			particles = reader.ReadInt32();
			modelDetail = reader.ReadInt32();
			waterDetail = reader.ReadInt32();
			ambientOcclusion = reader.ReadBoolean();
			bloom = reader.ReadBoolean();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(lighting);
			writer.Write(textures);
			writer.Write(antialiasing);
			writer.Write(shadows);
			writer.Write(particles);
			writer.Write(modelDetail);
			writer.Write(waterDetail);
			writer.Write(ambientOcclusion);
			writer.Write(bloom);
		}
	}
}
