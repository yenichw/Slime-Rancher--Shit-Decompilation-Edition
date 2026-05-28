using System;
using UnityEngine.UI;

public class AboutUI : BaseUI
{
	public Button creditsBtn;

	public Button dataPrivacyBtn;

	public void OnEnable()
	{
		bool active = SRSingleton<SceneContext>.Instance.AchievementsDirector.HasAchievement(AchievementsDirector.Achievement.FINISH_ADVENTURE);
		creditsBtn.gameObject.SetActive(active);
		dataPrivacyBtn.gameObject.SetActive(value: true);
		if (creditsBtn.gameObject.activeSelf)
		{
			creditsBtn.gameObject.AddComponent<InitSelected>();
		}
		else if (dataPrivacyBtn.gameObject.activeSelf)
		{
			dataPrivacyBtn.gameObject.AddComponent<InitSelected>();
		}
	}

	public void ShowCredits()
	{
		creditsBtn.interactable = false;
		CreditsUI component = SRSingleton<GameContext>.Instance.UITemplates.CreateCreditsPrefab(aboutCredits: true).GetComponent<CreditsUI>();
		component.OnCreditsEnded = (CreditsUI.OnCreditsEndedEvent)Delegate.Combine(component.OnCreditsEnded, new CreditsUI.OnCreditsEndedEvent(OnCreditsEnded));
	}

	private void OnCreditsEnded()
	{
		creditsBtn.interactable = true;
	}
}
