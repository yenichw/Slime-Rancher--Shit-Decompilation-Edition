using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Services/MessageOfTheDayCollection")]
public class MessageOfTheDayCollection : ScriptableObject
{
	public List<BundledMessageOfTheDay> messages;

	public BundledMessageOfTheDay GetRandomMessage()
	{
		return Randoms.SHARED.Pick(messages, null);
	}

	public BundledMessageOfTheDay GetRandomMessage(Predicate<BundledMessageOfTheDay> messageFilter)
	{
		return Randoms.SHARED.Pick(messages.Where((BundledMessageOfTheDay msg) => messageFilter(msg)), null);
	}
}
