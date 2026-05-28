using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ResourceCycleDataV02 : PersistedDataSet
	{
		public ResourceCycle.State state;

		public float progressTime;

		public override string Identifier => "SRRCD";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			state = (ResourceCycle.State)reader.ReadInt32();
			progressTime = reader.ReadSingle();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)state);
			writer.Write(progressTime);
		}

		public static void AssertAreEqual(ResourceCycleDataV02 expected, ResourceCycleDataV02 actual)
		{
		}
	}
}
