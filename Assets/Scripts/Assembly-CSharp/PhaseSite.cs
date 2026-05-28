using System.Collections.Generic;

public class PhaseSite : IdHandler
{
	public static List<PhaseSite> allSites = new List<PhaseSite>();

	public PhaseableObject phaseableObject;

	public void Awake()
	{
		allSites.Add(this);
	}

	public void OnDestroy()
	{
		allSites.Remove(this);
	}

	protected override string IdPrefix()
	{
		return "phaseSite";
	}
}
