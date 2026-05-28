using MonomiPark.SlimeRancher.DataModel;

public class LinkedSiloStorage : SiloStorage, Gadget.LinkDestroyer, GadgetModel.Participant
{
	private Gadget.Id gadgetId;

	private new WarpDepotModel model;

	private LinkedSiloStorage link;

	public override void Awake()
	{
		gadgetId = GetComponentInParent<Gadget>().id;
		foreach (GadgetSiteModel value in SRSingleton<SceneContext>.Instance.GameModel.AllGadgetSites().Values)
		{
			if (value.HasAttached() && value.attached.ident == gadgetId)
			{
				link = value.attached.transform.GetComponentInChildren<LinkedSiloStorage>();
				link.link = this;
				break;
			}
		}
		base.Awake();
	}

	public void InitModel(GadgetModel baseModel)
	{
		WarpDepotModel warpDepotModel = (WarpDepotModel)baseModel;
		base.LocalAmmo.InitModel(warpDepotModel.ammo);
		warpDepotModel.isPrimary = true;
	}

	public void SetModel(GadgetModel baseModel)
	{
		model = (WarpDepotModel)baseModel;
		base.LocalAmmo.SetModel(model.ammo);
		if (!model.isPrimary && link == null)
		{
			model.isPrimary = true;
		}
		if (link != null && link.model != null && model.isPrimary == link.model.isPrimary)
		{
			model.isPrimary = !ammo.IsEmpty();
			link.model.isPrimary = !model.isPrimary;
		}
	}

	public override Ammo GetRelevantAmmo()
	{
		if (model.isPrimary)
		{
			return ammo;
		}
		return link.ammo;
	}

	public bool ShouldDestroyPair()
	{
		return link != null;
	}

	public Gadget.LinkDestroyer GetLinked()
	{
		return link;
	}
}
