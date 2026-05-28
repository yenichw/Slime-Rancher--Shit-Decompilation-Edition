using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class SettingsV01 : PersistedDataSet
	{
		public OptionsV12 options = new OptionsV12();

		public override string Identifier => "SRSETTINGS";

		public override uint Version => 1u;

		protected override void LoadData(BinaryReader reader)
		{
			options = new OptionsV12();
			options.Load(reader.BaseStream);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			options.Write(writer.BaseStream);
		}

		public void SetLegacyProfileOptions(ProfileV04 legacyProfile)
		{
			OptionsV09 optionsV = legacyProfile.options;
			options = new OptionsV12();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				optionsV.Write(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				options.Load(memoryStream);
			}
		}
	}
}
