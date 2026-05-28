using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorporatePartnerUI : BaseUI
{
	[Serializable]
	public class RankEntry
	{
		public int cost;

		public Sprite[] rewardIcons;

		public Sprite rewardBanner;
	}

	public RankEntry[] ranks;

	public Sprite titleIcon;

	public int numLevels = 10;

	public TMP_Text costText;

	public Button learnMoreBtn;

	public Button purchaseBtn;

	public GameObject currRankPanel;

	public GameObject noCurrRankPlaceholder;

	public GameObject nextRankPanel;

	public GameObject nextRankRibbon;

	public GameObject noNextRankPlaceholder;

	public GameObject[] rewardObjects;

	public Image[] rewardIcons;

	public TMP_Text[] rewardTitles;

	public Image currRankRibbonImage;

	public TMP_Text currRankNumberText;

	public TMP_Text currRankTitleText;

	public Image nextRankRibbonImage;

	public TMP_Text nextRankNumberText;

	public TMP_Text nextRankTitleText;

	public TMP_Text currencyText;

	public GameObject purchaseFX;

	public SECTR_AudioCue rewardsPurchaseCue;

	public SECTR_AudioCue openCue;

	public SECTR_AudioCue closeCue;

	private bool waitForPedia;

	private const string ALREADY_HAS_LEVEL = "e.already_has_corp_level";

	private const string INELIGIBLE_FOR_LEVEL = "e.ineligible_for_corp_level";

	private const int CHROMA_LEVEL = 5;

	private const int SLIME_TOYS_LEVEL = 8;

	public override void Awake()
	{
		base.Awake();
		RebuildUI();
	}

	public void OnEnable()
	{
		Play(openCue);
	}

	public void OnDisable()
	{
		Play(closeCue);
	}

	public void RebuildUI()
	{
		ProgressDirector progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
		currencyText.text = SRSingleton<SceneContext>.Instance.PlayerState.GetCurrency().ToString();
		int progress = progressDirector.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
		int num = progress + 1;
		bool flag = progress > 0;
		bool flag2 = num <= ranks.Length;
		noCurrRankPlaceholder.SetActive(!flag);
		currRankPanel.SetActive(flag);
		if (flag)
		{
			currRankNumberText.text = progress.ToString();
			currRankTitleText.text = uiBundle.Get("m.partner_rank." + progress);
			currRankRibbonImage.sprite = ranks[progress - 1].rewardBanner;
		}
		noNextRankPlaceholder.SetActive(!flag2);
		nextRankPanel.SetActive(flag2);
		nextRankRibbon.SetActive(flag2);
		if (!flag2)
		{
			return;
		}
		nextRankTitleText.text = uiBundle.Get("m.partner_rank." + num);
		nextRankRibbonImage.sprite = ranks[num - 1].rewardBanner;
		nextRankNumberText.text = num.ToString();
		int num2 = ranks[num - 1].rewardIcons.Length;
		for (int i = 0; i < 3; i++)
		{
			if (i <= num2 - 1)
			{
				EnableReward(num, i);
			}
			else
			{
				DisableReward(i);
			}
		}
		costText.text = ranks[num - 1].cost.ToString();
	}

	private void EnableReward(int rank, int rewardIndex)
	{
		rewardObjects[rewardIndex].SetActive(value: true);
		rewardIcons[rewardIndex].sprite = ranks[rank - 1].rewardIcons[rewardIndex];
		rewardTitles[rewardIndex].text = uiBundle.Get($"m.partner_rank.{rank}.reward.{rewardIndex + 1}");
	}

	private void DisableReward(int rewardIndex)
	{
		rewardObjects[rewardIndex].SetActive(value: false);
	}

	private void BuyLevel(ProgressDirector progressDir, int level, int cost)
	{
		PlayerState playerState = SRSingleton<SceneContext>.Instance.PlayerState;
		int progress = progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
		if (progress >= level)
		{
			PlayErrorCue();
			Error("e.already_has_corp_level");
		}
		else if (progress < level - 1)
		{
			PlayErrorCue();
			Error("e.ineligible_for_corp_level");
		}
		else if (playerState.GetCurrency() >= cost)
		{
			PlayPurchaseCue();
			playerState.SpendCurrency(cost);
			progressDir.AddProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
			RebuildUI();
			PlayPurchaseFX();
			if (level >= 5)
			{
				SRSingleton<SceneContext>.Instance.PediaDirector.UnlockWithoutPopup(PediaDirector.Id.CHROMA);
			}
			if (level >= 8)
			{
				SRSingleton<SceneContext>.Instance.PediaDirector.UnlockWithoutPopup(PediaDirector.Id.SLIME_TOYS);
			}
		}
		else
		{
			PlayErrorCue();
			Error("e.insuf_coins");
		}
	}

	public void Pedia()
	{
		if (!waitForPedia)
		{
			waitForPedia = true;
			PediaUI component = SRSingleton<SceneContext>.Instance.PediaDirector.ShowPedia(PediaDirector.Id.PARTNER).GetComponent<PediaUI>();
			component.onDestroy = (OnDestroyDelegate)Delegate.Combine(component.onDestroy, (OnDestroyDelegate)delegate
			{
				waitForPedia = false;
			});
		}
	}

	public void Purchase()
	{
		if (!waitForPedia)
		{
			ProgressDirector progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
			int num = progressDirector.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER) + 1;
			int cost = ranks[num - 1].cost;
			BuyLevel(progressDirector, num, cost);
			SRSingleton<SceneContext>.Instance.AchievementsDirector.AddToStat(AchievementsDirector.IntStat.REWARD_LEVELS, 1);
			AnalyticsUtil.CustomEvent("ReachedRewardsLevel", new Dictionary<string, object> { { "level", num } });
			SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow();
		}
	}

	public void PlayPurchaseFX()
	{
		if (purchaseFX != null)
		{
			UnityEngine.Object.Instantiate(purchaseFX).transform.SetParent(purchaseBtn.transform, worldPositionStays: false);
		}
	}

	protected void PlayPurchaseCue()
	{
		Play(rewardsPurchaseCue);
	}
}
