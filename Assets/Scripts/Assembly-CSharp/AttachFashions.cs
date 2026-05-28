using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class AttachFashions : SRBehaviour, ActorModel.Participant, GadgetModel.Participant, GordoModel.Participant
{
	private class FashionEntry
	{
		public Identifiable.Id id;

		public GameObject gameObj;

		public FashionEntry(Identifiable.Id id, GameObject gameObj)
		{
			this.id = id;
			this.gameObj = gameObj;
		}
	}

	[Tooltip("If true, changes the root attachment path based off the gordo structure. (slimes only)")]
	public bool gordoMode;

	[Tooltip("Transform to attach front fashions to. (not used by slimes)")]
	public Transform attachmentFront;

	private Dictionary<Fashion.Slot, FashionEntry> slotted = new Dictionary<Fashion.Slot, FashionEntry>();

	private SlimeModel slimeModel;

	private SnareModel snareModel;

	private DroneModel droneModel;

	private GordoModel gordoModel;

	private AnimalModel animalModel;

	private SlimeAppearanceApplicator slimeAppearanceApplicator;

	public void Awake()
	{
		if (!gordoMode && Identifiable.IsSlime(Identifiable.GetId(base.gameObject)))
		{
			slimeAppearanceApplicator = GetRequiredComponent<SlimeAppearanceApplicator>();
		}
	}

	public void SetModel(ActorModel model)
	{
		slimeModel = model as SlimeModel;
		if (slimeModel != null)
		{
			SetFashions(new List<Identifiable.Id>(slimeModel.fashions));
		}
		animalModel = model as AnimalModel;
		if (animalModel != null)
		{
			SetFashions(new List<Identifiable.Id>(animalModel.fashions));
		}
	}

	public void InitModel(ActorModel model)
	{
	}

	public void SetModel(GadgetModel model)
	{
		if (model is SnareModel)
		{
			snareModel = (SnareModel)model;
			SetFashions(new List<Identifiable.Id>(snareModel.fashions));
		}
		else if (model is DroneModel)
		{
			droneModel = (DroneModel)model;
			SetFashions(new List<Identifiable.Id>(droneModel.fashions));
		}
		else
		{
			Log.Error("Unknown type of gadget model for fashions");
		}
	}

	public void InitModel(GadgetModel model)
	{
	}

	public void SetModel(GordoModel model)
	{
		gordoModel = model;
		SetFashions(new List<Identifiable.Id>(gordoModel.fashions));
	}

	public void InitModel(GordoModel model)
	{
	}

	public void OnResetEatenCount()
	{
	}

	public void Attach(Fashion fashion, bool skipFX = false)
	{
		if (slotted.ContainsKey(fashion.slot))
		{
			if (slimeModel != null)
			{
				slimeModel.fashions.Remove(slotted[fashion.slot].id);
			}
			else if (animalModel != null)
			{
				animalModel.fashions.Remove(slotted[fashion.slot].id);
			}
			else if (snareModel != null)
			{
				snareModel.fashions.Remove(slotted[fashion.slot].id);
			}
			else if (droneModel != null)
			{
				droneModel.fashions.Remove(slotted[fashion.slot].id);
			}
			else if (gordoModel != null)
			{
				gordoModel.fashions.Remove(slotted[fashion.slot].id);
			}
			Destroyer.Destroy(slotted[fashion.slot].gameObj, "AttachFashions.Attach");
		}
		Identifiable component = fashion.GetComponent<Identifiable>();
		Vector3 parentOffset = GetParentOffset(component.id);
		GameObject gameObject = Object.Instantiate(fashion.attachPrefab, parentOffset, Quaternion.identity);
		Transform parentForSlot = GetParentForSlot(fashion.slot, component.id);
		gameObject.transform.SetParent(parentForSlot, worldPositionStays: false);
		slotted[fashion.slot] = new FashionEntry(component.id, gameObject);
		if (slimeModel != null)
		{
			slimeModel.fashions.Add(component.id);
		}
		else if (animalModel != null)
		{
			animalModel.fashions.Add(component.id);
		}
		else if (snareModel != null)
		{
			snareModel.fashions.Add(component.id);
		}
		else if (droneModel != null)
		{
			droneModel.fashions.Add(component.id);
		}
		else if (gordoModel != null)
		{
			gordoModel.fashions.Add(component.id);
		}
		if (!skipFX && fashion.attachFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(fashion.attachFX, gameObject.transform.position, gameObject.transform.rotation);
		}
	}

	public void DetachAll(FashionRemover remover)
	{
		foreach (FashionEntry value in slotted.Values)
		{
			Destroyer.Destroy(value.gameObj, "AttachFashions.DetachAll");
		}
		if (remover.removeFX != null)
		{
			SRBehaviour.SpawnAndPlayFX(remover.removeFX, remover.transform.position, remover.transform.rotation);
		}
		slotted.Clear();
		if (slimeModel != null)
		{
			slimeModel.fashions.Clear();
		}
		else if (animalModel != null)
		{
			animalModel.fashions.Clear();
		}
		else if (snareModel != null)
		{
			snareModel.fashions.Clear();
		}
		else if (droneModel != null)
		{
			droneModel.fashions.Clear();
		}
		else if (gordoModel != null)
		{
			gordoModel.fashions.Clear();
		}
	}

	private Vector3 GetParentOffset(Identifiable.Id id)
	{
		if (Identifiable.MEAT_CLASS.Contains(Identifiable.GetId(base.gameObject)))
		{
			switch (id)
			{
			case Identifiable.Id.CUTE_FASHION:
				return new Vector3(0.049f, 0.044f, -0.179f);
			case Identifiable.Id.GOOGLY_FASHION:
				return new Vector3(0f, 0f, -0.09f);
			case Identifiable.Id.HANDLEBAR_FASHION:
				return new Vector3(0f, -0.127f, 0f);
			case Identifiable.Id.HEROIC_FASHION:
				return new Vector3(0f, 0f, -0.16f);
			case Identifiable.Id.PIRATEY_FASHION:
				return new Vector3(0f, 0f, -0.16f);
			case Identifiable.Id.ROYAL_FASHION:
				return new Vector3(-0.033f, 0f, -0.101f);
			case Identifiable.Id.SCIFI_FASHION:
				return new Vector3(0f, 0f, -0.16f);
			}
		}
		return Vector3.zero;
	}

	private Transform GetParentForSlot(Fashion.Slot slot, Identifiable.Id id)
	{
		if (Identifiable.MEAT_CLASS.Contains(Identifiable.GetId(base.gameObject)))
		{
			return attachmentFront;
		}
		if (slimeAppearanceApplicator != null)
		{
			return slimeAppearanceApplicator.GetFashionParent(slot);
		}
		string text = (gordoMode ? "Vibrating" : "prefab_slimeBase");
		switch (slot)
		{
		case Fashion.Slot.FRONT:
			return base.transform.Find(text + "/bone_root/bone_slime/bone_core/bone_jiggle_bac/bone_skin_bac");
		case Fashion.Slot.TOP:
			return base.transform.Find(text + "/bone_root/bone_slime/bone_core/bone_jiggle_top/bone_skin_top");
		default:
			Log.Error("Unhandled fashion slot", "slot", slot);
			return null;
		}
	}

	public List<Identifiable.Id> GetAllFashions()
	{
		List<Identifiable.Id> list = new List<Identifiable.Id>();
		foreach (FashionEntry value in slotted.Values)
		{
			list.Add(value.id);
		}
		return list;
	}

	public bool HasFashion(Identifiable.Id id)
	{
		return slotted.Values.Any((FashionEntry e) => e.id == id);
	}

	public void SetFashions(List<Identifiable.Id> ids)
	{
		if (slimeModel != null)
		{
			slimeModel.fashions.Clear();
		}
		else if (animalModel != null)
		{
			animalModel.fashions.Clear();
		}
		else if (snareModel != null)
		{
			snareModel.fashions.Clear();
		}
		else if (droneModel != null)
		{
			droneModel.fashions.Clear();
		}
		else if (gordoModel != null)
		{
			gordoModel.fashions.Clear();
		}
		LookupDirector lookupDirector = SRSingleton<GameContext>.Instance.LookupDirector;
		foreach (Identifiable.Id id in ids)
		{
			Fashion component = lookupDirector.GetPrefab(id).GetComponent<Fashion>();
			if (component != null)
			{
				Attach(component, skipFX: true);
			}
		}
	}
}
