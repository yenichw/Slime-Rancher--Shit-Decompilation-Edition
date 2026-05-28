using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Pather
{
	[Serializable]
	public class NodePair : IEquatable<NodePair>
	{
		public PathingNetworkNode node1;

		public PathingNetworkNode node2;

		public NodePair(PathingNetworkNode node1, PathingNetworkNode node2)
		{
			this.node1 = node1;
			this.node2 = node2;
		}

		public bool Equals(NodePair other)
		{
			if (!(node1 == other.node1) || !(node2 == other.node2))
			{
				if (node1 == other.node2)
				{
					return node2 == other.node1;
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (node1 == null || node2 == null)
			{
				return 0;
			}
			return node1.GetHashCode() ^ node2.GetHashCode();
		}
	}

	protected PathingNetworkNode[] nodes = new PathingNetworkNode[0];

	public void RecalculateNodeConnections(PathingNetworkNode[] nodes, List<NodePair> whitelist, List<NodePair> blacklist)
	{
		this.nodes = nodes;
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].connections = new List<PathingNetworkNode>();
		}
		HashSet<NodePair> hashSet = new HashSet<NodePair>(whitelist);
		HashSet<NodePair> hashSet2 = new HashSet<NodePair>(blacklist);
		for (int j = 0; j < nodes.Length; j++)
		{
			PathingNetworkNode pathingNetworkNode = nodes[j];
			for (int k = j + 1; k < nodes.Length; k++)
			{
				PathingNetworkNode pathingNetworkNode2 = nodes[k];
				NodePair item = new NodePair(nodes[j], nodes[k]);
				if (!hashSet2.Contains(item) && (hashSet.Contains(item) || PathPredicate(pathingNetworkNode.position, pathingNetworkNode2.position)))
				{
					pathingNetworkNode.connections.Add(pathingNetworkNode2);
					pathingNetworkNode2.connections.Add(pathingNetworkNode);
				}
			}
		}
	}

	public List<PathingNetworkNode> GeneratePathNodes(Vector3 start, Vector3 end)
	{
		if (PathPredicate(start, end))
		{
			return new List<PathingNetworkNode>();
		}
		PathingNetworkNode pathingNetworkNode = NearestAccessibleNode(start);
		if (pathingNetworkNode == null)
		{
			return null;
		}
		PathingNetworkNode pathingNetworkNode2 = NearestAccessibleNode(end);
		if (pathingNetworkNode2 == null)
		{
			return null;
		}
		HashSet<PathingNetworkNode> hashSet = new HashSet<PathingNetworkNode>();
		HashSet<PathingNetworkNode> hashSet2 = new HashSet<PathingNetworkNode>();
		hashSet2.Add(pathingNetworkNode);
		Dictionary<PathingNetworkNode, PathingNetworkNode> dictionary = new Dictionary<PathingNetworkNode, PathingNetworkNode>();
		Dictionary<PathingNetworkNode, float> dictionary2 = new Dictionary<PathingNetworkNode, float>();
		dictionary2[pathingNetworkNode] = 0f;
		Dictionary<PathingNetworkNode, float> fScore = new Dictionary<PathingNetworkNode, float>();
		fScore[pathingNetworkNode] = (pathingNetworkNode.position - pathingNetworkNode2.position).magnitude;
		while (hashSet2.Count > 0)
		{
			PathingNetworkNode pathingNetworkNode3 = hashSet2.OrderBy((PathingNetworkNode node) => fScore[node]).First();
			if (pathingNetworkNode3 == pathingNetworkNode2)
			{
				List<PathingNetworkNode> list = ConstructPathFromAStarResults(dictionary, pathingNetworkNode3);
				TrimPathEnds(list, start, end);
				return list;
			}
			hashSet2.Remove(pathingNetworkNode3);
			hashSet.Add(pathingNetworkNode3);
			foreach (PathingNetworkNode connection in pathingNetworkNode3.connections)
			{
				if (!hashSet.Contains(connection))
				{
					if (!hashSet2.Contains(connection))
					{
						hashSet2.Add(connection);
					}
					float num = GetValueOrDefault(dictionary2, pathingNetworkNode3, float.PositiveInfinity) + (pathingNetworkNode3.position - connection.position).magnitude;
					if (num < GetValueOrDefault(dictionary2, connection, float.PositiveInfinity))
					{
						dictionary[connection] = pathingNetworkNode3;
						dictionary2[connection] = num;
						fScore[connection] = num + (connection.position - pathingNetworkNode2.position).magnitude;
					}
				}
			}
		}
		return null;
	}

	public Queue<Vector3> GeneratePath(Vector3 start, Vector3 end)
	{
		List<PathingNetworkNode> list = GeneratePathNodes(start, end);
		if (list == null)
		{
			return null;
		}
		Queue<Vector3> queue = new Queue<Vector3>(list.Select((PathingNetworkNode n) => n.position));
		queue.Enqueue(end);
		return queue;
	}

	protected abstract bool PathPredicate(Vector3 start, Vector3 end);

	protected abstract bool NearestAccessibleNodePredicate(Vector3 start, Vector3 end);

	private PathingNetworkNode NearestAccessibleNode(Vector3 pos)
	{
		return (from n in nodes
			where NearestAccessibleNodePredicate(n.position, pos)
			orderby (n.position - pos).sqrMagnitude
			select n).FirstOrDefault();
	}

	private V GetValueOrDefault<K, V>(Dictionary<K, V> dict, K key, V defVal)
	{
		if (dict.ContainsKey(key))
		{
			return dict[key];
		}
		return defVal;
	}

	private List<PathingNetworkNode> ConstructPathFromAStarResults(Dictionary<PathingNetworkNode, PathingNetworkNode> cameFrom, PathingNetworkNode goal)
	{
		List<PathingNetworkNode> list = new List<PathingNetworkNode>();
		list.Add(goal);
		PathingNetworkNode pathingNetworkNode = goal;
		while (cameFrom.ContainsKey(pathingNetworkNode))
		{
			pathingNetworkNode = cameFrom[pathingNetworkNode];
			list.Add(pathingNetworkNode);
		}
		list.Reverse();
		return list;
	}

	private void TrimPathEnds(List<PathingNetworkNode> path, Vector3 start, Vector3 end)
	{
		for (int num = path.Count - 1; num >= 0; num--)
		{
			if (PathPredicate(start, path[num].position))
			{
				path.RemoveRange(0, num);
				break;
			}
		}
		for (int i = 0; i < path.Count - 1; i++)
		{
			if (PathPredicate(path[i].position, end))
			{
				path.RemoveRange(i + 1, path.Count - (i + 1));
				break;
			}
		}
	}
}
