using System;
using UnityEngine;

public static class LineupUtils
{
	public static SlimeAppearanceApplicator GenerateAppearancePreview(SlimeAppearanceApplicator prefab, SlimeDefinition slimeDefinition, SlimeAppearance appearance)
	{
		SlimeAppearanceApplicator slimeAppearanceApplicator = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
		slimeAppearanceApplicator.enabled = false;
		slimeAppearanceApplicator.SlimeDefinition = null;
		slimeAppearanceApplicator.Appearance = appearance;
		try
		{
			slimeAppearanceApplicator.ApplyAppearance();
		}
		catch (Exception ex)
		{
			Log.Error("An issue occurred while trying to apply the appearance: " + appearance.name, ex);
		}
		EnableBasedOnGrounded[] componentsInChildren = slimeAppearanceApplicator.GetComponentsInChildren<EnableBasedOnGrounded>();
		foreach (EnableBasedOnGrounded enableBasedOnGrounded in componentsInChildren)
		{
			enableBasedOnGrounded.gameObject.SetActive(!enableBasedOnGrounded.enableOnGrounded);
		}
		DeactivateOnHeld[] componentsInChildren2 = slimeAppearanceApplicator.GetComponentsInChildren<DeactivateOnHeld>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		NotifyBiteComplete[] componentsInChildren3 = slimeAppearanceApplicator.GetComponentsInChildren<NotifyBiteComplete>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[i]);
		}
		slimeAppearanceApplicator.transform.localScale = new Vector3(slimeDefinition.PrefabScale, slimeDefinition.PrefabScale, slimeDefinition.PrefabScale);
		return slimeAppearanceApplicator;
	}
}
