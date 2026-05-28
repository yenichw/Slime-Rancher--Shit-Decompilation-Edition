public class GardenDroneSubnetwork : PathingNetwork
{
	private const float MAX_CONNECTION_DIST = 10f;

	private DronePather pather = new DronePather(10f);

	public override Pather Pather => pather;
}
