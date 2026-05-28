using MonomiPark.SlimeRancher.Persist;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class GlitchSlimeModel : SlimeModel
	{
		public float exposureChance;

		public double deathTime;

		public GlitchSlimeModel(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, Transform transform)
			: base(actorId, ident, regionSetId, transform)
		{
		}

		public void Push(GlitchSlimeDataV01 persistence)
		{
			exposureChance = persistence.exposureChance;
			deathTime = persistence.deathTime;
		}

		public GlitchSlimeDataV01 Pull()
		{
			return new GlitchSlimeDataV01
			{
				exposureChance = exposureChance,
				deathTime = deathTime
			};
		}
	}
}
