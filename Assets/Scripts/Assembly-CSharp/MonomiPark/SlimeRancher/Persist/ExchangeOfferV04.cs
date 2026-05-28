using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ExchangeOfferV04 : VersionedPersistedDataSet<ExchangeOfferV03>
	{
		public bool hasOffer;

		public List<RequestedItemEntryV03> requests;

		public List<ItemEntryV03> rewards;

		public double expireTime;

		public double earlyExchangeTime;

		public string rancherId;

		public string offerId;

		public override string Identifier => "SREO";

		public override uint Version => 4u;

		public ExchangeOfferV04()
		{
		}

		public ExchangeOfferV04(ExchangeOfferV03 legacyData)
		{
			UpgradeFrom(legacyData);
		}

		public static ExchangeOfferV04 Load(BinaryReader reader)
		{
			ExchangeOfferV04 exchangeOfferV = new ExchangeOfferV04();
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
				earlyExchangeTime = reader.ReadDouble();
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
			writer.Write(earlyExchangeTime);
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

		protected override void UpgradeFrom(ExchangeOfferV03 legacyData)
		{
			hasOffer = legacyData.hasOffer;
			requests = legacyData.requests;
			rewards = legacyData.rewards;
			expireTime = legacyData.expireTime;
			rancherId = legacyData.rancherId;
			offerId = legacyData.offerId;
			earlyExchangeTime = double.NegativeInfinity;
		}

		public static void AssertAreEqual(ExchangeOfferV04 expected, ExchangeOfferV04 actual)
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

		public static void AssertAreEqual(ExchangeOfferV03 expected, ExchangeOfferV04 actual)
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
	}
}
