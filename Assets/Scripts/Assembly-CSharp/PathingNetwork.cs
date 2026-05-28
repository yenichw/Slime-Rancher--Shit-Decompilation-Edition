using System.Collections.Generic;
using UnityEngine;

public abstract class PathingNetwork : SRBehaviour
{
	[Tooltip("GameObject parenting the PathingNetworkNode.")]
	public GameObject nodesParent;

	[Tooltip("List of node pairings on the whitelist.")]
	public List<Pather.NodePair> whitelistConnections;

	[Tooltip("List of node pairings on the blacklist.")]
	public List<Pather.NodePair> blacklistConnections;

	[Tooltip("Enable/disable drawing of the network node gizmos.")]
	public bool drawNodeGizmos;

	[Tooltip("Enable/disable drawing of the network connection override gizmos.")]
	public bool drawOverrideGizmos;

	public abstract Pather Pather { get; }

	public PathingNetworkNode[] Nodes
	{
		get
		{
			if (!(nodesParent != null))
			{
				return new PathingNetworkNode[0];
			}
			return nodesParent.GetComponentsInChildren<PathingNetworkNode>(includeInactive: true);
		}
	}

	public virtual void Awake()
	{
		RecalculateNodeConnections();
	}

	public void RecalculateNodeConnections()
	{
		Pather.RecalculateNodeConnections(Nodes, whitelistConnections, blacklistConnections);
	}

	public Queue<Vector3> GeneratePath(Vector3 start, Vector3 end)
	{
		return Pather.GeneratePath(start, end);
	}
}
