public class DroneProgramSourceOutsidePlots : DroneProgramSourceDynamic
{
	protected override bool SourcePredicate(DroneNetwork.LandPlotMetadata metadata, Identifiable source)
	{
		if (metadata == null)
		{
			return base.SourcePredicate(metadata, source);
		}
		return false;
	}
}
