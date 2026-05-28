using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ResourceCycleDataV03 : VersionedPersistedDataSet<ResourceCycleDataV02>
	{
		public ResourceCycle.State state;

		public double progressTime;

		public override string Identifier => "SRRCD";

		public override uint Version => 3u;

		public ResourceCycleDataV03()
		{
		}

		public ResourceCycleDataV03(ResourceCycleDataV02 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			state = (ResourceCycle.State)reader.ReadInt32();
			progressTime = reader.ReadDouble();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)state);
			writer.Write(progressTime);
		}

		protected override void UpgradeFrom(ResourceCycleDataV02 legacyData)
		{
			state = legacyData.state;
			progressTime = legacyData.progressTime;
		}

		public static void AssertAreEqual(ResourceCycleDataV03 expected, ResourceCycleDataV03 actual)
		{
		}

		public static void AssertAreEqual(ResourceCycleDataV02 expected, ResourceCycleDataV03 actual)
		{
		}
	}
}
