using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class ProduceModel : ResourceModel
	{
		public ResourceCycle.State state = ResourceCycle.State.EDIBLE;

		public double progressTime = double.NaN;

		public ProduceModel(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, Transform transform)
			: base(actorId, ident, regionSetId, transform)
		{
		}

		public override bool IsEdible()
		{
			if (base.IsEdible())
			{
				return state == ResourceCycle.State.EDIBLE;
			}
			return false;
		}

		public void Push(ResourceCycle.State state, double progressTime)
		{
			this.state = state;
			this.progressTime = progressTime;
		}

		public void Pull(out ResourceCycle.State state, out double progressTime)
		{
			state = this.state;
			progressTime = this.progressTime;
		}
	}
}
