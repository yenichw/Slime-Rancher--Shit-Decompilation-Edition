using System.Collections.Generic;
using UnityEngine;

public class OfferGenerator
{
	private class ItemGenerator
	{
		public ICollection<Identifiable.Id> ids;

		public ItemGenerator(ICollection<Identifiable.Id> ids)
		{
			this.ids = ids;
		}

		public ExchangeDirector.ItemEntry Generate(ExchangeDirector exchangeDir, List<Identifiable.Id> disallowed, List<Identifiable.Id> whitelist, int value, bool isRequest)
		{
			List<Identifiable.Id> list = new List<Identifiable.Id>(ids);
			list.RemoveAll((Identifiable.Id id) => disallowed.Contains(id) || !whitelist.Contains(id));
			if (list.Count <= 0)
			{
				return null;
			}
			Identifiable.Id id2 = Randoms.SHARED.Pick(list, Identifiable.Id.NONE);
			int countForValue = exchangeDir.GetCountForValue(id2, value);
			if (countForValue == 0)
			{
				return null;
			}
			if (isRequest)
			{
				return new ExchangeDirector.RequestedItemEntry(id2, countForValue, 0);
			}
			return new ExchangeDirector.ItemEntry(id2, countForValue);
		}

		public bool ContainsAny(List<Identifiable.Id> whitelist)
		{
			foreach (Identifiable.Id item in whitelist)
			{
				if (ids.Contains(item))
				{
					return true;
				}
			}
			return false;
		}
	}

	public string rancherId;

	private int numBlurbs;

	private ItemGenerator requests;

	private ItemGenerator rewards;

	private ItemGenerator rareRewards;

	private const int MIN_OFFER_VAL = 50;

	private const int MAX_OFFER_VAL = 100;

	private const float BONUS_PROB = 0.125f;

	private const float RARE_PROB = 0.5f;

	private const float REWARD_VAL_MULT = 1.75f;

	private const float REWARD_BONUS_MULT = 2.5f;

	private const ExchangeDirector.NonIdentReward BONUS_CASH_REWARD = ExchangeDirector.NonIdentReward.NEWBUCKS_HUGE;

	private readonly Dictionary<ExchangeDirector.NonIdentReward, float> NORM_CASH_REWARDS = new Dictionary<ExchangeDirector.NonIdentReward, float>
	{
		{
			ExchangeDirector.NonIdentReward.NEWBUCKS_SMALL,
			3f
		},
		{
			ExchangeDirector.NonIdentReward.NEWBUCKS_MEDIUM,
			2f
		},
		{
			ExchangeDirector.NonIdentReward.NEWBUCKS_LARGE,
			1f
		}
	};

	public OfferGenerator(string rancherId, int numBlurbs, Identifiable.Id[] requests, Identifiable.Id[] rewards, Identifiable.Id[] rareRewards)
	{
		this.rancherId = rancherId;
		this.numBlurbs = numBlurbs;
		this.requests = new ItemGenerator(requests);
		this.rewards = new ItemGenerator(rewards);
		this.rareRewards = new ItemGenerator(rareRewards);
	}

	public ExchangeDirector.Offer Generate(ExchangeDirector exchangeDir, List<Identifiable.Id> whitelist, double expireTime, double earlyExchangeTime, int retries, bool isFirstOffer, bool isGoldPlortOffer)
	{
		for (int i = 0; i < retries; i++)
		{
			ExchangeDirector.Offer offer = GenerateOneOffer(exchangeDir, whitelist, expireTime, earlyExchangeTime, isFirstOffer, isGoldPlortOffer);
			if (offer != null)
			{
				return offer;
			}
		}
		return null;
	}

	public List<ExchangeDirector.RequestedItemEntry> GenerateRequestList(ExchangeDirector exchangeDir, List<Identifiable.Id> whitelist)
	{
		return GenerateRequestList(exchangeDir, whitelist, Randoms.SHARED.GetInRange(50, 100), new List<Identifiable.Id>());
	}

