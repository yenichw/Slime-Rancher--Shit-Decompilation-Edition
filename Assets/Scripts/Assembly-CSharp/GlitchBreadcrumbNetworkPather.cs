using UnityEngine;

public class GlitchBreadcrumbNetworkPather : Pather
{
	protected override bool PathPredicate(Vector3 start, Vector3 end)
	{
		return false;
	}

	protected override bool NearestAccessibleNodePredicate(Vector3 start, Vector3 end)
	{
		return true;
	}
}
