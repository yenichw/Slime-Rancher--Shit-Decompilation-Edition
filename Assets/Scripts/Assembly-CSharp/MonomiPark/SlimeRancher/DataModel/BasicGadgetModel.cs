using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class BasicGadgetModel : GadgetModel
	{
		public BasicGadgetModel(Gadget.Id gadgetId, string siteId, Transform transform)
			: base(gadgetId, siteId, transform)
		{
		}
	}
}
