using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public interface PrefabInstantiator
{
	GameObject InstantiateActor(long actorId, Identifiable.Id id, RegionRegistry.RegionSetId regionSetId, Vector3 pos, Vector3 rot, GameModel gameModel);

	GameObject InstantiateGadget(Gadget.Id id, GadgetSiteModel site, GameModel gameModel);

	void InstantiatePlot(LandPlot.Id id, LandPlotModel plotModel, bool expectingPush);
}
