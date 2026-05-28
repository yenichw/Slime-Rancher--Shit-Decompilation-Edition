using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlitchBreadcrumbNetwork : PathingNetwork
{
	private GlitchBreadcrumbNetworkPather pather = new GlitchBreadcrumbNetworkPather();

	private List<GlitchBreadcrumbNetworkNode> activeBreadcrumbs;

	private GlitchTeleportDestination exitDestination;

	public override Pather Pather => pather;

	public void Update()
	{
		List<PathingNetworkNode> list = null;
		if (exitDestination != null && exitDestination.IsLinkActive())
		{
			Vector3 position = SRSingleton<SceneContext>.Instance.Player.transform.position;
			Vector3 position2 = exitDestination.transform.position;
			list = Pather.GeneratePathNodes(position, position2);
		}
		if (list == null != (activeBreadcrumbs == null) || (list != null && list[0] != activeBreadcrumbs[0]))
		{
			OnBreadcrumbsChanged(list);
		}
	}

	public void OnDisable()
	{
		OnBreadcrumbsChanged(null);
	}

	public void OnGlitchRegionLoaded()
	{
		foreach (GlitchTeleportDestination destination in SRSingleton<GlitchRegionHelper>.Instance.destinations)
		{
			destination.onExitTeleporterBecameActive += OnExitTeleporterBecameActive;
		}
	}

	private void OnExitTeleporterBecameActive(GlitchTeleportDestination destination)
	{
		exitDestination = destination;
	}

	private void OnBreadcrumbsChanged(List<PathingNetworkNode> breadcrumbs)
	{
		if (activeBreadcrumbs != null)
		{
			activeBreadcrumbs.ForEach(delegate(GlitchBreadcrumbNetworkNode b)
			{
				b.Deactivate();
			});
			activeBreadcrumbs = null;
		}
		if (breadcrumbs != null && breadcrumbs.Any())
		{
			activeBreadcrumbs = breadcrumbs.Cast<GlitchBreadcrumbNetworkNode>().ToList();
			for (int i = 0; i < activeBreadcrumbs.Count; i++)
			{
				activeBreadcrumbs[i].Activate((i + 1 >= activeBreadcrumbs.Count) ? exitDestination.transform.position : activeBreadcrumbs[i + 1].position);
			}
		}
	}
}
