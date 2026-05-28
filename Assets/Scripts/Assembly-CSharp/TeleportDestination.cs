using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class TeleportDestination : MonoBehaviour
{
	public Transform destLoc;

	public GameObject arriveFX;

	public string teleportDestinationName;

	public bool reorient = true;

	public RegionRegistry.RegionSetId regionSetId { get; private set; }

	public virtual void Awake()
	{
		SRSingleton<SceneContext>.Instance.TeleportNetwork.Register(this);
		regionSetId = GetComponentInParent<Region>().setId;
	}

	public virtual void OnDepart()
	{
		TeleportSource component = base.gameObject.GetComponent<TeleportSource>();
		if (component != null)
		{
			component.waitForTriggerExit = true;
		}
	}

	public void OnArrive()
	{
		if (arriveFX != null)
		{
			Object.Instantiate(arriveFX, base.transform.position, base.transform.rotation);
		}
		GetComponent<SECTR_AudioSource>().Play();
	}

	public Vector3 GetPosition()
	{
		return base.gameObject.transform.position;
	}

	public Vector3? GetEulerAngles()
	{
		if (reorient)
		{
			return base.gameObject.transform.eulerAngles;
		}
		return null;
	}

	public virtual void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.TeleportNetwork.Deregister(this);
		}
	}

	public virtual bool IsLinkActive()
	{
		TeleportSource component = base.gameObject.GetComponent<TeleportSource>();
		if (!(component == null))
		{
			return component.IsLinkActive();
		}
		return true;
	}
}
