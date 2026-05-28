using System;
using System.Collections.Generic;
using TMPro;
using UiParticles;
using UnityEngine;
using UnityEngine.UI;

public class AmmoSlotUI : SRSingleton<AmmoSlotUI>
{
	[Serializable]
	public class Slot
	{
		public Image icon;

		public StatusBar bar;

		public Animator anim;

		public Image back;

		public Image front;

		public TMP_Text label;

		public GameObject keyBinding;
	}

	public GameObject selectedPrefab;

	private const int MAX_SLOTS = 5;

	public Slot[] slots;

	public Sprite backEmpty;

	public Sprite backFilled;

	public Sprite frontEmpty;

	public Sprite frontFilled;

	public Sprite backEmptyWater;

	public Sprite backFilledWater;

	public Sprite frontEmptyWater;

	public Sprite frontFilledWater;

	public TMP_Text liquidValueText;

	public TMP_Text liquidValueTimer;

	public GameObject liquidFXObj;

	public SlimeAppearanceDirector slimeAppearanceDirector;

	private GameObject selected;

	private PlayerState player;

	private TimeDirector timeDir;

	private LookupDirector lookupDir;

	private int animSelectedId;

	private Dictionary<Identifiable.Id, string> cachedNames = new Dictionary<Identifiable.Id, string>(Identifiable.idComparer);

	private int lastUsableSlotCount = -1;

	private int lastSelectedAmmoIndex = -1;

	private int[] lastSlotCounts = new int[5];

	private int[] lastSlotMaxAmmos = new int[5];

	private Identifiable.Id[] lastSlotIds = new Identifiable.Id[5];

