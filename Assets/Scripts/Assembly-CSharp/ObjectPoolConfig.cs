using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ObjectPoolConfig")]
public class ObjectPoolConfig : ScriptableObject
{
	public enum StartupPoolMode
	{
		Awake = 0,
		Start = 1,
		CallManually = 2
	}

	[Serializable]
	public class StartupPool
	{
		public int size;

		public GameObject prefab;

		public int maxSize;

		public bool doesNotSelfDestruct;
	}

	public StartupPool[] startupPools;

	public StartupPoolMode startupPoolMode;

	public bool loggingEnabled;

	public IEnumerable<string> CheckPooledConfiguration()
	{
		for (int ii = 0; ii < startupPools.Length; ii++)
		{
			StartupPool startupPool = startupPools[ii];
			if (startupPool == null)
			{
				yield return string.Format("Pool {1}[{0}] is null.", ii, base.name);
				continue;
			}
			if (startupPool.prefab == null)
			{
				yield return string.Format("Pool {1}[{0}] has a null prefab.", ii, base.name);
				continue;
			}
			string text = CheckForPooledParticleFX(startupPool.prefab, ii, !startupPool.doesNotSelfDestruct);
			if (!string.IsNullOrEmpty(text))
			{
				yield return text;
			}
		}
	}

	private string CheckForPooledParticleFX(GameObject prefab, int index, bool shouldAutoDestruct)
	{
		string text = string.Format("Pool {2}[{0}] ({1}) ", index, prefab.name, base.name);
		ParticleSystem particleSystem = prefab.GetComponent<ParticleSystem>();
		bool flag = false;
		if (particleSystem == null)
		{
			particleSystem = prefab.GetComponentInChildren<ParticleSystem>();
			if (particleSystem == null)
			{
				return null;
			}
			flag = true;
			text += "child particle system ";
		}
		CFX_AutoDestructShuriken component = particleSystem.gameObject.GetComponent<CFX_AutoDestructShuriken>();
		if (shouldAutoDestruct)
		{
			if (component == null)
			{
				return string.Format(text + "does not have a CFX_AutoDestructShuriken script.");
			}
			if (!component.RecycleOnCompletion)
			{
				return string.Format(text + "is not set to be recycled.");
			}
			if (flag && !component.RecycleParent)
			{
				return string.Format(text + "is not set to have its parent recycled.");
			}
		}
		else if (component != null)
		{
			return string.Format(text + "is not supposed to have a CFX_AutoDestructShuriken script.");
		}
		return null;
	}
}
