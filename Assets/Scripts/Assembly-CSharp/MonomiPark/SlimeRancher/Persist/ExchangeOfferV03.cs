using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ExchangeOfferV03 : VersionedPersistedDataSet<ExchangeOfferV02>
	{
		public bool hasOffer;

		public List<RequestedItemEntryV03> requests;

		public List<ItemEntryV03> rewards;

		public double expireTime;

		public string rancherId;

		public string offerId;

		public override string Identifier => "SREO";

		public override uint Version => 3u;

		public ExchangeOfferV03()
		{
		}

		public ExchangeOfferV03(ExchangeOfferV02 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		public static ExchangeOfferV03 Load(BinaryReader reader)
		{
			ExchangeOfferV03 exchangeOfferV = new ExchangeOfferV03();
			exchangeOfferV.Load(reader.BaseStream);
			return exchangeOfferV;
		}

		protected override void LoadData(BinaryReader reader)
		{
			hasOffer = reader.ReadBoolean();
			if (hasOffer)
			{
				rancherId = reader.ReadString();
				offerId = reader.ReadString();
				expireTime = reader.ReadDouble();
				int num = reader.ReadInt32();
				requests = new List<RequestedItemEntryV03>();
				while (num > 0)
				{
					RequestedItemEntryV03 requestedItemEntryV = new RequestedItemEntryV03();
					requestedItemEntryV.Load(reader.BaseStream);
					requests.Add(requestedItemEntryV);
					num--;
				}
				int num2 = reader.ReadInt32();
				rewards = new List<ItemEntryV03>();
				while (num2 > 0)
				{
					ItemEntryV03 itemEntryV = new ItemEntryV03();
					itemEntryV.Load(reader.BaseStream);
					rewards.Add(itemEntryV);
					num2--;
				}
			}
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(hasOffer);
			if (!hasOffer)
			{
				return;
			}
			writer.Write(rancherId);
			writer.Write(offerId);
			writer.Write(expireTime);
			writer.Write(requests.Count);
			foreach (RequestedItemEntryV03 request in requests)
			{
				request.Write(writer.BaseStream);
			}
			writer.Write(rewards.Count);
			foreach (ItemEntryV03 reward in rewards)
			{
				reward.Write(writer.BaseStream);
			}
		}

		protected override void UpgradeFrom(ExchangeOfferV02 legacyData)
		{
			hasOffer = legacyData.hasOffer;
			if (hasOffer)
			{
				requests = UpgradeFrom(legacyData.requests);
				rewards = UpgradeFrom(legacyData.rewards);
				expireTime = legacyData.expireTime;
				rancherId = legacyData.rancherId;
				offerId = legacyData.offerId;
			}
		}

		public static void AssertAreEqual(ExchangeOfferV03 expected, ExchangeOfferV03 actual)
		{
			for (int i = 0; i < expected.requests.Count; i++)
			{
				RequestedItemEntryV03.AssertAreEqual(expected.requests[i], actual.requests[i]);
			}
			for (int j = 0; j < expected.rewards.Count; j++)
			{
				ItemEntryV03.AssertAreEqual(expected.rewards[j], actual.rewards[j]);
			}
		}

		public static void AssertAreEqual(ExchangeOfferV02 expected, ExchangeOfferV03 actual)
		{
			if (expected.requests != null)
			{
				for (int i = 0; i < expected.requests.Count; i++)
				{
					RequestedItemEntryV03.AssertAreEqual(expected.requests[i], actual.requests[i]);
				}
			}
			if (expected.rewards != null)
			{
				for (int j = 0; j < expected.rewards.Count; j++)
				{
					ItemEntryV03.AssertAreEqual(expected.rewards[j], actual.rewards[j]);
				}
			}
		}

		private List<ItemEntryV03> UpgradeFrom(List<ItemEntryV02> legacyData)
		{
			List<ItemEntryV03> list = new List<ItemEntryV03>();
			foreach (ItemEntryV02 legacyDatum in legacyData)
			{
				list.Add(new ItemEntryV03(legacyDatum));
			}
			return list;
		}

		private List<RequestedItemEntryV03> UpgradeFrom(List<RequestedItemEntryV02> legacyData)
		{
			List<RequestedItemEntryV03> list = new List<RequestedItemEntryV03>();
			foreach (RequestedItemEntryV02 legacyDatum in legacyData)
			{
				list.Add(new RequestedItemEntryV03(legacyDatum));
			}
			return list;
		}
	}
}
