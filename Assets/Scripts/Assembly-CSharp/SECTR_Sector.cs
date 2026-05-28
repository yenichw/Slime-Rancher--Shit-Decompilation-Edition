using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Sector")]
public class SECTR_Sector : SECTR_Member
{
	public enum SectorSetId
	{
		UNSET = -1,
		HOME = 0,
		DESERT = 1
	}

	private class SectorSetIdComparer : IEqualityComparer<SectorSetId>
	{
		public static readonly SectorSetIdComparer Instance = new SectorSetIdComparer();

		public bool Equals(SectorSetId x, SectorSetId y)
		{
			return x == y;
		}

		public int GetHashCode(SectorSetId id)
		{
			return (int)id;
		}
	}

	private List<SECTR_Portal> portals = new List<SECTR_Portal>(8);

	private List<SECTR_Member> members = new List<SECTR_Member>(32);

	private bool visited;

	private static List<SECTR_Sector> allSectors = new List<SECTR_Sector>(128);

	private static Dictionary<SectorSetId, BoundsQuadtree<SECTR_Sector>> sectorsTrees = new Dictionary<SectorSetId, BoundsQuadtree<SECTR_Sector>>(SectorSetIdComparer.Instance)
	{
		{
			SectorSetId.HOME,
			new BoundsQuadtree<SECTR_Sector>(1000f, Vector3.zero, 250f, 1.2f)
		},
		{
			SectorSetId.DESERT,
			new BoundsQuadtree<SECTR_Sector>(1000f, Vector3.up * 1000f, 250f, 1.2f)
		}
	};

	private static Dictionary<SectorSetId, List<GameObject>> managedWithSets = new Dictionary<SectorSetId, List<GameObject>>();

	private static BoundsQuadtree<SECTR_Sector> currSectorsTree = null;

	[SECTR_ToolTip("The terrain Sector attached on the top side of this Sector.")]
	public SECTR_Sector TopTerrain;

	[SECTR_ToolTip("The terrain Sector attached on the bottom side of this Sector.")]
	public SECTR_Sector BottomTerrain;

	[SECTR_ToolTip("The terrain Sector attached on the left side of this Sector.")]
	public SECTR_Sector LeftTerrain;

	[SECTR_ToolTip("The terrain Sector attached on the right side of this Sector.")]
	public SECTR_Sector RightTerrain;

	public CellDirector cellDir;

	public new static List<SECTR_Sector> All => allSectors;

	public bool Visited
	{
		get
		{
			return visited;
		}
		set
		{
			visited = value;
		}
	}

	public List<SECTR_Portal> Portals => portals;

	public List<SECTR_Member> Members => members;

	private SECTR_Sector()
	{
		isSector = true;
	}

	public static SectorSetId GetSectorSetId()
	{
		if (currSectorsTree == sectorsTrees[SectorSetId.DESERT])
		{
			return SectorSetId.DESERT;
		}
		return SectorSetId.HOME;
	}

	public static void SetCurrSectorSet(SectorSetId setId, bool forceActivation = false)
	{
		if (!forceActivation && currSectorsTree == sectorsTrees[setId])
		{
			return;
		}
		foreach (KeyValuePair<SectorSetId, BoundsQuadtree<SECTR_Sector>> sectorsTree in sectorsTrees)
		{
			bool flag = sectorsTree.Key == setId;
			List<SECTR_Sector> result = new List<SECTR_Sector>();
			result = sectorsTree.Value.GetAll(ref result);
			foreach (SECTR_Sector item in result)
			{
				item.GetComponent<SECTR_Chunk>().SetCanProxy(flag);
			}
			if (!managedWithSets.ContainsKey(sectorsTree.Key))
			{
				continue;
			}
			foreach (GameObject item2 in managedWithSets[sectorsTree.Key])
			{
				item2.SetActive(flag);
			}
		}
		currSectorsTree = sectorsTrees[setId];
	}

	public static void SetCurrSectorSetForPos(Vector3 pos)
	{
		SetCurrSectorSet(GetSectorSetForPos(pos));
	}

	public static SectorSetId GetSectorSetForPos(Vector3 pos)
	{
		if (pos.y > 900f)
		{
			return SectorSetId.DESERT;
		}
		return SectorSetId.HOME;
	}

	public static bool IsCurrSectorSet(SectorSetId setId)
	{
		if (currSectorsTree != null)
		{
			return currSectorsTree == sectorsTrees[setId];
		}
		return true;
	}

	public static void ManageWithSectorSet(GameObject obj, SectorSetId setId)
	{
		if (!managedWithSets.ContainsKey(setId))
		{
			managedWithSets[setId] = new List<GameObject>();
		}
		managedWithSets[setId].Add(obj);
	}

	public static void ReleaseFromSectorSet(GameObject obj, SectorSetId setId)
	{
		if (managedWithSets.ContainsKey(setId))
		{
			managedWithSets[setId].Remove(obj);
		}
	}

