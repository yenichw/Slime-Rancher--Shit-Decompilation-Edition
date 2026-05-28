using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class RequestedItemEntryV03 : VersionedPersistedDataSet<RequestedItemEntryV02>
	{
		public Identifiable.Id id;

		public ExchangeDirector.NonIdentReward nonIdentReward;

		public int count;

		public int progress;

		public override string Identifier => "SRRIE";

		public override uint Version => 3u;

		public RequestedItemEntryV03()
		{
		}

		public RequestedItemEntryV03(RequestedItemEntryV02 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		protected override void LoadData(BinaryReader reader)
		{
			id = (Identifiable.Id)reader.ReadInt32();
			nonIdentReward = (ExchangeDirector.NonIdentReward)reader.ReadInt32();
			count = reader.ReadInt32();
			progress = reader.ReadInt32();
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write((int)id);
			writer.Write((int)nonIdentReward);
			writer.Write(count);
			writer.Write(progress);
		}

		public static void AssertAreEqual(RequestedItemEntryV03 expected, RequestedItemEntryV03 actual)
		{
		}

		public static void AssertAreEqual(RequestedItemEntryV02 expected, RequestedItemEntryV03 actual)
		{
		}

		protected override void UpgradeFrom(RequestedItemEntryV02 legacyData)
		{
			id = legacyData.id;
			count = legacyData.count;
			progress = legacyData.progress;
			nonIdentReward = ExchangeDirector.NonIdentReward.NONE;
		}
	}
}
