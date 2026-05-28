using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ResourceWaterV02 : PersistedDataSet
	{
		public float spawn;

		public float water;

		public override string Identifier => "SRRW";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			spawn = reader.ReadSingle();
			water = reader.ReadSingle();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(spawn);
			writer.Write(water);
		}

		public static ResourceWaterV02 Load(BinaryReader reader)
		{
			ResourceWaterV02 resourceWaterV = new ResourceWaterV02();
			resourceWaterV.Load(reader.BaseStream);
			return resourceWaterV;
		}

		public static void AssertAreEqual(ResourceWaterV02 expected, ResourceWaterV02 actual)
		{
		}
	}
}
