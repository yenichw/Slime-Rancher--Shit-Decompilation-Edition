using UnityEngine;

public class PooledSlimeAppearanceObjectProvider : SlimeAppearanceObjectProvider
{
	private ObjectPool pool;

	public PooledSlimeAppearanceObjectProvider(ObjectPool poolInstance)
	{
		pool = poolInstance;
	}

	public SlimeAppearanceObject Get(SlimeAppearanceObject appearanceObjectPrefab, GameObject targetParent)
	{
		return pool.Spawn(appearanceObjectPrefab, targetParent.transform, appearanceObjectPrefab.transform.position, appearanceObjectPrefab.transform.rotation);
	}

	public void Put(SlimeAppearanceObject appearanceObjectPrefab, SlimeAppearanceObject appearanceObject)
	{
		pool.Recycle(appearanceObject);
	}
}
