using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class EchoNetModel : GadgetModel
	{
		public double lastSpawnTime;

		public EchoNetModel(Gadget.Id gadgetId, string siteId, Transform transform)
			: base(gadgetId, siteId, transform)
		{
		}

		public void Push(double lastSpawnTime)
		{
			this.lastSpawnTime = lastSpawnTime;
		}

		public void Pull(out double lastSpawnTime)
		{
			lastSpawnTime = this.lastSpawnTime;
		}
	}
}
