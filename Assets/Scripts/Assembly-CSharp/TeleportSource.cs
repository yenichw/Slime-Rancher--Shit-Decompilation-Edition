using System.Collections.Generic;
using UnityEngine;

public class TeleportSource : SRBehaviour
{
	public GameObject activationBlocker;

	[Tooltip("Required progress to activate the teleporter.")]
	public ProgressDirector.ProgressType activationProgress = ProgressDirector.ProgressType.NONE;

	[Tooltip("Progresses that are set when the teleporter becomes active.")]
	public ProgressDirector.ProgressType[] setProgressTypesOnActivate;

	[Tooltip("QuicksilverEnergyGenerator that must not be active to use the teleporter. (optional)")]
	public QuicksilverEnergyGenerator blockingGenerator;

	public GameObject departFX;

	public GameObject activeFX;

	public string destinationSetName;

	[HideInInspector]
	public bool waitForExternalActivation;

	[HideInInspector]
	public bool waitForTriggerExit;

	private TeleportNetwork network;

	private bool activated;

	public virtual void Awake()
	{
		network = SRSingleton<SceneContext>.Instance.TeleportNetwork;
	}

	public void OnDisable()
	{
		waitForTriggerExit = false;
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (network.IsLinkFullyActive(this) && PhysicsUtil.IsPlayerMainCollider(collider))
		{
			TeleportablePlayer component = collider.gameObject.GetComponent<TeleportablePlayer>();
			if (component != null)
			{
				network.TeleportToDestination(component, this, destinationSetName, PickDestination);
			}
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		if (PhysicsUtil.IsPlayerMainCollider(collider))
		{
			waitForTriggerExit = false;
		}
	}

	public virtual void OnDepart()
	{
		if (departFX != null)
		{
			Object.Instantiate(departFX, base.transform.position, base.transform.rotation);
		}
	}

	public void Update()
	{
		bool flag = activated;
		activated = network.IsLinkFullyActive(this);
		if (activated && !flag && setProgressTypesOnActivate != null)
		{
			ProgressDirector.ProgressType[] array = setProgressTypesOnActivate;
			foreach (ProgressDirector.ProgressType type in array)
			{
				SRSingleton<SceneContext>.Instance.ProgressDirector.SetProgress(type, 1);
			}
		}
		if (activeFX != null)
		{
			activeFX.SetActive(activated);
		}
	}

	public void ExternalActivate()
	{
		waitForExternalActivation = false;
	}

	public virtual bool IsLinkActive()
	{
		if (waitForTriggerExit)
		{
			return false;
		}
		if (waitForExternalActivation)
		{
			return false;
		}
		if ((bool)activationBlocker && activationBlocker.activeSelf)
		{
			return false;
		}
		if (activationProgress != ProgressDirector.ProgressType.NONE && !SRSingleton<SceneContext>.Instance.ProgressDirector.HasProgress(activationProgress))
		{
			return false;
		}
		if (blockingGenerator != null && (blockingGenerator.GetState() == QuicksilverEnergyGenerator.State.ACTIVE || blockingGenerator.GetState() == QuicksilverEnergyGenerator.State.COUNTDOWN))
		{
			return false;
		}
		return true;
	}

	protected virtual TeleportDestination PickDestination(List<TeleportDestination> destinations)
	{
		return Randoms.SHARED.Pick(destinations, null);
	}
}
