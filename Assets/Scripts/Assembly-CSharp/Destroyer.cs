using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class Destroyer
{
	public class Metadata
	{
		public string objectName;

		public string source;

		public int frame;

		public override string ToString()
		{
			return $"{typeof(Metadata)} [object={objectName}, source={source}, frame={frame}]";
		}
	}

	private static Dictionary<int, List<Action<Metadata>>> monitorsDict = new Dictionary<int, List<Action<Metadata>>>();

	public static void Monitor(UnityEngine.Object instance, Action<Metadata> action)
	{
		if (!monitorsDict.TryGetValue(instance.GetInstanceID(), out var value))
		{
			monitorsDict.Add(instance.GetInstanceID(), value = new List<Action<Metadata>>());
		}
		value.Add(action);
	}

	public static void DestroyActor(GameObject actorObj, string source, bool okIfNonActor = false)
	{
		if (actorObj.GetComponent<Identifiable>() != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.DestroyActorModel(actorObj);
			Destroy(actorObj, 0f, source, asActorOk: true);
		}
		else
		{
			Destroy(actorObj, 0f, source);
		}
	}

	public static void DestroyGadget(string siteId, GameObject gadgetObj, string source)
	{
		Destroy(gadgetObj, 0f, source, asActorOk: false, asGadgetOk: true);
		SRSingleton<SceneContext>.Instance.GameModel.DestroyGadgetModel(siteId, gadgetObj);
	}

	public static void Destroy(UnityEngine.Object instance, string source)
	{
		Destroy(instance, 0f, source);
	}

	public static void Destroy(UnityEngine.Object instance, float t, string source, bool asActorOk = false, bool asGadgetOk = false)
	{
		if (instance != null)
		{
			Destroy(instance, t, new Metadata
			{
				objectName = instance.name,
				source = source,
				frame = Time.frameCount
			});
		}
	}

	private static void Destroy(UnityEngine.Object instance, float t, Metadata metadata)
	{
		if (instance is GameObject)
		{
			DOTween.Kill(((GameObject)instance).transform);
		}
		if (monitorsDict.TryGetValue(instance.GetInstanceID(), out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i](metadata);
			}
			monitorsDict.Remove(instance.GetInstanceID());
		}
		UnityEngine.Object.Destroy(instance, t);
	}
}
