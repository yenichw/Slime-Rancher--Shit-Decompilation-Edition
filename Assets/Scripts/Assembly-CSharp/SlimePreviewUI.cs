using System.Collections.Generic;
using System.Linq;
using DLCPackage;
using UnityEngine;
using UnityEngine.UI;

public class SlimePreviewUI : MonoBehaviour
{
	public SlimeDefinitions slimeDefinitions;

	public SlimeAppearanceApplicator slimeAppearanceApplicator;

	public SlimePreviewCamera slimeCam;

	public Toggle groundedToggle;

	public Dropdown typeDropdown;

	public Dropdown appearanceDropdown;

	public Button refreshButton;

	public Button lookAtButton;

	private SlimeDefinition currentSlimeDefinition;

	private SlimeAppearance currentAppearance;

	private readonly List<SlimeAppearance> currentSlimeAppearances = new List<SlimeAppearance>();

	private DLCDirector DLCDirector;

	public void Awake()
	{
		DLCDirector = SRSingleton<GameContext>.Instance.DLCDirector;
		DLCDirector.onPackageInstalled += OnDLCPackageInstalled;
	}

	public void OnDestroy()
	{
		if (DLCDirector != null)
		{
			DLCDirector.onPackageInstalled -= OnDLCPackageInstalled;
			DLCDirector = null;
		}
	}

	private void Start()
	{
		typeDropdown.ClearOptions();
		appearanceDropdown.ClearOptions();
		typeDropdown.AddOptions(slimeDefinitions.Slimes.Select((SlimeDefinition slime) => slime.Name).ToList());
		typeDropdown.onValueChanged.AddListener(OnTypeSelected);
		appearanceDropdown.onValueChanged.AddListener(OnAppearanceSelected);
		refreshButton.onClick.AddListener(RefreshAppearance);
		lookAtButton.onClick.AddListener(delegate
		{
			slimeCam.ResetCamToTarget(slimeAppearanceApplicator.transform);
		});
		groundedToggle.onValueChanged.AddListener(delegate
		{
			RefreshAppearance();
		});
		OnTypeSelected(0);
	}

	private void OnTypeSelected(int index)
	{
		currentSlimeDefinition = slimeDefinitions.Slimes[index];
		currentSlimeAppearances.Clear();
		currentSlimeAppearances.AddRange(from appearance in currentSlimeDefinition.Appearances.SelectMany((SlimeAppearance appearance) => new SlimeAppearance[3] { appearance, appearance.QubitAppearance, appearance.ShockedAppearance })
			where appearance != null
			select appearance);
		appearanceDropdown.ClearOptions();
		appearanceDropdown.AddOptions(currentSlimeAppearances.Select((SlimeAppearance appearance) => appearance.name).ToList());
		OnAppearanceSelected(0);
	}

	private void OnAppearanceSelected(int index)
	{
		currentAppearance = currentSlimeAppearances[index];
		RefreshAppearance();
	}

	private void RefreshAppearance()
	{
		slimeAppearanceApplicator.SlimeDefinition = currentSlimeDefinition;
		slimeAppearanceApplicator.Appearance = currentAppearance;
		slimeAppearanceApplicator.ApplyAppearance();
		slimeAppearanceApplicator.transform.localScale = new Vector3(currentSlimeDefinition.PrefabScale, currentSlimeDefinition.PrefabScale, currentSlimeDefinition.PrefabScale);
		EnableBasedOnGrounded[] componentsInChildren = slimeAppearanceApplicator.GetComponentsInChildren<EnableBasedOnGrounded>();
		groundedToggle.gameObject.SetActive(componentsInChildren.Length != 0);
		EnableBasedOnGrounded[] array = componentsInChildren;
		foreach (EnableBasedOnGrounded enableBasedOnGrounded in array)
		{
			enableBasedOnGrounded.gameObject.SetActive(enableBasedOnGrounded.enableOnGrounded ^ groundedToggle.isOn);
		}
		DeactivateOnHeld[] componentsInChildren2 = slimeAppearanceApplicator.GetComponentsInChildren<DeactivateOnHeld>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
	}

	private void OnDLCPackageInstalled(Id package)
	{
		if (package == Id.SECRET_STYLE)
		{
			OnTypeSelected(0);
		}
	}
}
