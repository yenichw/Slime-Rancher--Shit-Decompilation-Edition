using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class QuicksilverEnergyGeneratorV01 : PersistedDataSet
	{
		public QuicksilverEnergyGenerator.State state;

		public float? timer;

		public override string Identifier => "SRQSEG";

		public override uint Version => 1u;

		public static QuicksilverEnergyGeneratorV01 Load(BinaryReader reader)
		{
			QuicksilverEnergyGeneratorV01 quicksilverEnergyGeneratorV = new QuicksilverEnergyGeneratorV01();
			quicksilverEnergyGeneratorV.Load(reader.BaseStream);
			return quicksilverEnergyGeneratorV;
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)state);
			WriteNullable(writer, timer);
		}

		protected override void LoadData(BinaryReader reader)
		{
			state = (QuicksilverEnergyGenerator.State)reader.ReadInt32();
			LoadNullable(reader, out timer);
		}

		public static void AssertAreEqual(QuicksilverEnergyGeneratorV01 expected, QuicksilverEnergyGeneratorV01 actual)
		{
		}
	}
}
