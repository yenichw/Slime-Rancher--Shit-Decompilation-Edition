using UnityEngine;

public class DeluxeGardenUpgrader : PlotUpgrader
{
	public GameObject deluxeStuff;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		if (upgrade == LandPlot.Upgrade.DELUXE_GARDEN && deluxeStuff != null && !deluxeStuff.activeSelf)
		{
			deluxeStuff.SetActive(value: true);
			Identifiable.Id attachedCropId = GetComponent<LandPlot>().GetAttachedCropId();
			if (attachedCropId != 0)
			{
				SpawnResource componentInChildren = GetComponentInChildren<SpawnResource>();
				Destroyer.Destroy(componentInChildren.gameObject, "DeluxeGardenUpgrader.Apply");
				GetComponentInChildren<GardenCatcher>().Plant(attachedCropId, isReplacement: true).GetComponent<SpawnResource>().InitAsReplacement(componentInChildren);
			}
		}
	}
}
