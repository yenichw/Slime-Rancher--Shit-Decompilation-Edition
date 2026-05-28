using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace MonomiPark.SlimeRancher.Regions
{
	public class RegionMember : SRBehaviour, ActorModel.Participant, GadgetModel.Participant, PlayerModel.Participant
	{
		public delegate void MembershipChanged(List<Region> left, List<Region> joined);

		public bool canHibernate = true;

		[NonSerialized]
		public List<Region> regions = new List<Region>(4);

		private ActorModel actorModel;

		private DroneModel droneModel;

		private Vector3 lastMemberCheckPos;

		private RegionRegistry regionRegistry;

		private static List<Region> newRegions = new List<Region>(4);

		private static List<Region> leftRegions = new List<Region>(4);

		private const float MEMBER_RECHECK_DIST = 1f;

		private const float MEMBER_RECHECK_DIST_SQR = 1f;

		private bool regionsInitialized;

		private bool hibernating;

		private Vector3 hibernatingVelocity;

		private Vector3 hibernatingAngularVelocity;

		private bool hibernatingIsKinematic;

		private CollisionDetectionMode hibernatingCollisionDetectionMode;

		private List<Behaviour> hibernatingBehaviours = new List<Behaviour>();

		private List<Collider> hibernatingColliders = new List<Collider>();

		private List<Renderer> hibernatingRenderers = new List<Renderer>();

		public RegionRegistry.RegionSetId setId
		{
			get
			{
				if (actorModel == null)
				{
					if (droneModel == null)
					{
						return RegionRegistry.RegionSetId.UNSET;
					}
					return droneModel.currRegionSetId;
				}
				return actorModel.currRegionSetId;
			}
		}

		public event MembershipChanged regionsChanged;

		public void Awake()
		{
			if (Identifiable.GetId(base.gameObject) == Identifiable.Id.PLAYER)
			{
				SRSingleton<SceneContext>.Instance.GameModel.RegisterPlayerParticipant(this);
			}
		}

		public void InitModel(ActorModel model)
		{
		}

		public void SetModel(ActorModel model)
		{
			actorModel = model;
		}

		public void InitModel(GadgetModel model)
		{
		}

		public void SetModel(GadgetModel model)
		{
			droneModel = model as DroneModel;
		}

		public void RegionSetChanged(RegionRegistry.RegionSetId previous, RegionRegistry.RegionSetId current)
		{
			UpdateRegionMembership(forceUpdateEvenWhenHibernating: true);
		}

		public void Start()
		{
			regionRegistry = SRSingleton<SceneContext>.Instance.RegionRegistry;
			regionRegistry.RegisterMember(this);
			UpdateRegionMembership(forceUpdateEvenWhenHibernating: false);
		}

		public void OnEnable()
		{
			if (regionRegistry != null)
			{
				regionRegistry.RegisterMember(this);
				UpdateRegionMembership(forceUpdateEvenWhenHibernating: false);
			}
		}

		public void OnDisable()
		{
			if (regionRegistry != null)
			{
				regionRegistry.DeregisterMember(this);
			}
		}

		public void OnDestroy()
		{
			for (int i = 0; i < regions.Count; i++)
			{
				regions[i].RemoveMember(this);
			}
			if (this.regionsChanged != null && regions.Count > 0)
			{
				this.regionsChanged(regions, null);
			}
			regions.Clear();
		}

		public void RegistryUpdate()
		{
			if ((base.transform.position - lastMemberCheckPos).sqrMagnitude >= 1f)
			{
				UpdateRegionMembership(forceUpdateEvenWhenHibernating: false);
			}
		}

		public bool IsInZone(ZoneDirector.Zone zone)
		{
			return regions.Any((Region r) => r.GetZoneId() == zone);
		}

		public bool IsInRegion(RegionRegistry.RegionSetId regionSetId)
		{
			return setId == regionSetId;
		}

		public void UpdateRegionMembership(bool forceUpdateEvenWhenHibernating)
		{
			if (!Application.isPlaying || (hibernating && !forceUpdateEvenWhenHibernating))
			{
				return;
			}
			newRegions.Clear();
			leftRegions.Clear();
			regionRegistry.GetContaining(setId, ref newRegions, base.transform.position);
			lastMemberCheckPos = base.transform.position;
			int num = 0;
			int num2 = regions.Count;
			while (num < num2)
			{
				Region item = regions[num];
				if (newRegions.Contains(item))
				{
					newRegions.Remove(item);
					num++;
					continue;
				}
				leftRegions.Add(item);
				regions[num].RemoveMember(this);
				regions.RemoveAt(num);
				num2--;
			}
			int count = newRegions.Count;
			if (count > 0)
			{
				for (num = 0; num < count; num++)
				{
					Region region = newRegions[num];
					regions.Add(region);
					region.AddMember(this);
				}
			}
			if (leftRegions.Count > 0 || newRegions.Count > 0 || !regionsInitialized)
			{
				UpdateHibernation();
			}
			regionsInitialized = true;
			if ((leftRegions.Count > 0 || newRegions.Count > 0) && this.regionsChanged != null)
			{
				this.regionsChanged(leftRegions, newRegions);
			}
		}

		public void UpdateHibernation()
		{
			if (!canHibernate)
			{
				return;
			}
			bool flag;
			if (!regionRegistry.IsCurrRegionSet(setId))
			{
				flag = true;
			}
			else
			{
				bool flag2 = false;
				int count = regions.Count;
				bool flag3 = count > 0;
				for (int i = 0; i < count; i++)
				{
					Region region = regions[i];
					if (region.Proxied)
					{
						flag2 = true;
					}
					if (!region.Hibernated)
					{
						flag3 = false;
					}
				}
				flag = flag2 || flag3;
			}
			if (flag && !hibernating)
			{
				Hibernate();
				base.gameObject.SetActive(value: false);
			}
			else if (!flag && hibernating)
			{
				Unhibernate();
				base.gameObject.SetActive(value: true);
			}
		}

		private void Unhibernate()
		{
			if (hibernating)
			{
				hibernating = false;
				UpdateComponents();
			}
		}

		private void Hibernate()
		{
			if (!hibernating)
			{
				hibernating = true;
				UpdateComponents();
			}
		}

		private void UpdateComponents()
		{
			if (hibernating)
			{
				Behaviour[] componentsInChildren = GetComponentsInChildren<Behaviour>();
				int num = componentsInChildren.Length;
				for (int i = 0; i < num; i++)
				{
					Behaviour behaviour = componentsInChildren[i];
					if (behaviour != null && behaviour.enabled && behaviour.GetType() != typeof(RegionMember))
					{
						hibernatingBehaviours.Add(behaviour);
						behaviour.enabled = false;
					}
				}
			}
			else
			{
				int count = hibernatingBehaviours.Count;
				for (int j = 0; j < count; j++)
				{
					Behaviour behaviour2 = hibernatingBehaviours[j];
					if (behaviour2 != null)
					{
						behaviour2.enabled = true;
					}
				}
				hibernatingBehaviours.Clear();
			}
			Rigidbody component = GetComponent<Rigidbody>();
			if (component != null)
			{
				if (hibernating)
				{
					hibernatingVelocity = component.velocity;
					hibernatingAngularVelocity = component.angularVelocity;
					hibernatingIsKinematic = component.isKinematic;
					hibernatingCollisionDetectionMode = component.collisionDetectionMode;
					component.Sleep();
					component.collisionDetectionMode = CollisionDetectionMode.Discrete;
					component.isKinematic = true;
				}
				else if (component.IsSleeping())
				{
					component.isKinematic = hibernatingIsKinematic;
					component.collisionDetectionMode = hibernatingCollisionDetectionMode;
					component.WakeUp();
					component.velocity = hibernatingVelocity;
					component.angularVelocity = hibernatingAngularVelocity;
				}
			}
			if (hibernating)
			{
				Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
				int num2 = componentsInChildren2.Length;
				for (int k = 0; k < num2; k++)
				{
					Collider collider = componentsInChildren2[k];
					if (collider.enabled)
					{
						hibernatingColliders.Add(collider);
						collider.enabled = false;
					}
				}
			}
			else
			{
				int count2 = hibernatingColliders.Count;
				for (int l = 0; l < count2; l++)
				{
					Collider collider2 = hibernatingColliders[l];
					if (collider2 != null)
					{
						collider2.enabled = true;
					}
				}
				hibernatingColliders.Clear();
			}
			if (hibernating)
			{
				Renderer[] componentsInChildren3 = GetComponentsInChildren<Renderer>();
				int num3 = componentsInChildren3.Length;
				for (int m = 0; m < num3; m++)
				{
					Renderer renderer = componentsInChildren3[m];
					if (renderer.enabled)
					{
						hibernatingRenderers.Add(renderer);
						renderer.enabled = false;
					}
				}
				return;
			}
			int count3 = hibernatingRenderers.Count;
			for (int n = 0; n < count3; n++)
			{
				Renderer renderer2 = hibernatingRenderers[n];
				if (renderer2 != null)
				{
					renderer2.enabled = true;
				}
			}
			hibernatingRenderers.Clear();
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
