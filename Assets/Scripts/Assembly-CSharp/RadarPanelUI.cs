using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.UI;

public class RadarPanelUI : SRSingleton<RadarPanelUI>
{
	private class TrackedEntry
	{
		public GameObject obj;

		public Image img;

		public RegionRegistry.RegionSetId setId;

		public TrackedEntry(GameObject obj, Image img, RegionRegistry.RegionSetId setId)
		{
			this.obj = obj;
			this.img = img;
			this.setId = setId;
		}
	}

	private List<TrackedEntry> registered = new List<TrackedEntry>();

	private List<Image> optional = new List<Image>();

	private bool radarVisible;

	private Camera mainCamera;

	private RectTransform rectTransform;

	private RegionRegistry regionReg;

	public override void Awake()
	{
		base.Awake();
		regionReg = SRSingleton<SceneContext>.Instance.RegionRegistry;
		rectTransform = GetComponent<RectTransform>();
		UpdateOptionalActiveness();
	}

	public void Start()
	{
		mainCamera = Camera.main;
	}

	public void Update()
	{
		if (Time.timeScale != 0f && SRInput.Actions.radarToggle.WasPressed)
		{
			radarVisible = !radarVisible;
			AnalyticsUtil.CustomEvent("RadarToggled", new Dictionary<string, object> { { "RadarState", radarVisible } });
			UpdateOptionalActiveness();
		}
	}

	private void UpdateOptionalActiveness()
	{
		foreach (Image item in optional)
		{
			item.gameObject.SetActive(radarVisible);
		}
	}

	public void LateUpdate()
	{
		RegionRegistry.RegionSetId currentRegionSetId = regionReg.GetCurrentRegionSetId();
		foreach (TrackedEntry item in registered)
		{
			if (item.setId == currentRegionSetId)
			{
				Vector3 vector = mainCamera.WorldToViewportPoint(item.obj.transform.position);
				item.img.transform.localPosition = new Vector3(rectTransform.rect.width * (vector.x - 0.5f), rectTransform.rect.height * (vector.y - 0.5f));
				item.img.enabled = vector.z >= 0f;
			}
			else
			{
				item.img.enabled = false;
			}
		}
	}

	public void RegisterTracked(GameObject obj, RegionRegistry.RegionSetId regionSetId, Image img, bool isOptional)
	{
		Image image = Object.Instantiate(img);
		registered.Add(new TrackedEntry(obj, image, regionSetId));
		image.transform.SetParent(base.transform);
		if (isOptional)
		{
			optional.Add(image);
			image.gameObject.SetActive(radarVisible);
		}
	}

	public void UnregisterTracked(GameObject obj)
	{
		TrackedEntry trackedEntry = null;
		foreach (TrackedEntry item in registered)
		{
			if (item.obj == obj)
			{
				trackedEntry = item;
				break;
			}
		}
		if (trackedEntry != null)
		{
			Destroyer.Destroy(trackedEntry.img.gameObject, "RadarPanelUI.UnregisterTracked");
			optional.Remove(trackedEntry.img);
			registered.Remove(trackedEntry);
		}
	}
}
