using System;
using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchaseUI : BaseUI
{
	public delegate void OnClose();

	public delegate void OnSelected(Purchasable purchasable);

	public class Category
	{
		public string name;

		public Purchasable[] items;

		public Category(string name, params Purchasable[] items)
		{
			this.name = name;
			this.items = items;
		}
	}

	public class Purchasable
	{
		public string nameKey;

		public Sprite icon;

		public Sprite mainImg;

		public string descKey;

		public int cost;

		public PediaDirector.Id? pediaId;

		public UnityAction onPurchase;

		public Func<bool> unlocked;

		public Func<bool> avail;

		public string btnOverride;

		public string warning;

		public Func<int> currCount;

		public GadgetDefinition.CraftCost[] craftCosts;

		public OnSelected onSelected;

		public bool requireHoldToPurchase;

		public Purchasable(string nameKey, Sprite icon, Sprite mainImg, string descKey, int cost, PediaDirector.Id? pediaId, UnityAction onPurchase, Func<bool> unlocked, Func<bool> avail, string btnOverride = null, string warning = null, Func<int> currCount = null, GadgetDefinition.CraftCost[] craftCosts = null, bool requireHoldToPurchase = false)
		{
			this.nameKey = nameKey;
			this.icon = icon;
			this.mainImg = mainImg;
			this.descKey = descKey;
			this.cost = cost;
			this.pediaId = pediaId;
			this.onPurchase = onPurchase;
			this.unlocked = unlocked;
			this.avail = avail;
			this.btnOverride = btnOverride;
			this.warning = warning;
			this.currCount = currCount;
			this.craftCosts = craftCosts;
			this.requireHoldToPurchase = requireHoldToPurchase;
		}

		public Purchasable()
		{
		}
	}

	[Flags]
	public enum Panel
	{
		NONE = 0,
		SELECTION = 1,
		ACTION = 2,
		PLACEHOLDER = 4,
		COSTS = 8,
		LOADING = 0x10,
		DEFAULT = 3
	}

	[Tooltip("Internal title image")]
	public Image titleImg;

	[Tooltip("Internal title text")]
	public TMP_Text titleText;

	[Tooltip("Internal button content panel")]
	public GameObject buttonListPanel;

	[Tooltip("Internal unavailable-item button content panel")]
	public GameObject unavailButtonListPanel;

	[Tooltip("Internal cost content panel")]
	public GameObject costListPanel;

	[Tooltip("Internal selected panel title image")]
	public Image selectedImg;

	[Tooltip("Internal selected panel title text")]
	public TMP_Text selectedTitle;

	[Tooltip("Internal selected panel description text")]
	public TMP_Text selectedDesc;

	[Tooltip("Internal selected panel purchase cost text")]
	public TMP_Text selectedCost;

	[Tooltip("Internal selected panel purchase cost panel")]
	public GameObject selectedCostPanel;

	[Tooltip("Internal selected panel purchase no-cost placeholder panel")]
	public GameObject selectedNoCostPanel;

	[Tooltip("Internal selected panel pedia button")]
	public Button selectedPediaBtn;

	[Tooltip("Internal selected panel purchase button")]
	public Button purchaseBtn;

	[Tooltip("Internal selected panel purchase button text")]
	public TMP_Text purchaseBtnText;

	[Tooltip("Internal hold to purchase button (for example, used when demolishing silos).")]
	public Button holdToPurchaseBtn;

	[Tooltip("Internal hold to purchase button text (for example, used when demolishing silos).")]
	public TMP_Text holdToPurchaseBtnText;

	[Tooltip("Internal main action right-side panel")]
	public GameObject actionPanel;

	[Tooltip("Internal placeholder right-side panel")]
	public GameObject placeholderPanel;

	[Tooltip("Internal panel for type-specific customizations")]
	public GameObject customizationPanel;

	[Tooltip("Internal costs panel far-right, not always active.")]
	public GameObject costsPanel;

	[Tooltip("Internal category tabs panel.")]
	public GameObject tabsPanel;

	[Tooltip("Internal item warning text.")]
	public TMP_Text warningText;

	[Tooltip("Internal scrolling region for selection list.")]
	public ScrollRect selectionScroller;

	[Tooltip("Internal loading panel.")]
	public GameObject loadingPanel;

	[Tooltip("Internal selection panel.")]
	public GameObject selectionPanel;

	public GameObject buttonListItemPrefab;

	public GameObject costListItemPrefab;

	public GameObject catTabPrefab;

	public Material unavailIconMat;

	public GameObject purchaseFX;

	private Purchasable selected;

	private Dictionary<string, Category> categories;

	private Category selectedCategory;

	public OnSelected onSelected;

	private OnClose onClose;

	private MessageBundle pediaBundle;

	private MessageBundle actorBundle;

	private Dictionary<Toggle, Purchasable> toggleMap = new Dictionary<Toggle, Purchasable>();

	private Dictionary<string, Toggle> categoryMap = new Dictionary<string, Toggle>();

	private Dictionary<Purchasable, GameObject> buttonDict = new Dictionary<Purchasable, GameObject>();

	private bool hideNubuckCosts;

	private bool waitForPedia;

	private string purchaseMsg = "b.purchase";

	private string purchaseUnavailMsg = "b.sold_out";

	public override void Awake()
	{
		base.Awake();
		pediaBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia");
		actorBundle = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("actor");
		actionPanel.SetActive(value: false);
		placeholderPanel.SetActive(value: true);
	}

	public void Init(Sprite icon, string titleKey, OnClose onClose)
	{
		titleImg.sprite = icon;
		titleText.text = pediaBundle.Xlate(titleKey);
		this.onClose = onClose;
		toggleMap.Clear();
	}

	public void PlayPurchaseFX()
	{
		if (purchaseFX != null)
		{
			UnityEngine.Object.Instantiate(purchaseFX).transform.SetParent(purchaseBtn.transform, worldPositionStays: false);
		}
	}

	public void SetPurchaseMsgs(string availMsg, string unavailMsg)
	{
		purchaseMsg = availMsg;
		purchaseUnavailMsg = unavailMsg;
		if (selected != null)
		{
			Select(selected);
		}
	}

	public void AddButton(Purchasable purchasable, bool unavailInMainList)
	{
		GameObject gameObject = CreateButton(purchasable);
		if (purchasable.avail() || unavailInMainList)
		{
			gameObject.transform.SetParent(buttonListPanel.transform, worldPositionStays: false);
		}
		else
		{
			gameObject.transform.SetParent(unavailButtonListPanel.transform, worldPositionStays: false);
		}
		gameObject.SetActive(purchasable.unlocked());
		gameObject.GetRequiredComponent<Toggle>().group = buttonListPanel.GetRequiredComponentInParent<ToggleGroup>(includeInactive: true);
		buttonDict[purchasable] = gameObject;
	}

	public void ClearButtons()
	{
		foreach (GameObject value in buttonDict.Values)
		{
			Destroyer.Destroy(value, "PurchaseUI.ClearButtons");
		}
	}

	public void Rebuild(bool unavailInMainList)
	{
		foreach (KeyValuePair<Purchasable, GameObject> item in buttonDict)
		{
			UpdateButton(item.Key, item.Value);
			item.Value.SetActive(item.Key.unlocked());
			Transform transform = ((item.Key.avail() || unavailInMainList) ? buttonListPanel.transform : unavailButtonListPanel.transform);
			if (item.Value.transform.parent != transform)
			{
				item.Value.transform.SetParent(transform, worldPositionStays: false);
			}
		}
		if (selectedCategory != null)
		{
			ActivateCategory(selectedCategory);
		}
		if (buttonDict.ContainsKey(selected) && (selected.avail() || unavailInMainList))
		{
			buttonDict[selected].GetComponent<Toggle>().Select();
		}
		else
		{
			SelectFirst();
		}
	}

	public void HideSelectionPanel()
	{
		selectionPanel.SetActive(value: false);
	}

	private void SetCosts(GadgetDefinition.CraftCost[] costs)
	{
		costsPanel.SetActive(value: true);
		ClearCostListPanel(isRequiresTextEnabled: true);
		foreach (GadgetDefinition.CraftCost cost in costs)
		{
			CreateCost(cost).transform.SetParent(costListPanel.transform, worldPositionStays: false);
		}
	}

	private void ClearCostListPanel(bool isRequiresTextEnabled)
	{
		costListPanel.transform.GetChild(0).gameObject.SetActive(isRequiresTextEnabled);
		for (int i = 1; i < costListPanel.transform.childCount; i++)
		{
			Destroyer.Destroy(costListPanel.transform.GetChild(i).gameObject, "PurchaseUI.SetCosts");
		}
	}

	public void HideNubuckCost()
	{
		hideNubuckCosts = true;
	}

	private GameObject CreateButton(Purchasable purchasable)
	{
		GameObject buttonObj = UnityEngine.Object.Instantiate(buttonListItemPrefab);
		UpdateButton(purchasable, buttonObj);
		Toggle component = buttonObj.GetComponent<Toggle>();
		component.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				Select(purchasable);
			}
		});
		OnSelectDelegator.Create(buttonObj, delegate
		{
			buttonObj.GetComponent<Toggle>().isOn = true;
		});
		toggleMap[component] = purchasable;
		return buttonObj;
	}

	private void UpdateButton(Purchasable purchasable, GameObject buttonObj)
	{
		MeshToggleButtonStyler component = buttonObj.GetComponent<MeshToggleButtonStyler>();
		TMP_Text component2 = buttonObj.transform.Find("Content/Name").gameObject.GetComponent<TMP_Text>();
		Image component3 = buttonObj.transform.Find("Content/Icon").gameObject.GetComponent<Image>();
		TMP_Text component4 = buttonObj.transform.Find("Content/Count").gameObject.GetComponent<TMP_Text>();
		component2.text = pediaBundle.Xlate(purchasable.nameKey);
		component3.sprite = purchasable.icon;
		int num = ((purchasable.currCount == null) ? (-1) : purchasable.currCount());
		component4.text = uiBundle.Xlate(MessageUtil.Tcompose("l.curr_count", num));
		component4.gameObject.SetActive(num >= 0);
		if (!purchasable.avail())
		{
			component.ChangeStyle("ListEntryUnavail");
			component3.material = unavailIconMat;
		}
		else
		{
			component.ChangeStyle("ListEntry");
			component3.material = null;
		}
	}

	private GameObject CreateCost(GadgetDefinition.CraftCost cost)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(costListItemPrefab);
		TMP_Text component = gameObject.transform.Find("Content/Name").gameObject.GetComponent<TMP_Text>();
		Image component2 = gameObject.transform.Find("Content/Icon").gameObject.GetComponent<Image>();
		TMP_Text component3 = gameObject.transform.Find("CountsOuterPanel/CountsPanel/Counts").gameObject.GetComponent<TMP_Text>();
		Sprite icon = SRSingleton<GameContext>.Instance.LookupDirector.GetIcon(cost.id);
		int refineryCount = SRSingleton<SceneContext>.Instance.GadgetDirector.GetRefineryCount(cost.id);
		component.text = actorBundle.Xlate("l." + cost.id.ToString().ToLowerInvariant());
		component2.sprite = icon;
		component3.text = uiBundle.Xlate(MessageUtil.Tcompose("m.count_of_required", refineryCount, cost.amount));
		if (refineryCount < cost.amount)
		{
			component3.color = Color.red;
		}
		return gameObject;
	}

	public void Select(Purchasable purchasable)
	{
		actionPanel.SetActive(value: true);
		placeholderPanel.SetActive(value: false);
		selected = purchasable;
		selectedImg.sprite = purchasable.mainImg;
		selectedTitle.text = pediaBundle.Xlate(purchasable.nameKey);
		selectedDesc.text = pediaBundle.Xlate(purchasable.descKey);
		selectedCost.text = purchasable.cost.ToString();
		selectedCostPanel.SetActive(purchasable.cost > 0 && !hideNubuckCosts);
		selectedNoCostPanel.SetActive(purchasable.cost <= 0 && !hideNubuckCosts);
		selectedPediaBtn.gameObject.SetActive(purchasable.pediaId.HasValue);
		SetupActivePurchaseButton(purchasable);
		if (purchasable.craftCosts != null)
		{
			SetCosts(purchasable.craftCosts);
		}
		warningText.gameObject.SetActive(purchasable.warning != null);
		if (purchasable.warning != null)
		{
			warningText.text = uiBundle.Xlate(purchasable.warning);
		}
		if (onSelected != null)
		{
			onSelected(purchasable);
		}
		if (purchasable.onSelected != null)
		{
			purchasable.onSelected(purchasable);
		}
	}

	private void SetupActivePurchaseButton(Purchasable purchasable)
	{
		string text = uiBundle.Xlate((purchasable.btnOverride != null) ? purchasable.btnOverride : (purchasable.avail() ? purchaseMsg : purchaseUnavailMsg));
		bool interactable = purchasable.avail();
		if (purchasable.requireHoldToPurchase)
		{
			purchaseBtn.gameObject.SetActive(value: false);
			holdToPurchaseBtn.gameObject.SetActive(value: true);
			holdToPurchaseBtn.interactable = interactable;
			holdToPurchaseBtnText.text = text;
		}
		else
		{
			purchaseBtn.gameObject.SetActive(value: true);
			holdToPurchaseBtn.gameObject.SetActive(value: false);
			purchaseBtn.interactable = interactable;
			purchaseBtnText.text = text;
		}
	}

	public void SetCategories(List<Category> categories)
	{
		tabsPanel.SetActive(value: true);
		Toggle toggle = null;
		this.categories = new Dictionary<string, Category>();
		foreach (Category category in categories)
		{
			this.categories[category.name] = category;
		}
		selectedCategory = ((categories.Count > 0) ? categories[0] : null);
		foreach (Category category2 in categories)
		{
			GameObject obj = UnityEngine.Object.Instantiate(catTabPrefab);
			obj.transform.SetParent(tabsPanel.transform, worldPositionStays: false);
			obj.GetComponentInChildren<XlateText>().SetKey("b." + category2.name);
			Toggle component = obj.GetComponent<Toggle>();
			if (toggle == null)
			{
				toggle = component;
			}
			component.group = tabsPanel.GetComponent<ToggleGroup>();
			Category fCategory = category2;
			component.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn)
				{
					ActivateCategory(fCategory);
					SelectFirst();
				}
			});
			categoryMap[category2.name] = component;
		}
		if (toggle != null)
		{
			toggle.isOn = true;
		}
	}

	private void ActivateCategory(Category category)
	{
		bool flag = false;
		foreach (KeyValuePair<Toggle, Purchasable> item in toggleMap)
		{
			bool flag2 = Array.IndexOf(category.items, item.Value) != -1 && item.Value.unlocked();
			item.Key.gameObject.SetActive(flag2);
			item.Key.isOn = false;
			flag = flag || flag2;
		}
		actionPanel.SetActive(flag);
		placeholderPanel.SetActive(!flag);
		selectedCategory = category;
	}

	public void SelectFirst()
	{
		for (int i = 0; i < buttonListPanel.transform.childCount; i++)
		{
			GameObject gameObject = buttonListPanel.transform.GetChild(i).gameObject;
			if (gameObject.activeSelf)
			{
				gameObject.GetComponent<Toggle>().Select();
				return;
			}
		}
		for (int j = 0; j < unavailButtonListPanel.transform.childCount; j++)
		{
			GameObject gameObject2 = unavailButtonListPanel.transform.GetChild(j).gameObject;
			if (gameObject2.activeSelf)
			{
				gameObject2.GetComponent<Toggle>().Select();
				return;
			}
		}
		actionPanel.SetActive(value: false);
		placeholderPanel.SetActive(value: true);
		ClearCostListPanel(isRequiresTextEnabled: false);
		selected = null;
	}

	public void Pedia()
	{
		if (waitForPedia || selected == null || !selected.pediaId.HasValue)
		{
			return;
		}
		waitForPedia = true;
		PediaUI component = SRSingleton<SceneContext>.Instance.PediaDirector.ShowPedia(selected.pediaId.Value).GetComponent<PediaUI>();
		component.onDestroy = (OnDestroyDelegate)Delegate.Combine(component.onDestroy, (OnDestroyDelegate)delegate
		{
			if (SRSingleton<SceneContext>.Instance != null)
			{
				ReselectOnReturnFromPedia();
				waitForPedia = false;
			}
		});
	}

	public void Purchase()
	{
		if (!waitForPedia && selected != null)
		{
			selected.onPurchase();
		}
	}

	public override void Close()
	{
		base.Close();
		if (onClose != null)
		{
			onClose();
		}
	}

	public Category GetSelectedCategory()
	{
		return selectedCategory;
	}

	public void Resize(float widthSelection, float widthAction)
	{
		selectionScroller.GetComponent<LayoutElement>().preferredWidth = widthSelection;
		actionPanel.GetComponent<LayoutElement>().preferredWidth = widthAction;
		loadingPanel.GetComponent<LayoutElement>().preferredWidth = widthSelection + widthAction;
	}

	public void ReselectOnReturnFromPedia()
	{
		if (selected != null && buttonDict.ContainsKey(selected))
		{
			buttonDict[selected].GetComponent<Toggle>().Select();
		}
		else
		{
			SelectFirst();
		}
	}

	public void SetActivePanels(Panel active)
	{
		selectionPanel.SetActive((active & Panel.SELECTION) != 0);
		actionPanel.SetActive((active & Panel.ACTION) != 0);
		placeholderPanel.SetActive((active & Panel.PLACEHOLDER) != 0);
		costsPanel.SetActive((active & Panel.COSTS) != 0);
		loadingPanel.SetActive((active & Panel.LOADING) != 0);
	}
}
