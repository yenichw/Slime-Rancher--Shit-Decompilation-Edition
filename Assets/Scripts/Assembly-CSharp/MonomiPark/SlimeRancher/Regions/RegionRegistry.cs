using System;
using System.Collections.Generic;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Regions
{
	public class RegionRegistry : MonoBehaviour, PlayerModel.Participant
	{
		public enum RegionSetId
		{
			UNSET = -1,
			HOME = 0,
			DESERT = 1,
			VALLEY = 2,
			VIKTOR_LAB = 3,
			SLIMULATIONS = 4
		}

		public class RegionSetIdComparer : IEqualityComparer<RegionSetId>
		{
			public static readonly RegionSetIdComparer Instance = new RegionSetIdComparer();

			public bool Equals(RegionSetId x, RegionSetId y)
			{
				return x == y;
			}

			public int GetHashCode(RegionSetId id)
			{
				return (int)id;
			}
		}

		private Dictionary<RegionSetId, BoundsQuadtree<Region>> regionsTrees = new Dictionary<RegionSetId, BoundsQuadtree<Region>>(RegionSetIdComparer.Instance)
		{
			{
				RegionSetId.HOME,
				new BoundsQuadtree<Region>(2000f, Vector3.zero, 250f, 1.2f)
			},
			{
				RegionSetId.DESERT,
				new BoundsQuadtree<Region>(2000f, Vector3.up * 1000f, 250f, 1.2f)
			},
			{
				RegionSetId.VALLEY,
				new BoundsQuadtree<Region>(2000f, Vector3.back * 900f, 250f, 1.2f)
			},
			{
				RegionSetId.VIKTOR_LAB,
				new BoundsQuadtree<Region>(2000f, new Vector3(914f, 0f, -180f), 250f, 1.2f)
			},
			{
				RegionSetId.SLIMULATIONS,
				new BoundsQuadtree<Region>(2000f, new Vector3(1142f, 0f, 1562f), 250f, 1.2f)
			}
		};

		private Dictionary<RegionSetId, List<GameObject>> managedWithSets = new Dictionary<RegionSetId, List<GameObject>>();

		private ExposedArrayList<RegionMember> membersToUpdate = new ExposedArrayList<RegionMember>(1000);

		private PlayerModel playerModel;

		private const int MIN_LOCAL_ARRAY_RESIZE_AMOUNT = 50;

		private RegionMember[] UpdateMembers_localMembers = new RegionMember[50];

		public void Awake()
		{
			SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
		}

		public void InitModel(PlayerModel model)
		{
		}

		public void SetModel(PlayerModel model)
		{
			playerModel = model;
			LoadRegion(GetCurrentRegionSetId());
		}

		public void GetContaining(ref List<Region> regions, Vector3 pos)
		{
			GetContaining(GetCurrentRegionSetId(), ref regions, pos);
		}

		public void GetContaining(ref List<Region> regions, Bounds bounds)
		{
			GetContaining(GetCurrentRegionSetId(), ref regions, bounds);
		}

		public void GetContaining(RegionSetId setId, ref List<Region> regions, Vector3 pos)
		{
			regions.Clear();
			regionsTrees[setId].GetColliding(pos, ref regions);
		}

		public void GetContaining(RegionSetId setId, ref List<Region> regions, Bounds bounds)
		{
			regions.Clear();
			regionsTrees[setId].GetColliding(bounds, ref regions);
		}

		public RegionSetId GetCurrentRegionSetId()
		{
			return playerModel.currRegionSetId;
		}

		public void RegionSetChanged(RegionSetId previous, RegionSetId current)
		{
			LoadRegion(current);
		}

		public void TransformChanged(Vector3 pos, Quaternion rot)
		{
		}

		public void RegisteredPotentialAmmoChanged(Dictionary<PlayerState.AmmoMode, List<GameObject>> registeredPotentialAmmo)
		{
		}

		public void KeyAdded()
		{
		}

		public List<Region> GetRegions(RegionSetId setId)
		{
			List<Region> result = new List<Region>();
			return regionsTrees[setId].GetAll(ref result);
		}

		public bool IsCurrRegionSet(RegionSetId setId)
		{
			return setId == playerModel.currRegionSetId;
		}

		public void ManageWithRegionSet(GameObject obj, RegionSetId setId)
		{
			if (!managedWithSets.ContainsKey(setId))
			{
				managedWithSets[setId] = new List<GameObject>();
			}
			managedWithSets[setId].Add(obj);
			if (playerModel != null)
			{
				bool active = IsCurrRegionSet(setId);
				obj.SetActive(active);
			}
		}

		public void ReleaseFromRegionSet(GameObject obj, RegionSetId setId)
		{
			if (managedWithSets.ContainsKey(setId))
			{
				managedWithSets[setId].Remove(obj);
			}
		}

		public void RegisterMember(RegionMember member)
		{
			membersToUpdate.Add(member);
		}

		public void DeregisterMember(RegionMember member)
		{
			membersToUpdate.Remove(member);
		}

		public void RegisterRegion(Region region, RegionSetId setId, Bounds bounds)
		{
			regionsTrees[setId].Add(region, bounds);
		}

		public void DeregisterRegion(Region region, RegionSetId setId)
		{
			regionsTrees[setId].Remove(region);
		}

		public void Update()
		{
			UpdateMembers();
		}

		private void LoadRegion(RegionSetId setId)
		{
			foreach (KeyValuePair<RegionSetId, List<GameObject>> pair in managedWithSets)
			{
				pair.Value.ForEach(delegate(GameObject go)
				{
					go.SetActive(pair.Key == setId);
				});
			}
		}

		private void UpdateMembers()
		{
			if (membersToUpdate.Data.Length > UpdateMembers_localMembers.Length)
			{
				Array.Resize(ref UpdateMembers_localMembers, Math.Max(membersToUpdate.Data.Length, 50));
			}
			int count = membersToUpdate.GetCount();
			membersToUpdate.Data.CopyTo(UpdateMembers_localMembers, 0);
			for (int i = 0; i < count; i++)
			{
				try
				{
					UpdateMembers_localMembers[i].RegistryUpdate();
				}
				catch (NullReferenceException)
				{
					Log.Debug("Null reference caught in RegionRegistry update.", "position", i);
				}
			}
		}
	}
}
