using System.Collections;
using System.Linq;
using DLCPackage;
using UnityEngine;

public class DLCManageUI : BaseUI
{
	[Tooltip("Icon displayed at the top of the modal (see PurchaseUI).")]
	public Sprite icon;

	[Tooltip("Prefab showing the 'included in...' text/icon.")]
	public DLCManageUI_IncludedInPackage includedInPackagePrefab;

	private DLCManageUI_IncludedInPackage includedInPackage;

	private const float MIN_LOADING_TIME = 0.25f;

	private PurchaseUI purchaseUI;

	public static bool IsEnabled()
	{
		DLCDirector director = SRSingleton<GameContext>.Instance.DLCDirector;
		if (Levels.isSpecial() || SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().enableDLC)
		{
			return director.GetSupportedPackages().Any((Id id) => director.HasReached(id, State.AVAILABLE));
		}
		return false;
	}

	public override void Awake()
	{
		base.Awake();
		includedInPackage = Object.Instantiate(includedInPackagePrefab.gameObject).GetComponent<DLCManageUI_IncludedInPackage>();
		purchaseUI = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(icon, "t.dlc", new PurchaseUI.Purchasable[0], hideNubuckCost: true, Close).GetComponent<PurchaseUI>();
		includedInPackage.transform.SetParent(purchaseUI.customizationPanel.transform, worldPositionStays: false);
		purchaseUI.transform.SetParent(base.transform, worldPositionStays: false);
		purchaseUI.Resize(450f, 600f);
		purchaseUI.ReselectOnReturnFromPedia();
		statusArea = purchaseUI.statusArea;
		StartCoroutine(Refresh_Coroutine());
	}

	public void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			StartCoroutine(Refresh_Coroutine());
		}
	}

	private IEnumerator Refresh_Coroutine()
	{
		float minLoadingTime = Time.unscaledTime + 0.25f;
		purchaseUI.SetActivePanels(PurchaseUI.Panel.LOADING);
		purchaseUI.ClearButtons();
		DLCDirector director = SRSingleton<GameContext>.Instance.DLCDirector;
		yield return director.RegisterPackagesAsync();
		yield return new WaitUntil(() => Time.unscaledTime >= minLoadingTime);
		PurchaseUI.Purchasable[] array = director.LoadPackageMetadatas().SelectMany((DLCPackageMetadata package) => package.contents.Select((DLCPackageMetadata.Content item) => new PurchaseUI.Purchasable
		{
			nameKey = $"m.dlc.{package.id.ToString().ToLowerInvariant()}.contents.{item.id}",
			descKey = $"m.dlc.{package.id.ToString().ToLowerInvariant()}.contents.{item.id}.desc",
			icon = item.image,
			mainImg = item.imageLarge,
			onPurchase = delegate
			{
				director.ShowPackageInStore(package.id);
			},
			onSelected = delegate
			{
				OnPackageSelected(package);
			},
			unlocked = () => director.HasReached(package.id, State.AVAILABLE),
			avail = () => !director.HasReached(package.id, State.INSTALLED),
			btnOverride = (director.HasReached(package.id, State.INSTALLED) ? "b.dlc.installed" : "b.dlc.view_in_store")
		})).ToArray();
		foreach (PurchaseUI.Purchasable purchasable in array)
		{
			purchaseUI.AddButton(purchasable, unavailInMainList: false);
		}
		purchaseUI.SetActivePanels(PurchaseUI.Panel.DEFAULT);
		purchaseUI.SelectFirst();
	}

	public override void OnBundlesAvailable(MessageDirector msg)
	{
		base.OnBundlesAvailable(msg);
		if (purchaseUI != null)
		{
			purchaseUI.Rebuild(unavailInMainList: false);
		}
	}

	private void OnPackageSelected(DLCPackageMetadata package)
	{
		if (package.contents.Count > 1)
		{
			string text = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia").Get($"m.dlc.{package.id.ToString().ToLowerInvariant()}");
			includedInPackage.text.text = uiBundle.Get("m.dlc.included_in", text);
			includedInPackage.icon.sprite = package.icon;
			includedInPackage.gameObject.SetActive(value: true);
		}
		else
		{
			includedInPackage.gameObject.SetActive(value: false);
		}
	}
}
