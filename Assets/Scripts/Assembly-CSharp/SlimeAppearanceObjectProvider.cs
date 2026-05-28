using UnityEngine;

public interface SlimeAppearanceObjectProvider
{
	SlimeAppearanceObject Get(SlimeAppearanceObject appearanceObjectPrefab, GameObject targetParent);

	void Put(SlimeAppearanceObject appearanceObjectPrefab, SlimeAppearanceObject appearanceObject);
}
