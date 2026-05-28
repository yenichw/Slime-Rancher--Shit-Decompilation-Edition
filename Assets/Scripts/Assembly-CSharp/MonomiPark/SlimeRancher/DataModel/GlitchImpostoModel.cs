using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchImpostoModel : IdHandlerModel
	{
		public double? deactivateTime;

		public double cooldownTime;

		public void Push(GlitchImpostoV01 persistence)
		{
			deactivateTime = persistence.deactivateTime;
			cooldownTime = persistence.cooldownTime;
		}

		public GlitchImpostoV01 Pull()
		{
			return new GlitchImpostoV01
			{
				deactivateTime = deactivateTime,
				cooldownTime = cooldownTime
			};
		}
	}
}