	public static void GetContaining(ref List<SECTR_Sector> sectors, Vector3 position)
	{
		sectors.Clear();
		int count = allSectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = allSectors[i];
			if (sECTR_Sector.TotalBounds.Contains(position))
			{
				sectors.Add(sECTR_Sector);
			}
		}
	}

	public static void GetContaining(ref List<SECTR_Sector> sectors, Bounds bounds, bool checkAllSectorSets = false)
	{
		sectors.Clear();
		if (checkAllSectorSets)
		{
			foreach (BoundsQuadtree<SECTR_Sector> value in sectorsTrees.Values)
			{
				value.GetColliding(bounds, ref sectors);
			}
			return;
		}
		if (currSectorsTree != null)
		{
			currSectorsTree.GetColliding(bounds, ref sectors);
		}
	}

	public void ConnectTerrainNeighbors()
	{
		Terrain componentInChildren = GetComponentInChildren<Terrain>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetNeighbors(LeftTerrain ? LeftTerrain.GetComponentInChildren<Terrain>() : null, TopTerrain ? TopTerrain.GetComponentInChildren<Terrain>() : null, RightTerrain ? RightTerrain.GetComponentInChildren<Terrain>() : null, BottomTerrain ? BottomTerrain.GetComponentInChildren<Terrain>() : null);
		}
	}

	public void DisonnectTerrainNeighbors()
	{
		Terrain componentInChildren = GetComponentInChildren<Terrain>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetNeighbors(null, null, null, null);
		}
		if ((bool)TopTerrain)
		{
			Terrain componentInChildren2 = TopTerrain.GetComponentInChildren<Terrain>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.SetNeighbors(TopTerrain.LeftTerrain ? TopTerrain.LeftTerrain.GetComponentInChildren<Terrain>() : null, TopTerrain.TopTerrain ? TopTerrain.TopTerrain.GetComponentInChildren<Terrain>() : null, TopTerrain.RightTerrain ? TopTerrain.RightTerrain.GetComponentInChildren<Terrain>() : null, null);
			}
		}
		if ((bool)BottomTerrain)
		{
			Terrain componentInChildren3 = BottomTerrain.GetComponentInChildren<Terrain>();
			if ((bool)componentInChildren3)
			{
				componentInChildren3.SetNeighbors(BottomTerrain.LeftTerrain ? BottomTerrain.LeftTerrain.GetComponentInChildren<Terrain>() : null, null, BottomTerrain.RightTerrain ? BottomTerrain.RightTerrain.GetComponentInChildren<Terrain>() : null, BottomTerrain.BottomTerrain ? BottomTerrain.BottomTerrain.GetComponentInChildren<Terrain>() : null);
			}
		}
		if ((bool)LeftTerrain)
		{
			Terrain componentInChildren4 = LeftTerrain.GetComponentInChildren<Terrain>();
			if ((bool)componentInChildren4)
			{
				componentInChildren4.SetNeighbors(LeftTerrain.LeftTerrain ? LeftTerrain.LeftTerrain.GetComponentInChildren<Terrain>() : null, LeftTerrain.TopTerrain ? LeftTerrain.TopTerrain.GetComponentInChildren<Terrain>() : null, null, LeftTerrain.BottomTerrain ? LeftTerrain.BottomTerrain.GetComponentInChildren<Terrain>() : null);
			}
		}
		if ((bool)RightTerrain)
		{
			Terrain componentInChildren5 = RightTerrain.GetComponentInChildren<Terrain>();
			if ((bool)componentInChildren5)
			{
				componentInChildren5.SetNeighbors(null, RightTerrain.TopTerrain ? RightTerrain.TopTerrain.GetComponentInChildren<Terrain>() : null, RightTerrain.RightTerrain ? RightTerrain.RightTerrain.GetComponentInChildren<Terrain>() : null, RightTerrain.BottomTerrain ? RightTerrain.BottomTerrain.GetComponentInChildren<Terrain>() : null);
			}
		}
	}

	public void Register(SECTR_Portal portal)
	{
		if (!portals.Contains(portal))
		{
			portals.Add(portal);
		}
	}

	public void Deregister(SECTR_Portal portal)
	{
		portals.Remove(portal);
	}

	public void Register(SECTR_Member member)
	{
		members.Add(member);
	}

	public void Deregister(SECTR_Member member)
	{
		members.Remove(member);
	}

	public override void Awake()
	{
		base.Awake();
		cellDir = GetComponent<CellDirector>();
		if (Application.isPlaying && currSectorsTree == null)
		{
			SetCurrSectorSet(SectorSetId.HOME, forceActivation: true);
		}
	}

	protected override void OnEnable()
	{
		allSectors.Add(this);
		SectorSetId setId = GetSetId();
		sectorsTrees[setId].Add(this, base.TotalBounds);
		if (Application.isPlaying)
		{
			GetComponent<SECTR_Chunk>().SetCanProxy(currSectorsTree == sectorsTrees[setId]);
		}
		if ((bool)TopTerrain || (bool)BottomTerrain || (bool)RightTerrain || (bool)LeftTerrain)
		{
			ConnectTerrainNeighbors();
		}
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		List<SECTR_Member> list = new List<SECTR_Member>(members);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = list[i];
			if ((bool)sECTR_Member)
			{
				sECTR_Member.SectorDisabled(this);
			}
		}
		allSectors.Remove(this);
		sectorsTrees[GetSetId()].Remove(this);
		base.OnDisable();
	}

	public SectorSetId GetSetId()
	{
		ZoneDirector[] componentsInParent = GetComponentsInParent<ZoneDirector>(includeInactive: true);
		if (componentsInParent.Length == 0)
		{
			return SectorSetId.HOME;
		}
		if (componentsInParent[0].zone != ZoneDirector.Zone.DESERT)
		{
			return SectorSetId.HOME;
		}
		return SectorSetId.DESERT;
	}

	public override void NonOffsetLateUpdate()
	{
		Bounds bounds = base.TotalBounds;
		base.NonOffsetLateUpdate();
		if (bounds != base.TotalBounds)
		{
			SectorSetId setId = GetSetId();
			sectorsTrees[setId].Remove(this);
			sectorsTrees[setId].Add(this, base.TotalBounds);
		}
	}

	public override void OffsetLateUpdate()
	{
		Bounds bounds = base.TotalBounds;
		base.OffsetLateUpdate();
		if (bounds != base.TotalBounds)
		{
			SectorSetId setId = GetSetId();
			sectorsTrees[setId].Remove(this);
			sectorsTrees[setId].Add(this, base.TotalBounds);
		}
	}
}
