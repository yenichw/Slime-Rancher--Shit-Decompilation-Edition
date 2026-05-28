using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class PlortModel : ActorModel
	{
		public readonly GameObject gameObject;

		public double destroyTime;

		public PlortModel(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, GameObject gameObject)
			: base(actorId, ident, regionSetId, gameObject.transform)
		{
			this.gameObject = gameObject;
		}

		public override bool IsEdible()
		{
			PlortInvulnerability component = gameObject.GetComponent<PlortInvulnerability>();
			bool flag = component == null || !component.enabled;
			return base.IsEdible() && flag;
		}

		public void Push(double destroyTime)
		{
			this.destroyTime = destroyTime;
		}

		public void Pull(out double destroyTime)
		{
			destroyTime = this.destroyTime;
		}
	}
}
