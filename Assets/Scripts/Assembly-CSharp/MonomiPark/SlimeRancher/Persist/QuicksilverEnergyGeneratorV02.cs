using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class QuicksilverEnergyGeneratorV02 : VersionedPersistedDataSet<QuicksilverEnergyGeneratorV01>
	{
		public QuicksilverEnergyGenerator.State state;

		public double? timer;

		public override string Identifier => "SRQSEG";

		public override uint Version => 2u;

		public QuicksilverEnergyGeneratorV02()
		{
		}

		public QuicksilverEnergyGeneratorV02(QuicksilverEnergyGeneratorV01 legacy)
		{
			UpgradeFrom(legacy);
		}

		public static QuicksilverEnergyGeneratorV02 Load(BinaryReader reader)
		{
			QuicksilverEnergyGeneratorV02 quicksilverEnergyGeneratorV = new QuicksilverEnergyGeneratorV02();
			quicksilverEnergyGeneratorV.Load(reader.BaseStream);
			return quicksilverEnergyGeneratorV;
		}

		protected override void LoadData(BinaryReader reader)
		{
			state = (QuicksilverEnergyGenerator.State)reader.ReadInt32();
			LoadNullable(reader, out timer);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)state);
			WriteNullable(writer, timer);
		}

		protected override void UpgradeFrom(QuicksilverEnergyGeneratorV01 legacy)
		{
			state = legacy.state;
			timer = legacy.timer;
		}

		public static void AssertAreEqual(QuicksilverEnergyGeneratorV02 expected, QuicksilverEnergyGeneratorV02 actual)
		{
		}

		public static void AssertAreEqual(QuicksilverEnergyGeneratorV01 expected, QuicksilverEnergyGeneratorV02 actual)
		{
		}
	}
}
