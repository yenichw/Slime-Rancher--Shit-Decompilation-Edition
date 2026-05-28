using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class ExtractorModel : GadgetModel
	{
		public int cyclesRemaining;

		public int queuedToProduce;

		public double cycleEndTime;

		public double nextProduceTime;

		public ExtractorModel(Gadget.Id gadgetId, string siteId, Transform transform)
			: base(gadgetId, siteId, transform)
		{
		}

		public void Push(int cyclesRemaining, int queuedToProduce, double cycleEndTime, double nextProduceTime)
		{
			this.cyclesRemaining = cyclesRemaining;
			this.queuedToProduce = queuedToProduce;
			this.cycleEndTime = cycleEndTime;
			this.nextProduceTime = nextProduceTime;
		}

		public void Pull(out int cyclesRemaining, out int queuedToProduce, out double cycleEndTime, out double nextProduceTime)
		{
			cyclesRemaining = this.cyclesRemaining;
			queuedToProduce = this.queuedToProduce;
			cycleEndTime = this.cycleEndTime;
			nextProduceTime = this.nextProduceTime;
		}
	}
}
