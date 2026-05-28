using System.Collections.Generic;

public class DroneProgramSourceFreeRange : DroneProgramSourceDynamic
{
	protected override GardenDroneSubnetwork GetSubnetwork()
	{
		return drone.network.GetContaining(source)?.subnetwork;
	}

	protected override IEnumerable<Orientation> GetTargetOrientations(Identifiable source)
	{
		return GetTargetOrientations_Gather(source.gameObject, new GatherConfig
		{
			distanceVertical = 1.25f
		});
	}
}
