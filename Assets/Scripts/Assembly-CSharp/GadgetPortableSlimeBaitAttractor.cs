using System.Linq;
using UnityEngine;

public class GadgetPortableSlimeBaitAttractor : Attractor
{
	private Identifiable.Id bait;

	public void Awake()
	{
		switch (GetComponentInParent<Gadget>().id)
		{
		case Gadget.Id.PORTABLE_SLIME_BAIT_FRUIT:
			bait = Identifiable.FRUIT_CLASS.First();
			return;
		case Gadget.Id.PORTABLE_SLIME_BAIT_VEGGIE:
			bait = Identifiable.VEGGIE_CLASS.First();
			return;
		case Gadget.Id.PORTABLE_SLIME_BAIT_MEAT:
			bait = Identifiable.MEAT_CLASS.First();
			return;
		}
		Log.Error("Failed to get bait type for GadgetPortableSlimeBaitAttractor.", "gadget", GetComponentInParent<Gadget>().id);
	}

	public override float AweFactor(GameObject slime)
	{
		SlimeEat component = slime.GetComponent<SlimeEat>();
		return (component != null && component.enabled && component.DoesEat(bait)) ? 1 : 0;
	}

	public override bool CauseMoveTowards()
	{
		return true;
	}
}
