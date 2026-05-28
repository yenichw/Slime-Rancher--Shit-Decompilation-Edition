using System.Collections.Generic;
using UnityEngine;

public class FireSlimeIgnition : RegisteredActorBehaviour, Ignitable, LiquidConsumer, RegistryUpdateable, ControllerCollisionListener
{
	private bool isIgnited;

	private GameObject fireFXObj;

	private double reigniteAtTime = double.PositiveInfinity;

	private TimeDirector timeDir;

	private List<LiquidSource> waterSources = new List<LiquidSource>();

	private const float EXTINGUISH_HRS = 0.5f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
	}

	public override void Start()
	{
		base.Start();
		ExtractFire();
		GetComponent<SlimeAppearanceApplicator>().OnAppearanceChanged += delegate
		{
			ExtractFire();
		};
		Ignite(base.gameObject);
	}

	private void ExtractFire()
	{
		FireIndicatorMarker componentInChildren = GetComponentInChildren<FireIndicatorMarker>();
		if (componentInChildren != null)
		{
			fireFXObj = componentInChildren.gameObject;
			fireFXObj.SetActive(isIgnited);
		}
	}

	public void OnCollisionEnter(Collision col)
	{
		if (isIgnited)
		{
			col.gameObject.GetComponent<Ignitable>()?.Ignite(base.gameObject);
		}
	}

	public void OnControllerCollision(GameObject gameObj)
	{
		if (isIgnited)
		{
			gameObj.GetComponent<Ignitable>()?.Ignite(base.gameObject);
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId))
		{
			waterSources.Add(component);
			Extinguish();
		}
	}

	public void OnTriggerExit(Collider col)
	{
		LiquidSource component = col.gameObject.GetComponent<LiquidSource>();
		if (component != null && Identifiable.IsWater(component.liquidId))
		{
			waterSources.Remove(component);
		}
	}

	public void RegistryUpdate()
	{
		if (!isIgnited && timeDir.HasReached(reigniteAtTime))
		{
			Ignite(base.gameObject);
		}
	}

	public void Ignite(GameObject igniter)
	{
		waterSources.RemoveAll((LiquidSource w) => w == null || w.gameObject == null);
		if (waterSources.Count <= 0)
		{
			isIgnited = true;
			if (fireFXObj != null)
			{
				fireFXObj.SetActive(value: true);
			}
		}
	}

	public void Extinguish()
	{
		isIgnited = false;
		if (fireFXObj != null)
		{
			fireFXObj.SetActive(value: false);
		}
		reigniteAtTime = timeDir.HoursFromNow(0.5f);
	}

	public void AddLiquid(Identifiable.Id liquidId, float units)
	{
		if (Identifiable.IsWater(liquidId))
		{
			Extinguish();
		}
	}
}
