using System;
using System.Collections.Generic;
using UnityEngine;

public class RanchLargoTypesTracker : MonoBehaviour
{
	private AchievementsDirector achieveDir;

	private Dictionary<Identifiable.Id, int> largoTypes = new Dictionary<Identifiable.Id, int>(Identifiable.idComparer);

	public void Awake()
	{
		CellDirector[] componentsInChildren = GetComponentsInChildren<CellDirector>();
		foreach (CellDirector obj in componentsInChildren)
		{
			obj.onSlimeAdded = (CellDirector.OnSlimeAdded)Delegate.Combine(obj.onSlimeAdded, new CellDirector.OnSlimeAdded(OnSlimeAdded));
			obj.onSlimeRemoved = (CellDirector.OnSlimeRemoved)Delegate.Combine(obj.onSlimeRemoved, new CellDirector.OnSlimeRemoved(OnSlimeRemoved));
		}
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
	}

	public void OnDestroy()
	{
		CellDirector[] componentsInChildren = GetComponentsInChildren<CellDirector>();
		foreach (CellDirector obj in componentsInChildren)
		{
			obj.onSlimeAdded = (CellDirector.OnSlimeAdded)Delegate.Remove(obj.onSlimeAdded, new CellDirector.OnSlimeAdded(OnSlimeAdded));
		}
	}

	public void OnSlimeAdded(Identifiable.Id slimeId)
	{
		if (Identifiable.IsLargo(slimeId))
		{
			if (largoTypes.ContainsKey(slimeId))
			{
				Dictionary<Identifiable.Id, int> dictionary = largoTypes;
				int value = dictionary[slimeId] + 1;
				dictionary[slimeId] = value;
			}
			else
			{
				largoTypes[slimeId] = 1;
			}
			achieveDir.MaybeUpdateMaxStat(AchievementsDirector.IntStat.RANCH_LARGO_TYPES, largoTypes.Count);
		}
	}

	public void OnSlimeRemoved(Identifiable.Id slimeId)
	{
		if (!Identifiable.IsLargo(slimeId))
		{
			return;
		}
		if (largoTypes.ContainsKey(slimeId))
		{
			int num = largoTypes[slimeId] - 1;
			if (num > 0)
			{
				largoTypes[slimeId] = num;
			}
			else
			{
				largoTypes.Remove(slimeId);
			}
		}
		else
		{
			Log.Warning("Tried to remove non-registered largo ID: " + slimeId);
		}
	}
}
