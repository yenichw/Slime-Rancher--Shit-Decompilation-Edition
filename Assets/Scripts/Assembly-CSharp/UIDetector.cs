using UnityEngine;

public class UIDetector : SRBehaviour
{
	public GameObject activationGuiPrefab;

	public GameObject gadgetModeActivationGuiPrefab;

	public GameObject slimeGateActivationGuiPrefab;

	public GameObject slimeGateNoKeyActivationGuiPrefab;

	public GameObject puzzleGateActivationGuiPrefab;

	public GameObject puzzleGateLockedActivationGuiPrefab;

	public GameObject treasurePodActivationGuiPrefab;

	public GameObject treasurePodInsufKeyActivationGuiPrefab;

	public GameObject treasurePodNoKeyActivationGuiPrefab;

	public float interactDistance = 2f;

	private GameObject displayingGui;

	private GameObject displayingGuiPrefab;

	private vp_FPInput fpInput;

	private Camera mainCamera;

	private WeaponVacuum weaponVac;

	public void Awake()
	{
		fpInput = GetComponentInChildren<vp_FPInput>();
	}

	public void Start()
	{
		mainCamera = Camera.main;
		weaponVac = GetComponentInChildren<WeaponVacuum>();
	}

	public void OnDisable()
	{
		Destroyer.Destroy(displayingGui, "UIDetector.OnDisable");
	}

	private void Update()
	{
		Vector3 pos = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
		Physics.Raycast(mainCamera.ScreenPointToRay(pos), out var hitInfo, interactDistance);
		UIActivator uIActivator = null;
		SlimeGateActivator slimeGateActivator = null;
		TreasurePod treasurePod = null;
		TechActivator techActivator = null;
		GadgetInteractor gadgetInteractor = null;
		GadgetSite gadgetSite = null;
		if (hitInfo.collider != null)
		{
			GameObject obj = hitInfo.collider.gameObject;
			uIActivator = obj.GetComponent<UIActivator>();
			slimeGateActivator = obj.GetComponent<SlimeGateActivator>();
			treasurePod = obj.GetComponent<TreasurePod>();
			techActivator = obj.GetComponent<TechActivator>();
			gadgetInteractor = obj.GetComponentInParent<GadgetInteractor>();
			gadgetSite = obj.GetComponentInParent<GadgetSite>();
		}
		if (uIActivator != null && uIActivator.CanActivate() && InteractionEnabled())
		{
			if (false && uIActivator.blockInExpoPrefab != null)
			{
				MaybeInstantiateDisplayGui(uIActivator.blockInExpoPrefab);
				return;
			}
			MaybeInstantiateDisplayGui(activationGuiPrefab);
			if (SRInput.Actions.interact.WasReleased)
			{
				uIActivator.Activate();
			}
		}
		else if (slimeGateActivator != null && slimeGateActivator.gateDoor.CurrState == AccessDoor.State.LOCKED && InteractionEnabled())
		{
			bool num = SRSingleton<SceneContext>.Instance.PlayerState.GetKeys() > 0;
			if (num)
			{
				MaybeInstantiateDisplayGui(slimeGateActivationGuiPrefab);
			}
			else
			{
				MaybeInstantiateDisplayGui(slimeGateNoKeyActivationGuiPrefab);
			}
			if (num && SRInput.Actions.interact.WasReleased)
			{
				slimeGateActivator.Activate();
			}
		}
		else if (treasurePod != null && treasurePod.CurrState == TreasurePod.State.LOCKED && InteractionEnabled())
		{
			bool num2 = treasurePod.HasKey();
			bool flag = treasurePod.HasAnyKey();
			if (num2)
			{
				MaybeInstantiateDisplayGui(treasurePodActivationGuiPrefab);
			}
			else if (flag)
			{
				MaybeInstantiateDisplayGui(treasurePodInsufKeyActivationGuiPrefab);
			}
			else
			{
				MaybeInstantiateDisplayGui(treasurePodNoKeyActivationGuiPrefab);
			}
			if (num2 && SRInput.Actions.interact.WasReleased)
			{
				treasurePod.Activate();
			}
		}
		else if (techActivator != null && InteractionEnabled())
		{
			GameObject customGuiPrefab = techActivator.GetCustomGuiPrefab();
			if (customGuiPrefab == null)
			{
				customGuiPrefab = activationGuiPrefab;
			}
			MaybeInstantiateDisplayGui(customGuiPrefab);
			if (SRInput.Actions.interact.WasReleased)
			{
				techActivator.Activate();
				Destroyer.Destroy(displayingGui, "UIDetector.Update");
				displayingGui = null;
			}
		}
		else if (gadgetSite != null && InteractionEnabled() && weaponVac.InGadgetMode())
		{
			if (MaybeInstantiateDisplayGui(gadgetModeActivationGuiPrefab))
			{
				RotationRowUI component = displayingGui.GetComponent<RotationRowUI>();
				if (component != null)
				{
					if (gadgetSite.HasAttached())
					{
						component.ShowRow();
					}
					else
					{
						component.HideRow();
					}
				}
			}
			if (SRInput.Actions.interact.WasReleased)
			{
				gadgetSite.Activate();
			}
			if ((bool)SRInput.Actions.vac)
			{
				gadgetSite.OnRotateCCW();
			}
			else if ((bool)SRInput.Actions.attack)
			{
				gadgetSite.OnRotateCW();
			}
		}
		else if (gadgetInteractor != null && gadgetInteractor.CanInteract() && InteractionEnabled())
		{
			MaybeInstantiateDisplayGui(activationGuiPrefab);
			if (SRInput.Actions.interact.WasReleased)
			{
				gadgetInteractor.OnInteract();
			}
		}
		else if (displayingGui != null)
		{
			Destroyer.Destroy(displayingGui, "UIDetector.Update");
			displayingGui = null;
			displayingGuiPrefab = null;
		}
	}

	private bool MaybeInstantiateDisplayGui(GameObject prefab)
	{
		if (displayingGui != null && displayingGuiPrefab != null && displayingGuiPrefab != prefab)
		{
			displayingGui.SetActive(value: false);
			Destroyer.Destroy(displayingGui, "UIDetector.InstantiateGuiPrefab");
			displayingGui = null;
			displayingGuiPrefab = null;
		}
		if (displayingGui == null)
		{
			displayingGui = Object.Instantiate(prefab);
			displayingGuiPrefab = prefab;
			return true;
		}
		return false;
	}

	private bool InteractionEnabled()
	{
		if (Time.timeScale > 0f)
		{
			return fpInput.enabled;
		}
		return false;
	}
}
