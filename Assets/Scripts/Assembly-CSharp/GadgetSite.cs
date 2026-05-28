using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GadgetSite : IdHandler, GadgetSiteModel.Participant
{
	public GameObject placeGadgetUIPrefab;

	private GameObject attached;

	private GadgetDirector gadgetDir;

	private GadgetSiteModel model;

	private static float ROT_SPEED = 360f;

	public void Awake()
	{
		gadgetDir = SRSingleton<SceneContext>.Instance.GadgetDirector;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterGadgetSite(base.id, base.gameObject, this);
	}

	protected override string IdPrefix()
	{
		return "site";
	}

	public void OnDestroy()
	{
		if (!(attached != null))
		{
			return;
		}
		Gadget component = attached.GetComponent<Gadget>();
		if (component != null)
		{
			gadgetDir.DecrementPlacedGadgetCount(component.id);
			if (SRSingleton<SceneContext>.Instance != null)
			{
				SRSingleton<SceneContext>.Instance.GameModel.UnregisterGadgetSite(base.id);
			}
		}
	}

	public void InitModel(GadgetSiteModel model)
	{
	}

	public void SetModel(GadgetSiteModel model)
	{
		this.model = model;
	}

	public void SetAttached(GadgetModel gadgetModel)
	{
		if (gadgetModel == null)
		{
			attached = null;
			return;
		}
		attached = gadgetModel.transform.gameObject;
		Gadget component = attached.GetComponent<Gadget>();
		if (component != null)
		{
			gadgetDir.IncrementPlacedGadgetCount(component.id);
		}
	}

	public virtual void Activate()
	{
		PlaceGadgetUI component = Object.Instantiate(placeGadgetUIPrefab).GetComponent<PlaceGadgetUI>();
		if (component != null)
		{
			component.SetSite(this, model);
		}
	}

	public bool HasAttached()
	{
		return attached != null;
	}

	public Gadget.Id GetAttachedId()
	{
		if (attached == null)
		{
			return Gadget.Id.NONE;
		}
		Gadget component = attached.GetComponent<Gadget>();
		if (!(component == null))
		{
			return component.id;
		}
		return Gadget.Id.NONE;
	}

	public GameObject GetAttached()
	{
		return attached;
	}

	public void DestroyAttached()
	{
		Gadget component = attached.GetComponent<Gadget>();
		if (component != null)
		{
			gadgetDir.DecrementPlacedGadgetCount(component.id);
			component.OnUserDestroyed();
		}
		Destroyer.DestroyGadget(base.id, attached, "GadgetSite.DestroyAttached");
		attached = null;
	}

	public void DestroyAttachedWithPair()
	{
		GadgetSite componentInParent = ((MonoBehaviour)attached.GetComponentInChildren<Gadget.LinkDestroyer>().GetLinked()).GetComponentInParent<GadgetSite>(includeInactive: true);
		DestroyAttached();
		componentInParent.DestroyAttached();
	}

	public void OnRotateCW()
	{
		if (attached != null)
		{
			attached.GetComponent<Gadget>().AddRotation(ROT_SPEED * Time.deltaTime);
		}
	}

	public void OnRotateCCW()
	{
		if (attached != null)
		{
			attached.GetComponent<Gadget>().AddRotation((0f - ROT_SPEED) * Time.deltaTime);
		}
	}

	public void RotateToPlayer()
	{
		if (attached != null)
		{
			Vector3 vector = SRSingleton<SceneContext>.Instance.Player.transform.position - base.transform.position;
			attached.transform.rotation = Quaternion.LookRotation(new Vector3(vector.x, 0f, vector.z), Vector3.up);
		}
	}

	public bool DestroysLinkedPairOnRemoval()
	{
		Gadget component = attached.GetComponent<Gadget>();
		if (component != null)
		{
			return component.DestroysLinkedPairOnRemoval();
		}
		return false;
	}

	public bool DestroysOnRemoval()
	{
		Gadget component = attached.GetComponent<Gadget>();
		if (component != null)
		{
			return component.DestroysOnRemoval();
		}
		return false;
	}

	public bool DestroyingWillDestroyContents()
	{
		if (attached != null)
		{
			LinkedSiloStorage component = attached.GetComponent<LinkedSiloStorage>();
			if (component != null)
			{
				return !component.GetRelevantAmmo().IsEmpty();
			}
			return false;
		}
		return false;
	}
}
