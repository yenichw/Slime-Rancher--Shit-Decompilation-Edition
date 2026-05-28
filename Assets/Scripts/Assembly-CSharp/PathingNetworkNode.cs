using System.Collections.Generic;
using UnityEngine;

public class PathingNetworkNode : SRBehaviour
{
	[Tooltip("List of other nodes connected to this node.")]
	public List<PathingNetworkNode> connections;

	[Tooltip("Location transform.")]
	public Transform nodeLoc;

	public Vector3 position => nodeLoc.position;
}
