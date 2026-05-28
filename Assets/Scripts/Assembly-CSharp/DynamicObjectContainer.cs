using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class DynamicObjectContainer : SRSingleton<DynamicObjectContainer>
{
	public override void Awake()
	{
		base.Awake();
		Destroyer.Monitor(base.gameObject, delegate(Destroyer.Metadata metadata)
		{
			InvalidOperationException ex = new InvalidOperationException($"DynamicObjectContainer is being destroyed. [metadata={metadata}]");
			Log.Error(ex.ToString());
			SentrySdk.CaptureMessage("DynamicObjectContainer is being destroyed!");
			throw ex;
		});
	}

	private List<GameObject> GetChildren()
	{
		List<GameObject> list = new List<GameObject>();
		Identifiable[] componentsInChildren = GetComponentsInChildren<Identifiable>();
		foreach (Identifiable identifiable in componentsInChildren)
		{
			list.Add(identifiable.gameObject);
		}
		return list;
	}

	public void RegisterDynamicObjectActors()
	{
		List<GameObject> children = GetChildren();
		foreach (GameObject item in children)
		{
			SRSingleton<SceneContext>.Instance.GameModel.RegisterStartingActor(item, RegionRegistry.RegionSetId.HOME);
		}
		foreach (GameObject item2 in children)
		{
			item2.transform.SetParent(null);
		}
	}

	public void DestroyDynamicObjectActors()
	{
		foreach (GameObject child in GetChildren())
		{
			Destroyer.Destroy(child, 0f, "DynamicObjectContainer.Awake", asActorOk: true);
		}
	}
}
