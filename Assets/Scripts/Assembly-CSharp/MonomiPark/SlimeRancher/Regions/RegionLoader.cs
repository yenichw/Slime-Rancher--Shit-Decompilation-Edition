using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Regions
{
	public class RegionLoader : SRBehaviour, PlayerModel.Participant
	{
		[Tooltip("The dimensions of the volume in which stuff should be unhibernated/awake.")]
		public Vector3 WakeSize = new Vector3(20f, 10f, 20f);

		[Tooltip("The dimensions of the volume in which regions should be loaded.")]
		public Vector3 LoadSize = new Vector3(20f, 10f, 20f);

		[Tooltip("The distance from the load size that you need to move for a Region to unload (as a percentage).")]
		[Range(0f, 1f)]
		public float UnloadBuffer = 0.1f;

		private Vector3 lastRegionCheckPos;

		private List<Region> nonProxiedRegions = new List<Region>(16);

		private List<Region> nonHibernatedRegions = new List<Region>(16);

		private RegionRegistry regionReg;

		private static List<Region> loadRegions = new List<Region>(16);

		private static List<Region> unloadRegions = new List<Region>(16);

		private const float REGION_UPDATE_DIST = 1f;

		private const float REGION_UPDATE_DIST_SQR = 1f;

		public void Awake()
		{
			regionReg = SRSingleton<SceneContext>.Instance.RegionRegistry;
			SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
		}

		public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
		{
			LoadRegion(current, previous.ToEnumerable());
		}

		public void Start()
		{
			RegionRegistry.RegionSetId current = regionReg.GetCurrentRegionSetId();
			LoadRegion(current, from RegionRegistry.RegionSetId r in Enum.GetValues(typeof(RegionRegistry.RegionSetId))
				where r != RegionRegistry.RegionSetId.UNSET
				where r != current
				select r);
		}

		public void Update()
		{
			if ((base.transform.position - lastRegionCheckPos).sqrMagnitude >= 1f)
			{
				ForceUpdate();
			}
		}

		private void LoadRegion(RegionRegistry.RegionSetId current, IEnumerable<RegionRegistry.RegionSetId> previousEnumerable)
		{
			foreach (RegionRegistry.RegionSetId item in previousEnumerable)
			{
				foreach (Region region in regionReg.GetRegions(item))
				{
					region.OnRegionSetDeactivated();
				}
			}
			ForceUpdate();
			foreach (Region region2 in regionReg.GetRegions(current))
			{
				if (!nonProxiedRegions.Contains(region2))
				{
					region2.RemoveNonProxiedReference();
				}
			}
		}

		private void ForceUpdate()
		{
			UpdateProxied(base.transform.position);
			UpdateHibernated(base.transform.position);
			lastRegionCheckPos = base.transform.position;
		}

		private void UpdateProxied(Vector3 position)
		{
			Bounds bounds = new Bounds(position, LoadSize);
			Bounds bounds2 = new Bounds(position, LoadSize * (1f + UnloadBuffer));
			regionReg.GetContaining(ref loadRegions, bounds);
			regionReg.GetContaining(ref unloadRegions, bounds2);
			int num = 0;
			int num2 = nonProxiedRegions.Count;
			while (num < num2)
			{
				Region region = nonProxiedRegions[num];
				if (loadRegions.Contains(region))
				{
					loadRegions.Remove(region);
					num++;
				}
				else if (!unloadRegions.Contains(region))
				{
					region.RemoveNonProxiedReference();
					nonProxiedRegions.RemoveAt(num);
					num2--;
				}
				else
				{
					num++;
				}
			}
			num2 = loadRegions.Count;
			if (num2 <= 0)
			{
				return;
			}
			for (num = 0; num < num2; num++)
			{
				Region region2 = loadRegions[num];
				if (!nonProxiedRegions.Contains(region2))
				{
					region2.AddNonProxiedReference();
					nonProxiedRegions.Add(region2);
				}
			}
		}

		private void UpdateHibernated(Vector3 position)
		{
			Bounds bounds = new Bounds(position, WakeSize);
			Bounds bounds2 = new Bounds(position, WakeSize * (1f + UnloadBuffer));
			regionReg.GetContaining(ref loadRegions, bounds);
			regionReg.GetContaining(ref unloadRegions, bounds2);
			int num = 0;
			int num2 = nonHibernatedRegions.Count;
			while (num < num2)
			{
				Region region = nonHibernatedRegions[num];
				if (loadRegions.Contains(region))
				{
					loadRegions.Remove(region);
					num++;
				}
				else if (!unloadRegions.Contains(region))
				{
					region.RemoveNonHibernateReference();
					nonHibernatedRegions.RemoveAt(num);
					num2--;
				}
				else
				{
					num++;
				}
			}
			num2 = loadRegions.Count;
			if (num2 <= 0)
			{
				return;
			}
			for (num = 0; num < num2; num++)
			{
				Region region2 = loadRegions[num];
				if (!nonHibernatedRegions.Contains(region2))
				{
					region2.AddNonHibernateReference();
					nonHibernatedRegions.Add(region2);
				}
			}
		}

		public void InitModel(PlayerModel model)
		{
		}

		public void SetModel(PlayerModel model)
		{
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
	}
}
