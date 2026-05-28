using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AppearancePreviewLineup : MonoBehaviour
{
	[Header("Prefabs")]
	[Tooltip("SlimeAppearanceApplicator prefab for slime appearances.")]
	public SlimeAppearanceApplicator appearancePreviewPrefab;

	[Tooltip("SlimeAppearanceApplicator prefab for qubit appearances.")]
	public SlimeAppearanceApplicator qubitPreviewPrefab;

	[Header("Camera")]
	public SlimePreviewCamera slimePreviewCamera;

	[Header("Slime Definitions")]
	public SlimeDefinitions slimeDefinitions;

	[Header("UI")]
	public Text slimeNameText;

	public Text controlStateText;

	public Dropdown selectedSlimeDefinition;

	public Dropdown animationParamDropdown;

	public Text animationValueText;

	[Header("Spacing")]
	public float xSpacing = 2f;

	public float qubitYSpacing = 3f;

	[Header("Extra Appearances")]
	[Tooltip("Extra appearances to show in the preview.")]
	public List<SlimeAppearance> extraAppearances = new List<SlimeAppearance>();

	private int currentFocusedIndex;

	private List<SlimeAppearanceApplicator> currentAppearancePreviews = new List<SlimeAppearanceApplicator>();

	private List<SlimeDefinition> currentDefinitions = new List<SlimeDefinition>();

	private List<SlimeAppearanceApplicator> qubitPreviews = new List<SlimeAppearanceApplicator>();

	private List<MaterialStealthController> stealthControllers = new List<MaterialStealthController>();

	private bool qubitModeEnabled;

	private bool menuControlsEnabled;

	private int extraOffset;

	private int baseSlimeCount;

	private List<SlimeDefinition> baseSlimes;

	private float currentCloakOpacity = 1f;

	private float targetCloakOpacity = 1f;

	private const float OPACITY_CHANGE_PER_SEC = 2f;

	private void Start()
	{
		foreach (SlimeAppearanceApplicator currentAppearancePreview in currentAppearancePreviews)
		{
			if (currentAppearancePreview != null)
			{
				currentAppearancePreview.ApplyAppearance();
			}
		}
		selectedSlimeDefinition.options.Clear();
		baseSlimes = slimeDefinitions.Slimes.Where((SlimeDefinition slime) => !slime.IsLargo).ToList();
		baseSlimeCount = baseSlimes.Count;
		if (extraAppearances.Count > 0)
		{
			extraOffset = 1;
			selectedSlimeDefinition.options.Add(new Dropdown.OptionData("Extra Appearances"));
		}
		foreach (SlimeDefinition baseSlime in baseSlimes)
		{
			selectedSlimeDefinition.options.Add(new Dropdown.OptionData("All " + baseSlime.name));
		}
		SlimeDefinition[] slimes = slimeDefinitions.Slimes;
		foreach (SlimeDefinition slimeDefinition in slimes)
		{
			selectedSlimeDefinition.options.Add(new Dropdown.OptionData(slimeDefinition.name));
		}
		selectedSlimeDefinition.value = 0;
		selectedSlimeDefinition.RefreshShownValue();
		OnDropdownValueChanged(0);
		ToggleControls();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			ToggleControls();
		}
		if (menuControlsEnabled)
		{
			bool wasPressed = SRInput.PauseActions.menuDown.WasPressed;
			bool wasPressed2 = SRInput.PauseActions.menuUp.WasPressed;
			bool wasPressed3 = SRInput.PauseActions.menuLeft.WasPressed;
			bool wasPressed4 = SRInput.PauseActions.menuRight.WasPressed;
			if (wasPressed2 && selectedSlimeDefinition.value > 0)
			{
				selectedSlimeDefinition.value--;
			}
			else if (wasPressed && selectedSlimeDefinition.value < selectedSlimeDefinition.options.Count)
			{
				selectedSlimeDefinition.value++;
			}
			if (wasPressed3)
			{
				MoveCamera(-1);
			}
			else if (wasPressed4)
			{
				MoveCamera(1);
			}
		}
		if (targetCloakOpacity > currentCloakOpacity)
		{
			currentCloakOpacity = Mathf.Min(targetCloakOpacity, currentCloakOpacity + 2f * Time.deltaTime);
		}
		else if (targetCloakOpacity < currentCloakOpacity)
		{
			currentCloakOpacity = Mathf.Max(targetCloakOpacity, currentCloakOpacity - 2f * Time.deltaTime);
		}
		ApplyCloak();
	}

	private void ToggleControls()
	{
		slimePreviewCamera.zoomControlsEnabled = !slimePreviewCamera.zoomControlsEnabled;
		menuControlsEnabled = !slimePreviewCamera.zoomControlsEnabled;
		controlStateText.text = (menuControlsEnabled ? "Menu Controls Enabled" : "Camera Controls Enabled") + " (Tab to change)";
	}

	public void OnDropdownValueChanged(int index)
	{
		if (index < extraOffset)
		{
			List<SlimeDefinition> slimesToShow = extraAppearances.Select(delegate(SlimeAppearance appearance)
			{
				SlimeDefinition slimeDefinition = ScriptableObject.CreateInstance<SlimeDefinition>();
				slimeDefinition.PrefabScale = ((appearance.DependentAppearances.Length != 0) ? 2f : 1f);
				slimeDefinition.AppearancesDefault = new SlimeAppearance[1] { appearance };
				return slimeDefinition;
			}).ToList();
			ShowAppearances(slimesToShow);
		}
		else if (index < baseSlimeCount)
		{
			SlimeDefinition baseType = baseSlimes[index - extraOffset];
			List<SlimeDefinition> list = new List<SlimeDefinition> { baseType };
			list.AddRange(slimeDefinitions.Slimes.Where((SlimeDefinition slime) => slime.BaseSlimes.Contains(baseType)).ToList());
			ShowAppearances(list);
		}
		else
		{
			ShowAppearances(new List<SlimeDefinition> { slimeDefinitions.Slimes[index - baseSlimeCount - extraOffset] });
		}
	}

	public void ShowAppearances(List<SlimeDefinition> slimesToShow)
	{
		currentDefinitions.Clear();
		currentDefinitions.AddRange(slimesToShow);
		currentFocusedIndex = 0;
		Refresh();
	}

	public void Refresh()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(base.gameObject.transform.GetChild(i).gameObject);
			}
			else
			{
				Object.DestroyImmediate(base.gameObject.transform.GetChild(i).gameObject);
			}
		}
		qubitPreviews.Clear();
		currentAppearancePreviews.Clear();
		int index = 0;
		foreach (SlimeDefinition definition in currentDefinitions)
		{
			currentAppearancePreviews.AddRange(definition.Appearances.Select(delegate(SlimeAppearance appearance)
			{
				SlimeAppearanceApplicator slimeAppearanceApplicator = CreateAndShowAppearance(appearancePreviewPrefab, appearance, definition, index);
				if (index == 0)
				{
					PopulateAnimationDropdown(slimeAppearanceApplicator.GetComponentInChildren<Animator>());
				}
				int num = index;
				index = num + 1;
				return slimeAppearanceApplicator;
			}));
		}
		stealthControllers.Clear();
		foreach (SlimeAppearanceApplicator currentAppearancePreview in currentAppearancePreviews)
		{
			MaterialStealthController item = new MaterialStealthController(currentAppearancePreview.gameObject);
			stealthControllers.Add(item);
		}
		if (currentFocusedIndex < currentAppearancePreviews.Count)
		{
			LookAtIndex(currentFocusedIndex);
		}
		else
		{
			LookAtIndex(0);
		}
		ApplyCloak();
	}

	private SlimeAppearanceApplicator CreateAndShowAppearance(SlimeAppearanceApplicator prefab, SlimeAppearance appearance, SlimeDefinition definition, int index, float yOffset = -0.5f)
	{
		SlimeAppearanceApplicator slimeAppearanceApplicator = LineupUtils.GenerateAppearancePreview(appearancePreviewPrefab, definition, appearance);
		slimeAppearanceApplicator.transform.parent = base.transform;
		slimeAppearanceApplicator.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		slimeAppearanceApplicator.transform.localPosition = new Vector3((float)index * xSpacing, yOffset + definition.PrefabScale / 2f, 0f);
		if (qubitModeEnabled && appearance.QubitAppearance != null)
		{
			qubitPreviews.Add(CreateAndShowAppearance(qubitPreviewPrefab, appearance.QubitAppearance, definition, index, qubitYSpacing));
		}
		return slimeAppearanceApplicator;
	}

	private void PopulateAnimationDropdown(Animator animator)
	{
		animationParamDropdown.ClearOptions();
		List<string> list = new List<string>();
		AnimatorControllerParameter[] parameters = animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
			{
				list.Add(animatorControllerParameter.name);
			}
		}
		animationParamDropdown.AddOptions(list);
		UpdateCurrentAnimationValueText();
	}

	public void MoveCamera(int direction)
	{
		LookAtIndex(currentFocusedIndex + direction);
	}

	public void SetQubitMode(bool showQubits)
	{
		qubitModeEnabled = showQubits;
		Refresh();
	}

	public void SetCloakedMode(bool cloak)
	{
		targetCloakOpacity = (cloak ? 0f : 1f);
		ApplyCloak();
	}

	private void ApplyCloak()
	{
		foreach (MaterialStealthController stealthController in stealthControllers)
		{
			stealthController.SetOpacity(currentCloakOpacity);
		}
	}

	public void ToggleAnimationBool()
	{
		string text = animationParamDropdown.options[animationParamDropdown.value].text;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		foreach (SlimeAppearanceApplicator currentAppearancePreview in currentAppearancePreviews)
		{
			Animator componentInChildren = currentAppearancePreview.GetComponentInChildren<Animator>();
			componentInChildren.SetBool(text, !componentInChildren.GetBool(text));
		}
		UpdateCurrentAnimationValueText();
	}

	public void UpdateCurrentAnimationValueText()
	{
		string text = animationParamDropdown.options[animationParamDropdown.value].text;
		if (currentAppearancePreviews.Count > 0)
		{
			Animator componentInChildren = currentAppearancePreviews[0].GetComponentInChildren<Animator>();
			animationValueText.text = text + ": " + componentInChildren.GetBool(text);
		}
		else
		{
			animationValueText.text = "";
		}
	}

	private void LookAtIndex(int previewIndex)
	{
		if (previewIndex >= 0 && previewIndex < currentAppearancePreviews.Count)
		{
			currentFocusedIndex = previewIndex;
			slimePreviewCamera.ResetCamToTarget(currentAppearancePreviews[currentFocusedIndex].transform);
			slimeNameText.text = currentAppearancePreviews[currentFocusedIndex].Appearance.name;
		}
	}
}