	public override void Awake()
	{
		base.Awake();
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		lookupDir = SRSingleton<GameContext>.Instance.LookupDirector;
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundlesAvailable);
		animSelectedId = Animator.StringToHash("selected");
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i].keyBinding.SetActive(value: true);
		}
	}

	public void Start()
	{
		selected = UnityEngine.Object.Instantiate(selectedPrefab);
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		slimeAppearanceDirector.onSlimeAppearanceChanged += OnSlimeAppearanceChanged;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		slimeAppearanceDirector.onSlimeAppearanceChanged -= OnSlimeAppearanceChanged;
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundlesAvailable);
		}
	}

	public void OnBundlesAvailable(MessageDirector msgDir)
	{
		cachedNames.Clear();
		foreach (VacItemDefinition vacItemDefinition in lookupDir.VacItemDefinitions)
		{
			GetName(vacItemDefinition.Id, recache: true);
		}
		cachedNames[Identifiable.Id.NONE] = " ";
		for (int i = 0; i < lastSlotIds.Length; i++)
		{
			lastSlotIds[i] = Identifiable.Id.NONE;
		}
	}

	private string GetName(Identifiable.Id id, bool recache = false)
	{
		if (recache || !cachedNames.ContainsKey(id))
		{
			cachedNames[id] = Identifiable.GetName(id);
		}
		return cachedNames[id];
	}

	public void Update()
	{
		int usableSlotCount = player.Ammo.GetUsableSlotCount();
		int selectedAmmoIdx = player.Ammo.GetSelectedAmmoIdx();
		for (int i = 0; i < slots.Length; i++)
		{
			Slot slot = slots[i];
			if (usableSlotCount != lastUsableSlotCount)
			{
				ToggleSlotUsability(slot, i, usableSlotCount);
			}
			if (i >= usableSlotCount)
			{
				continue;
			}
			Identifiable.Id slotName = player.Ammo.GetSlotName(i);
			int slotMaxCount = player.Ammo.GetSlotMaxCount(i);
			int slotCount = player.Ammo.GetSlotCount(i);
			if (lastSlotCounts[i] != slotCount || lastSlotMaxAmmos[i] != slotMaxCount)
			{
				if (slotName != 0)
				{
					slot.bar.currValue = slotCount;
					slot.bar.maxValue = slotMaxCount;
					lastSlotCounts[i] = slotCount;
				}
				else
				{
					slot.bar.currValue = 0f;
					slot.bar.maxValue = slotMaxCount;
					lastSlotCounts[i] = 0;
				}
				lastSlotMaxAmmos[i] = slotMaxCount;
			}
			if (lastSlotIds[i] != slotName)
			{
				slot.icon.enabled = slotName != Identifiable.Id.NONE;
				slot.icon.sprite = GetCurrentIcon(slotName);
				slot.bar.barColor = GetCurrentColor(slotName);
				Sprite sprite = ((i != slots.Length - 1) ? ((slot.bar.currValue == 0f) ? frontEmpty : frontFilled) : ((slot.bar.currValue == 0f) ? frontEmptyWater : frontFilledWater));
				Sprite sprite2 = ((i != slots.Length - 1) ? ((slot.bar.currValue == 0f) ? backEmpty : backFilled) : ((slot.bar.currValue == 0f) ? backEmptyWater : backFilledWater));
				if (slot.front.sprite != sprite)
				{
					slot.front.sprite = sprite;
				}
				if (slot.back.sprite != sprite2)
				{
					slot.back.sprite = sprite2;
				}
				slot.label.text = GetName(slotName);
				lastSlotIds[i] = slotName;
			}
			if (lastSelectedAmmoIndex != selectedAmmoIdx)
			{
				slots[i].anim.SetBool(animSelectedId, selectedAmmoIdx == i);
				if (selectedAmmoIdx == i)
				{
					selected.transform.SetParent(slot.bar.transform);
					selected.transform.localPosition = Vector3.zero;
					selected.transform.localScale = Vector3.one;
					selected.transform.SetSiblingIndex(0);
				}
			}
		}
		double remainingWaterIsMagicMins = player.Ammo.GetRemainingWaterIsMagicMins();
		if (remainingWaterIsMagicMins > 0.0)
		{
			liquidValueTimer.text = timeDir.FormatTime((int)Math.Floor(remainingWaterIsMagicMins));
			liquidFXObj.SetActive(value: true);
			liquidValueText.gameObject.SetActive(value: false);
			liquidValueTimer.gameObject.SetActive(value: true);
		}
		else
		{
			liquidValueText.gameObject.SetActive(value: true);
			liquidValueTimer.gameObject.SetActive(value: false);
			liquidFXObj.SetActive(value: false);
		}
		lastUsableSlotCount = usableSlotCount;
		lastSelectedAmmoIndex = selectedAmmoIdx;
	}

	private void OnSlimeAppearanceChanged(SlimeDefinition definition, SlimeAppearance appearance)
	{
		for (int i = 0; i < player.Ammo.GetUsableSlotCount(); i++)
		{
			Slot slot = slots[i];
			Identifiable.Id slotName = player.Ammo.GetSlotName(i);
			if (slotName == definition.IdentifiableId)
			{
				slot.icon.sprite = GetCurrentIcon(slotName);
				slot.bar.barColor = GetCurrentColor(slotName);
			}
		}
	}

	private void ToggleSlotUsability(Slot slot, int slotIndex, int usableSlotCount)
	{
		GameObject gameObject = slot.label.transform.parent.gameObject;
		if (slotIndex >= usableSlotCount)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
		else if (!gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
		}
	}

	public void SpawnAndPlayFX(GameObject prefab, int index, int count)
	{
		GameObject gameObject = SRBehaviour.SpawnAndPlayFX(prefab, slots[index].anim.gameObject);
		gameObject.GetComponent<ParticleSystem>().emission.SetBursts(new ParticleSystem.Burst[1]
		{
			new ParticleSystem.Burst(0f, count)
		});
		Sprite currentIcon = GetCurrentIcon(player.Ammo.GetSlotName(index));
		global::UiParticles.UiParticles component = gameObject.GetComponent<global::UiParticles.UiParticles>();
		component.materialForRendering.mainTexture = currentIcon.texture;
		component.SetMaterialDirty();
	}

	private Sprite GetCurrentIcon(Identifiable.Id id)
	{
		if (Identifiable.IsSlime(id))
		{
			return slimeAppearanceDirector.GetCurrentSlimeIcon(id);
		}
		if (id != 0)
		{
			return lookupDir.GetIcon(id);
		}
		return null;
	}

	private Color GetCurrentColor(Identifiable.Id id)
	{
		if (Identifiable.IsSlime(id))
		{
			return slimeAppearanceDirector.GetChosenSlimeAppearance(id).ColorPalette.Ammo;
		}
		if (id != 0)
		{
			return lookupDir.GetColor(id);
		}
		return Color.clear;
	}
}
