using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class EchoModel : ActorModel
	{
		public EchoModel(long actorId, Identifiable.Id ident, RegionRegistry.RegionSetId regionSetId, Transform transform)
			: base(actorId, ident, regionSetId, transform)
		{
		}
	}
}
