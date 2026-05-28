using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MonomiPark.SlimeRancher.Regions
{
	[ExecuteInEditMode]
	public class Region : MonoBehaviour
	{
		public delegate void OnHibernationStateChanged(bool hibernated);

		public bool overrideBounds;

		public Bounds bounds;

		public GameObject root;

		public Mesh proxyMesh;

		public Material[] proxyMaterials;

		private GameObject proxyObj;

		private int nonProxyRefCount;

		private int nonHibernateRefCount;

		private RegionRegistry regionReg;

		private ExposedArrayList<RegionMember> members = new ExposedArrayList<RegionMember>();

		[HideInInspector]
		public CellDirector cellDir;

		private const float HEADROOM = 160f;

		private const int MIN_LOCAL_ARRAY_RESIZE_AMOUNT = 10;

		private static RegionMember[] Update_localMembers = new RegionMember[10];

		public RegionRegistry.RegionSetId setId { get; private set; }

		public bool Proxied => nonProxyRefCount == 0;

		public bool Hibernated => nonHibernateRefCount == 0;

		public event OnHibernationStateChanged onHibernationStateChanged;

		public void AddNonProxiedReference()
		{
			nonProxyRefCount++;
			if (nonProxyRefCount == 1)
			{
				Unproxy();
			}
		}

		public void RemoveNonProxiedReference()
		{
			nonProxyRefCount--;
			if (nonProxyRefCount <= 0)
			{
				nonProxyRefCount = 0;
				Proxy();
			}
		}

		public void AddNonHibernateReference()
		{
			nonHibernateRefCount++;
			if (nonHibernateRefCount == 1)
			{
				UpdateMembersHibernationStates();
			}
		}

		public void RemoveNonHibernateReference()
		{
			nonHibernateRefCount--;
			if (nonHibernateRefCount <= 0)
			{
				nonHibernateRefCount = 0;
				UpdateMembersHibernationStates();
			}
		}

		private void UpdateMembersHibernationStates()
		{
			if (members.Data.Length > Update_localMembers.Length)
			{
				Array.Resize(ref Update_localMembers, Math.Max(members.Data.Length, Update_localMembers.Length + 10));
			}
			int count = members.GetCount();
			members.Data.CopyTo(Update_localMembers, 0);
			for (int i = 0; i < count; i++)
			{
				Update_localMembers[i].UpdateHibernation();
			}
			if (this.onHibernationStateChanged != null)
			{
				this.onHibernationStateChanged(Hibernated);
			}
		}

		public void AddMember(RegionMember regionMember)
		{
			members.Add(regionMember);
		}

		public void RemoveMember(RegionMember regionMember)
		{
			members.Remove(regionMember);
		}

		public void CheckReferences()
		{
			if (nonHibernateRefCount <= 0)
			{
				nonHibernateRefCount = 0;
			}
		}

		public ZoneDirector.Zone GetZoneId()
		{
			if (cellDir != null)
			{
				return cellDir.GetZoneId();
			}
			return ZoneDirector.Zone.NONE;
		}

		public void OnRegionSetDeactivated()
		{
			if (proxyObj != null)
			{
				Destroyer.Destroy(proxyObj, "Region.OnRegionSetDeactivated");
				proxyObj = null;
			}
			nonProxyRefCount = 0;
			nonHibernateRefCount = 0;
			root.SetActive(value: false);
			UpdateMembersHibernationStates();
		}

		private void Proxy()
		{
			if (proxyMesh != null && regionReg.IsCurrRegionSet(setId))
			{
				CreateProxy();
			}
			root.SetActive(value: false);
		}

		private void CreateProxy()
		{
			if (proxyObj == null && proxyMesh != null)
			{
				proxyObj = new GameObject(base.name + " Proxy");
				proxyObj.AddComponent<MeshFilter>().sharedMesh = proxyMesh;
				MeshRenderer meshRenderer = proxyObj.AddComponent<MeshRenderer>();
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
				meshRenderer.sharedMaterials = proxyMaterials;
				proxyObj.transform.position = base.transform.position;
				proxyObj.transform.rotation = base.transform.rotation;
				proxyObj.transform.localScale = base.transform.lossyScale;
				proxyObj.transform.SetParent(base.transform, worldPositionStays: true);
			}
		}

		private void Unproxy()
		{
			if (proxyObj != null)
			{
				Destroyer.Destroy(proxyObj, "Region.Unproxy");
				proxyObj = null;
			}
			root.SetActive(value: true);
		}

		public void Awake()
		{
			if (Application.isPlaying)
			{
				cellDir = GetComponent<CellDirector>();
				regionReg = SRSingleton<SceneContext>.Instance.RegionRegistry;
				ZoneDirector[] componentsInParent = GetComponentsInParent<ZoneDirector>(includeInactive: true);
				setId = ZoneDirector.GetRegionSetId(componentsInParent[0].zone);
			}
		}

		public void OnEnable()
		{
			if (Application.isPlaying)
			{
				regionReg.RegisterRegion(this, setId, bounds);
			}
		}

		public void OnDisable()
		{
			if (Application.isPlaying)
			{
				regionReg.DeregisterRegion(this, setId);
			}
		}
	}
}
