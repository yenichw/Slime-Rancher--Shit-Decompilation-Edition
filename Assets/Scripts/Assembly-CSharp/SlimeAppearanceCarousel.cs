using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class SlimeAppearanceCarousel : SRBehaviour
{
	public delegate void OnSlimeAppearanceSelectedDelegate(SlimeDefinition slime, SlimeAppearance appearance);

	public Camera cam;

	public Transform root;

	public SlimeAppearanceApplicator appearancePrefab;

	public GameObject appearanceSpritePrefab;

	public float spacing = 1.5f;

	public float transitionTime = 0.25f;

	public SlimeAppearanceDirector slimeAppearanceDirector;

	public GameObject shadowPrefab;

	public Transform spotlight;

	public GameObject selectFx;

	public float unselectedMoveBack = 1f;

	public float unselectedXScaling = 1.2f;

	public float jumpAmount = 0.25f;

	public int maxAppearancesToShow = 2;

	private const string unscaledTimeKeyword = "_UNSCALEDTIME_ON";

	private readonly Type[] blacklistedObjectTypes = new Type[2]
	{
		typeof(RadExpandMarker),
		typeof(TrailRenderer)
	};

	private List<GameObject> currentSlimeAppearancePreviews = new List<GameObject>();

	private SlimeDefinition currentSlime;

	private SlimeAppearance[] currentAppearances;

	private int unscaledTimePropertyId;

	public event OnSlimeAppearanceSelectedDelegate onSlimeAppearanceConfirmed = delegate
	{
	};

	private void Awake()
	{
		unscaledTimePropertyId = Shader.PropertyToID("_UnscaledTime");
	}

	private IEnumerator ResetExpressionAfterTime(SlimeAppearanceApplicator appearanceApplicator)
	{
		yield return new WaitForSecondsRealtime(0.5f);
		if (appearanceApplicator != null && appearanceApplicator.gameObject != null && appearanceApplicator.gameObject.activeInHierarchy)
		{
			appearanceApplicator.SetExpression(SlimeFace.SlimeExpression.Happy);
		}
	}

	public void ShowSlime(SlimeDefinition slime)
	{
		currentSlime = slime;
		foreach (GameObject currentSlimeAppearancePreview in currentSlimeAppearancePreviews)
		{
			UnityEngine.Object.Destroy(currentSlimeAppearancePreview);
		}
		currentSlimeAppearancePreviews.Clear();
		currentAppearances = slime.Appearances.ToArray();
		SlimeAppearance chosenSlimeAppearance = slimeAppearanceDirector.GetChosenSlimeAppearance(slime);
		for (int i = 0; i < Mathf.Min(maxAppearancesToShow, currentAppearances.Length); i++)
		{
			SlimeAppearance slimeAppearance = currentAppearances[i];
			GameObject gameObject = CreateAppearancePreview(slime, slimeAppearance, slimeAppearance == chosenSlimeAppearance);
			gameObject.transform.localPosition = new Vector3((float)i * spacing - spacing / 2f, 0f, 0f);
			gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
			if (slimeAppearance != chosenSlimeAppearance)
			{
				gameObject.transform.localPosition = GetUnfocusedPosition(gameObject.transform.position);
			}
			else
			{
				spotlight.localPosition = new Vector3((float)i * spacing - spacing / 2f, spotlight.localPosition.y, spotlight.localPosition.z);
			}
		}
	}

	private Vector3 GetUnfocusedPosition(Vector3 focusedPosition)
	{
		return new Vector3(focusedPosition.x * unselectedXScaling, 0f, unselectedMoveBack);
	}

	public void ConfirmSlimeAppearance(int index)
	{
		if (slimeAppearanceDirector.GetChosenSlimeAppearance(currentSlime) == currentAppearances[index])
		{
			return;
		}
		this.onSlimeAppearanceConfirmed(currentSlime, currentAppearances[index]);
		GameObject gameObject = currentSlimeAppearancePreviews[index];
		SpriteRenderer componentInChildren = gameObject.GetComponentInChildren<SpriteRenderer>();
		SlimeAppearanceApplicator component = gameObject.GetComponent<SlimeAppearanceApplicator>();
		if (component != null)
		{
			component.SetExpression(SlimeFace.SlimeExpression.Elated);
			StartCoroutine(ResetExpressionAfterTime(component));
		}
		else if (componentInChildren != null)
		{
			componentInChildren.color = Color.white;
		}
		if (selectFx != null)
		{
			SRBehaviour.SpawnAndPlayFX(selectFx, gameObject);
		}
		for (int i = 0; i < currentSlimeAppearancePreviews.Count; i++)
		{
			if (i != index)
			{
				GameObject obj = currentSlimeAppearancePreviews[i];
				SlimeAppearanceApplicator component2 = obj.GetComponent<SlimeAppearanceApplicator>();
				SpriteRenderer componentInChildren2 = obj.GetComponentInChildren<SpriteRenderer>();
				if (component2 == null && componentInChildren2 != null)
				{
					componentInChildren2.color = Color.grey;
				}
				obj.transform.DOLocalJump(GetUnfocusedPosition(new Vector3((float)i * spacing - spacing / 2f, 0f, 0f)), jumpAmount, 1, transitionTime).SetUpdate(isIndependentUpdate: true);
			}
		}
		currentSlimeAppearancePreviews[index].transform.DOLocalJump(new Vector3((float)index * spacing - spacing / 2f, 0f, 0f), jumpAmount, 1, transitionTime).SetUpdate(isIndependentUpdate: true);
		spotlight.transform.DOLocalMove(new Vector3((float)index * spacing - spacing / 2f, spotlight.localPosition.y, spotlight.localPosition.z), transitionTime).SetUpdate(isIndependentUpdate: true);
	}

	private bool UseSpriteForSlime(SlimeDefinition slime)
	{
		return slime.IdentifiableId == Identifiable.Id.SABER_SLIME;
	}

	private GameObject CreateAppearancePreview(SlimeDefinition slime, SlimeAppearance appearance, bool isSelected)
	{
		GameObject gameObject;
		if (UseSpriteForSlime(slime))
		{
			gameObject = UnityEngine.Object.Instantiate(appearanceSpritePrefab, root);
			SpriteRenderer componentInChildren = gameObject.GetComponentInChildren<SpriteRenderer>();
			SetLayerInChildren(gameObject);
			currentSlimeAppearancePreviews.Add(gameObject);
			componentInChildren.sprite = appearance.Icon ?? slimeAppearanceDirector.missingIcon;
			componentInChildren.color = (isSelected ? Color.white : Color.grey);
		}
		else
		{
			SlimeAppearanceApplicator slimeAppearanceApplicator = UnityEngine.Object.Instantiate(appearancePrefab, root);
			slimeAppearanceApplicator.Appearance = appearance;
			slimeAppearanceApplicator.ApplyAppearance();
			currentSlimeAppearancePreviews.Add(slimeAppearanceApplicator.gameObject);
			SetLayerInChildren(slimeAppearanceApplicator.gameObject);
			Type[] array = blacklistedObjectTypes;
			foreach (Type t in array)
			{
				Component[] componentsInChildren = slimeAppearanceApplicator.GetComponentsInChildren(t);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].gameObject.SetActive(value: false);
				}
			}
			EnableBasedOnGrounded[] componentsInChildren2 = slimeAppearanceApplicator.GetComponentsInChildren<EnableBasedOnGrounded>();
			foreach (EnableBasedOnGrounded enableBasedOnGrounded in componentsInChildren2)
			{
				if (enableBasedOnGrounded.enableOnGrounded)
				{
					enableBasedOnGrounded.gameObject.SetActive(value: false);
				}
			}
			DeactivateOnHeld[] componentsInChildren3 = slimeAppearanceApplicator.GetComponentsInChildren<DeactivateOnHeld>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
			Animator[] componentsInChildren4 = slimeAppearanceApplicator.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren4.Length; i++)
			{
				componentsInChildren4[i].updateMode = AnimatorUpdateMode.UnscaledTime;
			}
			ParticleSystem[] componentsInChildren5 = slimeAppearanceApplicator.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren5.Length; i++)
			{
				ParticleSystem.MainModule main = componentsInChildren5[i].main;
				main.useUnscaledTime = true;
			}
			Renderer[] componentsInChildren6 = slimeAppearanceApplicator.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren6.Length; i++)
			{
				Material[] materials = componentsInChildren6[i].materials;
				foreach (Material obj in materials)
				{
					obj.SetInt(unscaledTimePropertyId, 1);
					obj.EnableKeyword("_UNSCALEDTIME_ON");
				}
			}
			slimeAppearanceApplicator.GetComponentInChildren<RubberBoneEffect>().unscaledTime = true;
			gameObject = slimeAppearanceApplicator.gameObject;
		}
		UnityEngine.Object.Instantiate(shadowPrefab, gameObject.transform);
		return gameObject;
	}

	private void SetLayerInChildren(GameObject gameObject)
	{
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = base.gameObject.layer;
		}
	}
}
