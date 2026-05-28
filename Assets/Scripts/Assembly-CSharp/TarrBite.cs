using Assets.Script.Util.Extensions;
using UnityEngine;

public class TarrBite : MonoBehaviour
{
	private BodyMeshMarker[] bodyMeshes;

	private BiteMeshMarker[] biteMeshes;

	private BiteEventAggregator aggregator;

	private SlimeAppearanceApplicator appearanceApplicator;

	public void Awake()
	{
		aggregator = base.gameObject.GetRequiredComponentInChildren<BiteEventAggregator>();
		appearanceApplicator = base.gameObject.GetRequiredComponentInChildren<SlimeAppearanceApplicator>();
	}

	public void Start()
	{
		aggregator.OnEnableBite += ShowBite;
		aggregator.OnDisableBite += HideBite;
		WireBodyAndBiteComponents();
		appearanceApplicator.OnAppearanceChanged += OnAppearanceChanged;
	}

	private void OnAppearanceChanged(SlimeAppearance appearance)
	{
		WireBodyAndBiteComponents();
	}

	private void WireBodyAndBiteComponents()
	{
		bodyMeshes = GetComponentsInChildren<BodyMeshMarker>();
		biteMeshes = GetComponentsInChildren<BiteMeshMarker>();
		if (aggregator.IsBiteAnimationStateActive())
		{
			ShowBite();
		}
		else
		{
			HideBite();
		}
	}

	private void ShowBite()
	{
		BodyMeshMarker[] array = bodyMeshes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		BiteMeshMarker[] array2 = biteMeshes;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value: true);
		}
	}

	private void HideBite()
	{
		BodyMeshMarker[] array = bodyMeshes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
		BiteMeshMarker[] array2 = biteMeshes;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value: false);
		}
	}

	public void OnDestroy()
	{
		if (aggregator != null)
		{
			aggregator.OnEnableBite -= ShowBite;
			aggregator.OnDisableBite -= HideBite;
		}
		if (appearanceApplicator != null)
		{
			appearanceApplicator.OnAppearanceChanged -= OnAppearanceChanged;
		}
	}
}
