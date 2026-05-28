using System.Linq;
using DLCPackage;
using UnityEngine;

[CreateAssetMenu(menuName = "Message Of The Day/Local Provider")]
public class MessageOfTheDayLocalProvider : MessageOfTheDayProvider
{
	public MessageOfTheDayCollection messageCollection;

	private DLCDirector dlcDirector;

	public void SetDLCDirector(DLCDirector director)
	{
		dlcDirector = director;
	}

	protected override void RetrieveMessage(SuccessHandler onSuccess, ErrorHandler onError)
	{
		if (messageCollection != null && messageCollection.messages.Count > 0)
		{
			MessageOfTheDay randomMessage = ((dlcDirector == null) ? (randomMessage = messageCollection.GetRandomMessage()) : (randomMessage = messageCollection.GetRandomMessage(CanShowMessage)));
			if (randomMessage != null)
			{
				onSuccess(randomMessage);
			}
			else
			{
				onError();
			}
		}
		else
		{
			onError();
		}
	}

	private bool CanShowMessage(BundledMessageOfTheDay msg)
	{
		if (msg.showForAvailableDLCPackages.Count == 0)
		{
			return true;
		}
		return msg.showForAvailableDLCPackages.Any((Id packageId) => !SRSingleton<GameContext>.Instance.AutoSaveDirector.ProfileManager.Profile.DLC.installed.Contains(packageId));
	}
}
