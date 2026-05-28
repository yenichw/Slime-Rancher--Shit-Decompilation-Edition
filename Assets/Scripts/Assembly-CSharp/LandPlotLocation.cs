using UnityEngine;

public class LandPlotLocation : IdHandler
{
	public void Awake()
	{
		SRSingleton<SceneContext>.Instance.GameModel.RegisterLandPlot(base.id, base.gameObject);
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.UnregisterLandPlot(base.id);
		}
	}

	protected override string IdPrefix()
	{
		return "plot";
	}

	public GameObject Replace(LandPlot oldLandPlot, GameObject replacementPrefab)
	{
		GameObject obj = Object.Instantiate(replacementPrefab, oldLandPlot.transform.parent, worldPositionStays: false);
		obj.transform.position = oldLandPlot.transform.position;
		obj.transform.rotation = oldLandPlot.transform.rotation;
		Destroyer.Destroy(oldLandPlot.gameObject, "LandPlotUI.Replace");
		oldLandPlot.transform.parent = null;
		SRSingleton<SceneContext>.Instance.GameModel.UnregisterLandPlot(base.id);
		SRSingleton<SceneContext>.Instance.GameModel.RegisterLandPlot(base.id, base.gameObject);
		return obj;
	}
}
