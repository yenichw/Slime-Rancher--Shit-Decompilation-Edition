using System.Collections.Generic;
using System.IO;

namespace MonomiPark.SlimeRancher.Persist
{
	public class ExchangeOfferV02 : PersistedDataSet
	{
		public bool hasOffer;

		public List<RequestedItemEntryV02> requests;

		public List<ItemEntryV02> rewards;

		public float expireTime;

		public string rancherId;

		public string offerId;

		public override string Identifier => "SREO";

		public override uint Version => 2u;

		protected override void LoadData(BinaryReader reader)
		{
			hasOffer = reader.ReadBoolean();
			if (hasOffer)
			{
				rancherId = reader.ReadString();
				offerId = reader.ReadString();
				expireTime = reader.ReadSingle();
				int num = reader.ReadInt32();
				requests = new List<RequestedItemEntryV02>();
				while (num > 0)
				{
					RequestedItemEntryV02 requestedItemEntryV = new RequestedItemEntryV02();
					requestedItemEntryV.Load(reader.BaseStream);
					requests.Add(requestedItemEntryV);
					num--;
				}
				int num2 = reader.ReadInt32();
				rewards = new List<ItemEntryV02>();
				while (num2 > 0)
				{
					ItemEntryV02 itemEntryV = new ItemEntryV02();
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
			foreach (RequestedItemEntryV02 request in requests)
			{
				request.Write(writer.BaseStream);
			}
			writer.Write(rewards.Count);
			foreach (ItemEntryV02 reward in rewards)
			{
				reward.Write(writer.BaseStream);
			}
		}

		public static void AssertAreEqual(ExchangeOfferV02 expected, ExchangeOfferV02 actual)
		{
			for (int i = 0; i < expected.requests.Count; i++)
			{
				RequestedItemEntryV02.AssertAreEqual(expected.requests[i], actual.requests[i]);
			}
			for (int j = 0; j < expected.rewards.Count; j++)
			{
				ItemEntryV02.AssertAreEqual(expected.rewards[j], actual.rewards[j]);
			}
		}
	}
}
