using UnityEngine;

public class StorageUpgrader : PlotUpgrader
{
	public GameObject storageAdd2;

	public GameObject storageAdd3;

	public GameObject storageAdd4;

	public GameObject storageOnly1;

	public GameObject storageOnly2;

	public GameObject storageOnly3And4;

	public override void Apply(LandPlot.Upgrade upgrade)
	{
		switch (upgrade)
		{
		case LandPlot.Upgrade.STORAGE2:
			storageAdd2.SetActive(value: true);
			storageOnly1.SetActive(value: false);
			storageOnly2.SetActive(value: true);
			storageOnly3And4.SetActive(value: false);
			break;
		case LandPlot.Upgrade.STORAGE3:
			storageAdd3.SetActive(value: true);
			storageOnly1.SetActive(value: false);
			storageOnly2.SetActive(value: false);
			storageOnly3And4.SetActive(value: true);
			break;
		case LandPlot.Upgrade.STORAGE4:
			storageAdd4.SetActive(value: true);
			storageOnly1.SetActive(value: false);
			storageOnly2.SetActive(value: false);
			storageOnly3And4.SetActive(value: true);
			break;
		}
	}
}
