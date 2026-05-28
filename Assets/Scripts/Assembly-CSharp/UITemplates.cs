using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UITemplates : SRBehaviour
{
	[Serializable]
	public class IconEntry
	{
		public string keyStr;

		public Sprite icon;
	}

	public GameObject purchaseButtonPrefab;

	public GameObject teleportButtonPrefab;

	public GameObject labelPrefab;

	public GameObject errorDialogPrefab;

	public GameObject confirmDialogPrefab;

	public GameObject purchaseUIPrefab;

	public GameObject decorizerUIPrefab;

	public GameObject availUpgradePrefab;

	public GameObject mailPrefab;

	public GameObject storyCreditsPrefab;

	public GameObject aboutCreditsPrefab;

	public GameObject rancherChatPrefab;

	public GameObject rancherChoicePrefab;

	public SECTR_AudioCue clickCue;

	public SECTR_AudioCue errorCue;

	public SECTR_AudioCue purchaseCue;

	public SECTR_AudioCue purchaseExpansionCue;

	public SECTR_AudioCue purchasePlotCue;

	public SECTR_AudioCue purchaseUpgradeCue;

	public SECTR_AudioCue purchasePersonalUpgradeCue;

	public SECTR_AudioCue purchaseBlueprintCue;

	public SECTR_AudioCue fabricateGadgetCue;

	public SECTR_AudioCue placeGadgetCue;

	public SECTR_AudioCue removeGadgetCue;

	public Sprite currencyIcon;

	public MessageBundle uiBundle;

	public MessageBundle rangeBundle;

	public GameObject loadingUI;

	public GameObject demoUI;

	[FormerlySerializedAs("buttonIconList")]
	public IconEntry[] xboxButtonIconList;

	public IconEntry[] ps4ButtonIconList;

	public Sprite[] steamButtonIcons;

	public IconEntry[] mouseIconList;

	public Sprite unknownButtonIcon;

	private Dictionary<InputDeviceStyle, Dictionary<string, Sprite>> deviceButtonIconDict;

	public GameObject CreateCreditsPrefab(bool aboutCredits)
	{
		return UnityEngine.Object.Instantiate(aboutCredits ? aboutCreditsPrefab : storyCreditsPrefab);
	}

	public void Awake()
	{
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(InitBundles);
		deviceButtonIconDict = new Dictionary<InputDeviceStyle, Dictionary<string, Sprite>>
		{
			{
				InputDeviceStyle.XboxOne,
				xboxButtonIconList.ToDictionary((IconEntry e) => e.keyStr, (IconEntry e) => e.icon)
			},
			{
				InputDeviceStyle.PlayStation4,
				ps4ButtonIconList.ToDictionary((IconEntry e) => e.keyStr, (IconEntry e) => e.icon)
			},
			{
				InputDeviceStyle.Unknown,
				mouseIconList.ToDictionary((IconEntry e) => e.keyStr, (IconEntry e) => e.icon)
			}
		};
	}

	public void InitBundles(MessageDirector msgDir)
	{
		uiBundle = msgDir.GetBundle("ui");
		rangeBundle = msgDir.GetBundle("range");
	}

	public GameObject CreatePurchaseButton(string itemName, Sprite costIcon, int cost, UnityAction onButton)
	{
		GameObject obj = UnityEngine.Object.Instantiate(purchaseButtonPrefab);
		Button component = obj.GetComponent<Button>();
		TMP_Text component2 = obj.transform.Find("ItemName").gameObject.GetComponent<TMP_Text>();
		Image component3 = obj.transform.Find("Bottom/CostIcon").gameObject.GetComponent<Image>();
		TMP_Text component4 = obj.transform.Find("Bottom/CostAmount").gameObject.GetComponent<TMP_Text>();
		component2.text = itemName;
		component3.sprite = costIcon;
		component4.text = cost.ToString();
		component.onClick.AddListener(onButton);
		return obj;
	}

	public GameObject CreateTeleportButton(string teleportName, bool enableTeleport, UnityAction onButton)
	{
		GameObject obj = UnityEngine.Object.Instantiate(teleportButtonPrefab);
		Button component = obj.GetComponent<Button>();
		component.interactable = enableTeleport;
		obj.transform.Find("TeleportName").gameObject.GetComponent<TMP_Text>().text = rangeBundle.Get("m.teleporter." + teleportName);
		component.onClick.AddListener(onButton);
		return obj;
	}

	public GameObject CreatePurchaseUI(Sprite titleIcon, string titleKey, PurchaseUI.Purchasable[] purchasables, bool hideNubuckCost, PurchaseUI.OnClose onClose, bool unavailInMainList = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(purchaseUIPrefab);
		PurchaseUI component = gameObject.GetComponent<PurchaseUI>();
		component.Init(titleIcon, titleKey, onClose);
		foreach (PurchaseUI.Purchasable purchasable in purchasables)
		{
			component.AddButton(purchasable, unavailInMainList);
		}
		if (hideNubuckCost)
		{
			component.HideNubuckCost();
		}
		component.SelectFirst();
		return gameObject;
	}

	public GameObject CreateRancherChoiceUI(List<string> rancherIds)
	{
		GameObject obj = UnityEngine.Object.Instantiate(rancherChoicePrefab);
		obj.GetComponent<RancherChoiceUI>().Init(rancherIds);
		return obj;
	}

	public GameObject CreateErrorDialog(string errorMsg)
	{
		GameObject obj = UnityEngine.Object.Instantiate(errorDialogPrefab);
		obj.transform.Find("MainPanel/Message").GetComponent<TMP_Text>().text = uiBundle.Xlate(errorMsg);
		return obj;
	}

	public GameObject CreateErrorDialogWithArgs(string errorMsg, params object[] args)
	{
		GameObject obj = UnityEngine.Object.Instantiate(errorDialogPrefab);
		obj.transform.Find("MainPanel/Message").GetComponent<TMP_Text>().text = uiBundle.Get(errorMsg, args);
		return obj;
	}

	public GameObject CreateConfirmDialog(string confirmMsg, ConfirmUI.OnConfirm onConfirm)
	{
		GameObject obj = UnityEngine.Object.Instantiate(confirmDialogPrefab);
		obj.transform.Find("MainPanel/Message").GetComponent<TMP_Text>().text = uiBundle.Xlate(confirmMsg);
		obj.GetComponent<ConfirmUI>().onConfirm = onConfirm;
		return obj;
	}

	public Sprite GetButtonIcon(InputDeviceStyle inputDevice, string keyStr, out bool iconFound)
	{
		bool num = InputDirector.UsingGamepad();
		InputDeviceStyle key = InputDeviceStyle.Unknown;
		if (num)
		{
			key = ((inputDevice == InputDeviceStyle.PlayStation2 || inputDevice == InputDeviceStyle.PlayStation3 || inputDevice == InputDeviceStyle.PlayStation4) ? InputDeviceStyle.PlayStation4 : InputDeviceStyle.XboxOne);
		}
		iconFound = false;
		if (keyStr != null && deviceButtonIconDict.ContainsKey(key))
		{
			Dictionary<string, Sprite> dictionary = deviceButtonIconDict[key];
			if (dictionary.ContainsKey(keyStr))
			{
				iconFound = true;
				return dictionary[keyStr];
			}
		}
		return unknownButtonIcon;
	}

	internal Sprite GetSteamButtonIcon(int originId, out bool iconFound)
	{
		if (originId >= 0 && originId < steamButtonIcons.Length)
		{
			iconFound = true;
			return steamButtonIcons[originId];
		}
		iconFound = false;
		return unknownButtonIcon;
	}
}
