using System.Collections.Generic;
using UnityEngine.UI;

public class TeleporterGadget : TeleportDestination, Gadget.LinkDestroyer
{
	public Image destinationIcon;

	public string linkName;

	private TeleporterGadget linked;

	public override void Awake()
	{
		TeleportSource component = GetComponent<TeleportSource>();
		component.waitForExternalActivation = true;
		destinationIcon.enabled = false;
		List<TeleportDestination> destinations = SRSingleton<SceneContext>.Instance.TeleportNetwork.GetDestinations(linkName);
		if (destinations.Count == 1)
		{
			linked = (TeleporterGadget)destinations[0];
			linked.linked = this;
			teleportDestinationName = string.Format("{0}_{1}", linkName, "linked");
			component.destinationSetName = linked.teleportDestinationName;
			component.waitForExternalActivation = false;
			TeleportSource component2 = linked.GetComponent<TeleportSource>();
			component2.destinationSetName = teleportDestinationName;
			component2.waitForExternalActivation = false;
		}
		else
		{
			teleportDestinationName = linkName;
		}
		base.Awake();
	}

	public void Start()
	{
		if (linked != null)
		{
			destinationIcon.sprite = ZoneDirector.GetZoneIcon(linked.gameObject);
			destinationIcon.enabled = destinationIcon.sprite != null;
			linked.destinationIcon.sprite = ZoneDirector.GetZoneIcon(base.gameObject);
			linked.destinationIcon.enabled = linked.destinationIcon.sprite != null;
		}
	}

	public bool ShouldDestroyPair()
	{
		return linked != null;
	}

	public Gadget.LinkDestroyer GetLinked()
	{
		return linked;
	}
}
