using UnityEngine;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class WarpDepotModel : GadgetModel
	{
		public bool isPrimary;

		public AmmoModel ammo = new AmmoModel();

		public WarpDepotModel(Gadget.Id gadgetId, string siteId, Transform transform)
			: base(gadgetId, siteId, transform)
		{
		}

		public void Push(bool isPrimary, Ammo.Slot[] slots)
		{
			this.isPrimary = isPrimary;
			ammo.Push(slots);
		}

		public void Pull(out bool isPrimary, out Ammo.Slot[] slots)
		{
			isPrimary = this.isPrimary;
			ammo.Pull(out slots);
		}
	}
}
