using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchTarrNodeModel : IdHandlerModel
	{
		public double activationTime;

		public void Push(GlitchTarrNodeV01 persistence)
		{
			activationTime = persistence.activationTime;
		}

		public GlitchTarrNodeV01 Pull()
		{
			return new GlitchTarrNodeV01
			{
				activationTime = activationTime
			};
		}
	}
}
