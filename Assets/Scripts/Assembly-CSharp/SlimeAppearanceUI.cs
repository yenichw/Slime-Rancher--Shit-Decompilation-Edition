using System;
using System.Collections.Generic;
using System.Linq;
using DLCPackage;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlimeAppearanceUI : BaseUI
{
	public GameObject contentPanel;

	public GameObject placeholderPanel;

	public GameObject buttonListPanel;

	public GameObject appearancePanel;

	public GameObject tutorialPanel;

	public Button showTutorialButton;

	public GameObject buttonListItemPrefab;

	public SECTR_AudioBus sfxBus;

	public SECTR_AudioCue openCue;

	public SECTR_AudioCue closeCue;

	public Vector2 confirmCueVolumeRange = Vector2.one;

	public SlimeDefinitions slimeDefinitions;

	public SlimeAppearanceDirector slimeAppearanceDirector;

	public XlateText appearanceOneNameText;

	public XlateText appearanceTwoNameText;

	public Button toggleButton;

	public Button selectAreaOne;

	public Button selectAreaTwo;

	public GameObject slimeAppearanceCarouselPrefab;

	public SECTR_AudioCue tabbySelectionCue;

	public SECTR_AudioCue saberSelectionCue;

	public int confirmCueMaxInstances = 5;

	private readonly HashSet<Identifiable.Id> tabbySoundSlimes = new HashSet<Identifiable.Id>
	{
		Identifiable.Id.TABBY_SLIME,
		Identifiable.Id.LUCKY_SLIME,
		Identifiable.Id.HUNTER_SLIME
	};

	private GameObject slimeAppearanceCarouselCamSetup;

	private SlimeAppearanceCarousel slimeAppearanceCarousel;

	private PediaDirector pediaDirector;

	private TutorialDirector tutorialDirector;

	private SlimeDefinition currentSlime;

	private SECTR_AudioCue confirmCue;

	private bool tutorialOnStack;

	public static bool IsEnabled()
	{
		return SRSingleton<GameContext>.Instance.DLCDirector.IsPackageInstalledAndEnabled(Id.SECRET_STYLE);
	}

	public override void Awake()
	{
		base.Awake();
		contentPanel.SetActive(value: false);
		placeholderPanel.SetActive(value: true);
		tutorialDirector = SRSingleton<SceneContext>.Instance.TutorialDirector;
		pediaDirector = SRSingleton<SceneContext>.Instance.PediaDirector;
		if (!tutorialDirector.IsCompletedOrDisabled(TutorialDirector.Id.APPEARANCE_UI))
		{
			ShowTutorial(onTop: false);
		}
		else
		{
			HideTutorial();
		}
		slimeAppearanceCarouselCamSetup = UnityEngine.Object.Instantiate(slimeAppearanceCarouselPrefab);
		slimeAppearanceCarousel = slimeAppearanceCarouselCamSetup.GetComponentInChildren<SlimeAppearanceCarousel>();
		toggleButton.onClick.AddListener(delegate
		{
			if (slimeAppearanceDirector.GetChosenSlimeAppearance(currentSlime) == currentSlime.Appearances.ElementAt(0))
			{
				slimeAppearanceCarousel.ConfirmSlimeAppearance(1);
				SetAppearanceSelectableStates(appearanceOne: true, appearanceTwo: false);
			}
			else
			{
				slimeAppearanceCarousel.ConfirmSlimeAppearance(0);
				SetAppearanceSelectableStates(appearanceOne: false, appearanceTwo: true);
			}
		});
		selectAreaOne.onClick.AddListener(delegate
		{
			slimeAppearanceCarousel.ConfirmSlimeAppearance(0);
			SetAppearanceSelectableStates(appearanceOne: false, appearanceTwo: true);
		});
		selectAreaTwo.onClick.AddListener(delegate
		{
			slimeAppearanceCarousel.ConfirmSlimeAppearance(1);
			SetAppearanceSelectableStates(appearanceOne: true, appearanceTwo: false);
		});
		slimeAppearanceCarousel.onSlimeAppearanceConfirmed += delegate(SlimeDefinition definition, SlimeAppearance appearance)
		{
			Play(confirmCue);
			slimeAppearanceDirector.UpdateChosenSlimeAppearance(definition, appearance);
		};
		List<SlimeDefinition> list = slimeDefinitions.Slimes.Where(ShouldShowSlimeInList).ToList();
		PediaSortSlimes(list);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			SlimeDefinition slime = list[i];
			GameObject gameObject = AddButton(slime);
			if (!flag && IsSlimeAppearanceMenuUnlocked(slime))
			{
				gameObject.AddComponent<InitSelected>();
				flag = true;
			}
		}
	}

	private void SetAppearanceSelectableStates(bool appearanceOne, bool appearanceTwo)
	{
		selectAreaOne.interactable = appearanceOne;
		selectAreaTwo.interactable = appearanceTwo;
	}

	private void PediaSortSlimes(List<SlimeDefinition> slimes)
	{
		slimes.Sort((SlimeDefinition slimeOne, SlimeDefinition slimeTwo) => pediaDirector.GetPediaId(slimeOne.IdentifiableId).Value.CompareTo(pediaDirector.GetPediaId(slimeTwo.IdentifiableId).Value));
	}

	private bool ShouldShowSlimeInList(SlimeDefinition slime)
	{
		if (!slime.IsLargo)
		{
			return slime.Appearances.Count() > 1;
		}
		return false;
	}

	private bool IsSlimeAppearanceMenuUnlocked(SlimeDefinition slime)
	{
		return slimeAppearanceDirector.GetUnlockedAppearances(slime).Count() > 1;
	}

	public void OnEnable()
	{
		Play(openCue);
	}

	public void OnDisable()
	{
		UnityEngine.Object.Destroy(slimeAppearanceCarouselCamSetup);
		Play(closeCue);
	}

	public void ShowTutorial(bool onTop)
	{
		appearancePanel.SetActive(value: false);
		tutorialPanel.SetActive(value: true);
		showTutorialButton.gameObject.SetActive(value: false);
		tutorialOnStack = onTop;
	}

	public void HideTutorial()
	{
		tutorialDirector.MarkTutorialCompleted(TutorialDirector.Id.APPEARANCE_UI);
		appearancePanel.SetActive(value: true);
		tutorialPanel.SetActive(value: false);
		showTutorialButton.gameObject.SetActive(value: true);
		tutorialOnStack = false;
	}

	private GameObject AddButton(SlimeDefinition slime)
	{
		GameObject obj = CreateButton(slime);
		obj.transform.SetParent(buttonListPanel.transform, worldPositionStays: false);
		obj.GetComponent<Toggle>().group = buttonListPanel.GetComponent<ToggleGroup>();
		return obj;
	}

	private GameObject CreateButton(SlimeDefinition slime)
	{
		GameObject buttonObj = UnityEngine.Object.Instantiate(buttonListItemPrefab);
		Toggle component = buttonObj.GetComponent<Toggle>();
		TMP_Text component2 = buttonObj.transform.Find("Name").gameObject.GetComponent<TMP_Text>();
		Image component3 = buttonObj.transform.Find("Icon").gameObject.GetComponent<Image>();
		if (IsSlimeAppearanceMenuUnlocked(slime))
		{
			component2.text = Identifiable.GetName(slime.IdentifiableId);
			component3.sprite = slime.Appearances.First().Icon;
			UnityAction<bool> onButton = delegate(bool isOn)
			{
				if (isOn)
				{
					Select(slime);
				}
			};
			component.onValueChanged.AddListener(onButton);
			OnSelectDelegator.Create(buttonObj, delegate
			{
				onButton(arg0: true);
				buttonObj.GetComponent<Toggle>().isOn = true;
			});
		}
		else
		{
			component2.text = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia").Get("t." + Enum.GetName(typeof(PediaDirector.Id), pediaDirector.lockedEntry.id).ToLowerInvariant());
			component3.sprite = pediaDirector.lockedEntry.icon;
			component.interactable = false;
		}
		return buttonObj;
	}

	protected override void OnCancelPressed()
	{
		if (tutorialOnStack)
		{
			HideTutorial();
		}
		else
		{
			base.OnCancelPressed();
		}
	}

	private void Select(SlimeDefinition slime)
	{
		currentSlime = slime;
		placeholderPanel.SetActive(value: false);
		contentPanel.SetActive(value: true);
		slimeAppearanceCarousel.ShowSlime(slime);
		appearanceOneNameText.SetKey(slime.Appearances.ElementAt(0).NameXlateKey);
		appearanceTwoNameText.SetKey(slime.Appearances.ElementAt(1).NameXlateKey);
		if (slimeAppearanceDirector.GetChosenSlimeAppearance(slime) == slime.Appearances.ElementAt(0))
		{
			SetAppearanceSelectableStates(appearanceOne: false, appearanceTwo: true);
		}
		else
		{
			SetAppearanceSelectableStates(appearanceOne: true, appearanceTwo: false);
		}
		confirmCue = ScriptableObject.CreateInstance<SECTR_AudioCue>();
		List<SECTR_AudioCue.ClipData> list = new List<SECTR_AudioCue.ClipData>();
		if (tabbySoundSlimes.Contains(slime.IdentifiableId))
		{
			list.AddRange(tabbySelectionCue.AudioClips);
		}
		else if (slime.IdentifiableId == Identifiable.Id.SABER_SLIME)
		{
			list.AddRange(saberSelectionCue.AudioClips);
		}
		else
		{
			list.AddRange(slime.Sounds.voiceFunCue.AudioClips);
		}
		confirmCue.Bus = sfxBus;
		confirmCue.Volume = confirmCueVolumeRange;
		confirmCue.MaxInstances = confirmCueMaxInstances;
		confirmCue.Pitch = new Vector2(0.9f, 1.1f);
		confirmCue.AudioClips = list;
		confirmCue.Spatialization = SECTR_AudioCue.Spatializations.Simple2D;
	}
}