	public int GetRandomBlurb()
	{
		return Randoms.SHARED.GetInRange(1, numBlurbs + 1);
	}

	private List<ExchangeDirector.RequestedItemEntry> GenerateRequestList(ExchangeDirector exchangeDir, List<Identifiable.Id> whitelist, int requestValue, List<Identifiable.Id> used)
	{
		List<ExchangeDirector.RequestedItemEntry> list = new List<ExchangeDirector.RequestedItemEntry>();
		int inRange = Randoms.SHARED.GetInRange(2, 4);
		float[] array = new float[inRange];
		float num = 0f;
		for (int i = 0; i < inRange; i++)
		{
			array[i] = Randoms.SHARED.GetInRange(0.5f, 1.5f);
			num += array[i];
		}
		for (int j = 0; j < inRange; j++)
		{
			int value = Mathf.RoundToInt((float)requestValue * array[j] / num);
			if (requests.Generate(exchangeDir, used, whitelist, value, isRequest: true) is ExchangeDirector.RequestedItemEntry requestedItemEntry)
			{
				list.Add(requestedItemEntry);
				used.Add(requestedItemEntry.id);
				continue;
			}
			return null;
		}
		return list;
	}

	private ExchangeDirector.Offer GenerateOneOffer(ExchangeDirector exchangeDir, List<Identifiable.Id> whitelist, double expireTime, double earlyExchangeTime, bool isFirstOffer, bool isGoldPlortOffer)
	{
		List<Identifiable.Id> list = new List<Identifiable.Id>();
		bool flag = !isFirstOffer && Randoms.SHARED.GetProbability(0.125f);
		bool flag2 = flag && rareRewards.ContainsAny(whitelist) && Randoms.SHARED.GetProbability(0.5f);
		int inRange = Randoms.SHARED.GetInRange(50, 100);
		int num = Mathf.RoundToInt((float)inRange * (flag ? 2.5f : 1.75f));
		List<ExchangeDirector.RequestedItemEntry> list2 = GenerateRequestList(exchangeDir, whitelist, inRange, list);
		if (list2 == null)
		{
			return null;
		}
		List<ExchangeDirector.ItemEntry> list3 = new List<ExchangeDirector.ItemEntry>();
		if (isGoldPlortOffer)
		{
			list3.Add(new ExchangeDirector.ItemEntry(Identifiable.Id.GOLD_PLORT, 3));
			list.Add(Identifiable.Id.GOLD_PLORT);
		}
		else
		{
			int num2 = (flag2 ? 1 : Randoms.SHARED.GetInRange(2, 4));
			float[] array = new float[num2];
			float num3 = 0f;
			for (int i = 0; i < num2; i++)
			{
				array[i] = Randoms.SHARED.GetInRange(0.5f, 1.5f);
				num3 += array[i];
			}
			for (int j = 0; j < num2; j++)
			{
				int value = (flag2 ? 100 : Mathf.RoundToInt((float)num * array[j] / num3));
				ExchangeDirector.ItemEntry itemEntry = (flag2 ? rareRewards : rewards).Generate(exchangeDir, list, whitelist, value, isRequest: false);
				if (itemEntry != null)
				{
					list3.Add(itemEntry);
					list.Add(itemEntry.id);
					continue;
				}
				return null;
			}
			ExchangeDirector.ItemEntry item = new ExchangeDirector.ItemEntry(flag ? ExchangeDirector.NonIdentReward.NEWBUCKS_HUGE : Randoms.SHARED.Pick(NORM_CASH_REWARDS, ExchangeDirector.NonIdentReward.NEWBUCKS_SMALL));
			list3.Add(item);
		}
		int num4 = (isFirstOffer ? 1 : GetRandomBlurb());
		return new ExchangeDirector.Offer((flag || isGoldPlortOffer) ? ("m.bonusoffer." + rancherId) : ("m.offer_" + num4 + "." + rancherId), rancherId, expireTime, earlyExchangeTime, list2, list3);
	}
}
