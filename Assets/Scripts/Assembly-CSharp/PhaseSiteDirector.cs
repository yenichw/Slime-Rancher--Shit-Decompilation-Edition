using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class PhaseSiteDirector : SRBehaviour, WorldModel.Participant
{
	public List<PhaseSite> availablePhaseSites;

	public List<PhaseSite> occupiedPhaseSites = new List<PhaseSite>();

	public PhaseableObject phaseableObjectPrefab;

	public int numberOfPhaseableObjects;

	private const int MAX_SITE_SELECTION_ATTEMPTS = 10;

	private const float UPDATE_SPAWNABLE_RESOURCE_PERIOD = 10f;

	private float nextSpawnableResourceUpdate;

	private TimeDirector timeDirector;

	private WorldModel worldModel;

	private List<PhaseSite> local_occupiedPhaseSite = new List<PhaseSite>();

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		SRSingleton<SceneContext>.Instance.GameModel.RegisterWorldParticipant(this);
	}

	public void InitModel(WorldModel model)
	{
	}

	public void SetModel(WorldModel model)
	{
		worldModel = model;
		ResetAllSites();
		foreach (PhaseSite item in new List<PhaseSite>(availablePhaseSites))
		{
			if (model.occupiedPhaseSites.Contains(item.id))
			{
				PlacePhaseObject(item);
			}
		}
		RefreshTotalPhaseableObjects();
	}

	public void ClearSites()
	{
		foreach (PhaseSite item in new List<PhaseSite>(occupiedPhaseSites))
		{
			ClearSite(item);
		}
	}

	public void Update()
	{
		foreach (PhaseSite occupiedPhaseSite in occupiedPhaseSites)
		{
			local_occupiedPhaseSite.Add(occupiedPhaseSite);
		}
		bool flag = false;
		if (nextSpawnableResourceUpdate < Time.time)
		{
			flag = true;
			nextSpawnableResourceUpdate = Time.time + 10f;
		}
		foreach (PhaseSite item in local_occupiedPhaseSite)
		{
			if (item.phaseableObject.ReadyToPhase())
			{
				PhaseSite target = PickRandomAvailableSite();
				Phase(item, target);
			}
			else if (flag)
			{
				SpawnResource component = item.phaseableObject.GetComponent<SpawnResource>();
				if (component != null && !component.isActiveAndEnabled)
				{
					component.UpdateToTime(timeDirector.WorldTime(), 0.0);
				}
			}
		}
		local_occupiedPhaseSite.Clear();
	}

	public void PlacePhaseObject(string phaseSiteId)
	{
		PhaseSite phaseSite = availablePhaseSites.FirstOrDefault((PhaseSite s) => string.Compare(phaseSiteId, s.id) == 0);
		if (phaseSite != null)
		{
			PlacePhaseObject(phaseSite);
		}
	}

	public void Phase(PhaseSite source, PhaseSite target)
	{
		source.phaseableObject.PhaseOut();
		PlacePhaseObject(target, source.phaseableObject);
		ClearSite(source);
		target.phaseableObject.PhaseIn();
	}

	public void ClearSite(PhaseSite site)
	{
		site.phaseableObject = null;
		availablePhaseSites.Add(site);
		occupiedPhaseSites.Remove(site);
		worldModel.occupiedPhaseSites.Remove(site.id);
	}

	public void PlacePhaseObject(PhaseSite site)
	{
		PlacePhaseObject(site, Object.Instantiate(phaseableObjectPrefab, site.transform));
	}

	public void PlacePhaseObject(PhaseSite site, PhaseableObject phasingObject)
	{
		site.phaseableObject = phasingObject;
		occupiedPhaseSites.Add(site);
		availablePhaseSites.Remove(site);
		worldModel.occupiedPhaseSites.Add(site.id);
	}

	public PhaseSite PickRandomAvailableSite()
	{
		return availablePhaseSites.ElementAt(Random.Range(0, availablePhaseSites.Count - 1));
	}

	public void ResetAllSites()
	{
		foreach (PhaseSite item in new List<PhaseSite>(occupiedPhaseSites))
		{
			PhaseableObject phaseableObject = item.phaseableObject;
			ClearSite(item);
			Destroyer.Destroy(phaseableObject.gameObject, "PhaseSiteDirector.ResetAllSites");
		}
	}

	public void RefreshTotalPhaseableObjects()
	{
		while (occupiedPhaseSites.Count > numberOfPhaseableObjects)
		{
			PhaseSite phaseSite = occupiedPhaseSites.ElementAt(0);
			PhaseableObject phaseableObject = phaseSite.phaseableObject;
			ClearSite(phaseSite);
			Destroyer.Destroy(phaseableObject.gameObject, "PhaseSiteDirector.RefreshTotalPhaseableObjects");
		}
		while (occupiedPhaseSites.Count < numberOfPhaseableObjects)
		{
			PlacePhaseObject(PickRandomAvailableSite());
		}
	}
}
