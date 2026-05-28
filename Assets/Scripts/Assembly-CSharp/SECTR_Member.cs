using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Member")]
public class SECTR_Member : MonoBehaviour
{
	[Serializable]
	public class Child
	{
		public GameObject gameObject;

		public int gameObjectHash;

		public SECTR_Member member;

		public Renderer renderer;

		public int renderHash;

		public Light light;

		public int lightHash;

		public Terrain terrain;

		public int terrainHash;

		public Bounds rendererBounds;

		public Bounds lightBounds;

		public Bounds terrainBounds;

		public bool shadowLight;

		public bool rendererCastsShadows;

		public bool terrainCastsShadows;

		public LayerMask layer;

		public Vector3 shadowLightPosition;

		public float shadowLightRange;

		public LightType shadowLightType;

		public int shadowCullingMask;

		public void Init(GameObject gameObject, Renderer renderer, Light light, Terrain terrain, SECTR_Member member, bool dirShadowCaster, Vector3 shadowVec)
		{
			if (gameObject == null)
			{
				return;
			}
			this.gameObject = gameObject;
			gameObjectHash = this.gameObject.GetInstanceID();
			this.member = member;
			this.renderer = (((bool)renderer && renderer.enabled) ? renderer : null);
			this.light = (((bool)light && light.enabled && (light.type == LightType.Point || light.type == LightType.Spot)) ? light : null);
			this.terrain = (((bool)terrain && terrain.enabled) ? terrain : null);
			Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
			rendererBounds = (((bool)this.renderer && gameObject.transform.lossyScale.sqrMagnitude > Mathf.Epsilon) ? this.renderer.bounds : bounds);
			lightBounds = (((bool)this.light && gameObject.transform.lossyScale.sqrMagnitude > 0f) ? SECTR_Geometry.ComputeBounds(this.light) : bounds);
			terrainBounds = (((bool)this.terrain && gameObject.transform.lossyScale.sqrMagnitude > 0f) ? SECTR_Geometry.ComputeBounds(this.terrain) : bounds);
			layer = gameObject.layer;
			if (SECTR_Modules.VIS)
			{
				renderHash = (this.renderer ? this.renderer.GetInstanceID() : 0);
				lightHash = (this.light ? this.light.GetInstanceID() : 0);
				terrainHash = (this.terrain ? this.terrain.GetInstanceID() : 0);
				bool flag = !member.legacyBakeMode || LightmapSettings.lightmapsMode == LightmapsMode.CombinedDirectional;
				shadowLight = (bool)this.light && light.shadows != 0 && (!light.bakingOutput.isBaked || flag);
				rendererCastsShadows = this.renderer != null && renderer.shadowCastingMode != 0 && (renderer.lightmapIndex == -1 || flag);
				terrainCastsShadows = (bool)this.terrain && terrain.shadowCastingMode != 0 && (terrain.lightmapIndex == -1 || flag);
				if (dirShadowCaster)
				{
					if (rendererCastsShadows)
					{
						rendererBounds = SECTR_Geometry.ProjectBounds(rendererBounds, shadowVec);
					}
					if (terrainCastsShadows)
					{
						terrainBounds = SECTR_Geometry.ProjectBounds(terrainBounds, shadowVec);
					}
				}
				if (shadowLight)
				{
					shadowLightPosition = light.transform.position;
					shadowLightRange = light.range;
					shadowLightType = light.type;
					shadowCullingMask = light.cullingMask;
				}
				else
				{
					shadowLightPosition = Vector3.zero;
					shadowLightRange = 0f;
					shadowLightType = LightType.Area;
					shadowCullingMask = 0;
				}
			}
			else
			{
				renderHash = 0;
				lightHash = 0;
				terrainHash = 0;
				shadowLight = false;
				rendererCastsShadows = false;
				terrainCastsShadows = false;
				shadowLightPosition = Vector3.zero;
				shadowLightRange = 0f;
				shadowLightType = LightType.Area;
				shadowCullingMask = 0;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is Child)
			{
				return this == (Child)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return gameObjectHash;
		}

		public static bool operator ==(Child x, Child y)
		{
			return x.gameObjectHash == y.gameObjectHash;
		}

		public static bool operator !=(Child x, Child y)
		{
			return !(x == y);
		}
	}

	public enum BoundsUpdateModes
	{
		Start = 0,
		Movement = 1,
		Always = 2,
		Static = 3,
		Offset = 4
	}

	public enum ChildCullModes
	{
		Default = 0,
		Group = 1,
		Individual = 2
	}

	public delegate void MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined);

	[SerializeField]
	[HideInInspector]
	private List<Child> children = new List<Child>(16);

	[SerializeField]
	[HideInInspector]
	private List<Child> renderers = new List<Child>(16);

	[SerializeField]
	[HideInInspector]
	private List<Child> lights = new List<Child>(16);

	[SerializeField]
	[HideInInspector]
	private List<Child> terrains = new List<Child>(2);

	[SerializeField]
	[HideInInspector]
	private List<Child> shadowLights = (SECTR_Modules.VIS ? new List<Child>(16) : null);

	[SerializeField]
	[HideInInspector]
	private List<Child> shadowCasters = (SECTR_Modules.VIS ? new List<Child>(16) : null);

	[SerializeField]
	[HideInInspector]
	private Bounds totalBounds;

	[SerializeField]
	[HideInInspector]
	private Bounds renderBounds;

	[SerializeField]
	[HideInInspector]
	private Bounds lightBounds;

	[SerializeField]
	[HideInInspector]
	private bool hasRenderBounds;

	[SerializeField]
	[HideInInspector]
	private bool hasLightBounds;

	[SerializeField]
	[HideInInspector]
	private bool shadowCaster;

	[SerializeField]
	[HideInInspector]
	private bool shadowLight;

	[SerializeField]
	[HideInInspector]
	private bool frozen;

	[HideInInspector]
	public bool isFrozen;

	[SerializeField]
	[HideInInspector]
	private bool hibernate;

	[SerializeField]
	[HideInInspector]
	private bool neverJoin;

	[SerializeField]
	[HideInInspector]
	protected List<Light> bakedOnlyLights = (SECTR_Modules.VIS ? new List<Light>(8) : null);

	[SerializeField]
	[HideInInspector]
	protected bool legacyBakeMode;

	protected bool isSector;

	private bool started;

	private bool usedStartSector;

	[NonSerialized]
	public List<SECTR_Sector> sectors = new List<SECTR_Sector>(4);

	private List<SECTR_Sector> newSectors = new List<SECTR_Sector>(4);

	private List<SECTR_Sector> leftSectors = new List<SECTR_Sector>(4);

	private Dictionary<Light, Light> bakedOnlyTable;

	private SECTR_Member childProxy;

	private Vector3 lastPosition = Vector3.zero;

	private Stack<Child> childPool = new Stack<Child>(32);

	private static LightmapSettings lightmapSettings;

	private static List<SECTR_Member> allMembers = new List<SECTR_Member>(256);

	private Vector3? lastMembershipPos;

	[SECTR_ToolTip("Set to true if Sector membership should only change when crossing a portal.")]
	public bool PortalDetermined;

	[SECTR_ToolTip("If set, forces the initial Sector to be the specified Sector.", "PortalDetermined")]
	public SECTR_Sector ForceStartSector;

	[SECTR_ToolTip("Determines how often the bounds are recomputed. More frequent updates requires more CPU.")]
	public BoundsUpdateModes BoundsUpdateMode = BoundsUpdateModes.Always;

	[SECTR_ToolTip("Adds a buffer on bounding box to compensate for minor imprecisions.")]
	public float ExtraBounds = 0.01f;

	[SECTR_ToolTip("Override computed bounds with the user specified bounds. Advanced users only.")]
	public bool OverrideBounds;

	[SECTR_ToolTip("User specified override bounds. Auto-populated with the current bounds when override is inactive.", "OverrideBounds")]
	public Bounds BoundsOverride;

	[SECTR_ToolTip("Optional shadow casting directional light to use in membership calculations. Bounds will be extruded away from light, if set.")]
	public Light DirShadowCaster;

	[SECTR_ToolTip("Distance by which to extend the bounds away from the shadow casting light.", "DirShadowCaster")]
	public float DirShadowDistance = 100f;

	[SECTR_ToolTip("Determines if this SectorCuller should cull individual children, or cull all children based on the aggregate bounds.")]
	public ChildCullModes ChildCulling;

	[HideInInspector]
	public bool isHibernating;

	private SECTR_Hibernator memberHibernator;

	[NonSerialized]
	[HideInInspector]
	public Transform memberTransform;

	public static List<SECTR_Member> All => allMembers;

	public bool CullEachChild
	{
		get
		{
			if (ChildCulling != ChildCullModes.Individual)
			{
				if (ChildCulling == ChildCullModes.Default)
				{
					return isSector;
				}
				return false;
			}
			return true;
		}
	}

	public List<SECTR_Sector> Sectors => sectors;

	public List<Child> Children
	{
		get
		{
			if (!childProxy)
			{
				return children;
			}
			return childProxy.children;
		}
	}

	public List<Child> Renderers
	{
		get
		{
			if (!childProxy)
			{
				return renderers;
			}
			return childProxy.renderers;
		}
	}

	public bool ShadowCaster
	{
		get
		{
			if (!childProxy)
			{
				return shadowCaster;
			}
			return childProxy.shadowCaster;
		}
	}

	public List<Child> ShadowCasters
	{
		get
		{
			if (!childProxy)
			{
				return shadowCasters;
			}
			return childProxy.shadowCasters;
		}
	}

	public List<Child> Lights
	{
		get
		{
			if (!childProxy)
			{
				return lights;
			}
			return childProxy.lights;
		}
	}

	public bool ShadowLight
	{
		get
		{
			if (!childProxy)
			{
				return shadowLight;
			}
			return childProxy.shadowLight;
		}
	}

	public List<Child> ShadowLights
	{
		get
		{
			if (!childProxy)
			{
				return shadowLights;
			}
			return childProxy.shadowLights;
		}
	}

	public List<Child> Terrains
	{
		get
		{
			if (!childProxy)
			{
				return terrains;
			}
			return childProxy.terrains;
		}
	}

	public Bounds TotalBounds => totalBounds;

	public Bounds RenderBounds
	{
		get
		{
			if (!childProxy)
			{
				return renderBounds;
			}
			return childProxy.renderBounds;
		}
	}

	public bool HasRenderBounds
	{
		get
		{
			if (!childProxy)
			{
				return hasRenderBounds;
			}
			return childProxy.hasRenderBounds;
		}
	}

	public Bounds LightBounds
	{
		get
		{
			if (!childProxy)
			{
				return lightBounds;
			}
			return childProxy.lightBounds;
		}
	}

	public bool HasLightBounds
	{
		get
		{
			if (!childProxy)
			{
				return hasLightBounds;
			}
			return childProxy.hasLightBounds;
		}
	}

	public bool Frozen
	{
		get
		{
			return frozen;
		}
		set
		{
			if (isSector)
			{
				frozen = value;
				isFrozen = value;
			}
		}
	}

	public bool Hibernate
	{
		get
		{
			return hibernate;
		}
		set
		{
			if (isSector)
			{
				hibernate = value;
				isHibernating = value;
			}
		}
	}

	public SECTR_Member ChildProxy
	{
		set
		{
			childProxy = value;
		}
	}

	public bool NeverJoin
	{
		set
		{
			neverJoin = true;
		}
	}

	public bool IsSector => isSector;

	public bool IsHibernating
	{
		get
		{
			if ((object)memberHibernator != null)
			{
				return memberHibernator.isHibernating;
			}
			return false;
		}
	}

	public event MembershipChanged Changed;

	public void ForceUpdateBounds()
	{
		OffsetLateUpdate();
	}

	public void ForceUpdate(bool updateChildren, bool checkAllSectorSets = false)
	{
		if (updateChildren)
		{
			_UpdateChildren();
		}
		lastPosition = base.transform.position;
		if (!isSector && !neverJoin)
		{
			_UpdateSectorMembership(checkAllSectorSets);
		}
	}

	public void SectorDisabled(SECTR_Sector sector)
	{
		if ((bool)sector)
		{
			sectors.Remove(sector);
			if (this.Changed != null)
			{
				leftSectors.Clear();
				leftSectors.Add(sector);
				this.Changed(leftSectors, null);
			}
		}
	}

	public virtual void Start()
	{
		started = true;
		ForceUpdate(updateChildren: true, checkAllSectorSets: true);
	}

	protected virtual void OnEnable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null && GetComponent<SECTR_Hibernator>() == null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.RegisterMember(this);
		}
		allMembers.Add(this);
		if (bakedOnlyLights != null)
		{
			int count = bakedOnlyLights.Count;
			bakedOnlyTable = new Dictionary<Light, Light>(count);
			for (int i = 0; i < count; i++)
			{
				Light light = bakedOnlyLights[i];
				if ((bool)light)
				{
					bakedOnlyTable[light] = light;
				}
			}
		}
		if (started && Application.isPlaying)
		{
			ForceUpdate(updateChildren: true, checkAllSectorSets: true);
		}
	}

	protected virtual void OnDisable()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null && GetComponent<SECTR_Hibernator>() == null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.DeregisterMember(this);
		}
		if (this.Changed != null && sectors.Count > 0)
		{
			this.Changed(sectors, null);
		}
		if (!isSector && !neverJoin)
		{
			int count = sectors.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_Sector sECTR_Sector = sectors[i];
				if ((bool)sECTR_Sector)
				{
					sECTR_Sector.Deregister(this);
				}
			}
			sectors.Clear();
		}
		bakedOnlyTable = null;
		allMembers.Remove(this);
	}

	private void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.SECTRDirector != null)
		{
			SRSingleton<SceneContext>.Instance.SECTRDirector.DeregisterMember(this);
		}
	}

	public virtual void Awake()
	{
		memberTransform = base.transform;
		memberHibernator = GetComponent<SECTR_Hibernator>();
	}

	public virtual void OffsetLateUpdate()
	{
		if (!isSector && !neverJoin && (!lastMembershipPos.HasValue || (base.transform.localPosition - lastMembershipPos.Value).sqrMagnitude > 1f))
		{
			if (lastMembershipPos.HasValue)
			{
				Vector3 vector = base.transform.localPosition - lastMembershipPos.Value;
				if (vector.x != 0f || vector.y != 0f || vector.z != 0f)
				{
					totalBounds.center += vector;
					renderBounds.center += vector;
					lightBounds.center += vector;
				}
			}
			_UpdateSectorMembership();
			lastMembershipPos = base.transform.localPosition;
		}
		lastPosition = base.transform.localPosition;
	}

	public virtual void NonOffsetLateUpdate()
	{
		_UpdateChildren();
		Vector3 position = base.transform.position;
		if (!isSector && !neverJoin)
		{
			_UpdateSectorMembership();
			lastMembershipPos = position;
		}
		if (!isSector && !neverJoin && (!lastMembershipPos.HasValue || (position - lastMembershipPos.Value).sqrMagnitude > 1f))
		{
			_UpdateSectorMembership();
			lastMembershipPos = position;
		}
		lastPosition = position;
	}

	private void _UpdateChildren()
	{
		if (frozen || (bool)childProxy)
		{
			return;
		}
		bool flag = SECTR_Modules.VIS && (bool)DirShadowCaster && DirShadowCaster.type == LightType.Directional && DirShadowCaster.shadows != LightShadows.None;
		Vector3 shadowVec = (flag ? (DirShadowCaster.transform.forward * DirShadowDistance) : Vector3.zero);
		int count = children.Count;
		hasLightBounds = false;
		hasRenderBounds = false;
		shadowCaster = false;
		shadowLight = false;
		renderers.Clear();
		lights.Clear();
		terrains.Clear();
		if (SECTR_Modules.VIS)
		{
			shadowCasters.Clear();
			shadowLights.Clear();
		}
		if ((BoundsUpdateMode == BoundsUpdateModes.Start || BoundsUpdateMode == BoundsUpdateModes.Offset) && count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				Child child = children[i];
				child.Init(child.gameObject, child.renderer, child.light, child.terrain, child.member, flag, shadowVec);
				Renderer renderer = child.renderer;
				if (renderer != null && !(renderer is ParticleSystemRenderer))
				{
					if (!hasRenderBounds)
					{
						renderBounds = child.rendererBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.rendererBounds);
					}
					renderers.Add(child);
				}
				if ((bool)child.terrain)
				{
					if (!hasRenderBounds)
					{
						renderBounds = child.terrainBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.terrainBounds);
					}
					terrains.Add(child);
				}
				if ((bool)child.light)
				{
					if (SECTR_Modules.VIS && child.shadowLight)
					{
						shadowLights.Add(child);
						shadowLight = true;
					}
					if (!hasLightBounds)
					{
						lightBounds = child.lightBounds;
						hasLightBounds = true;
					}
					else
					{
						lightBounds.Encapsulate(child.lightBounds);
					}
					lights.Add(child);
				}
				if (SECTR_Modules.VIS && (child.terrainCastsShadows || child.rendererCastsShadows))
				{
					shadowCasters.Add(child);
					shadowCaster = true;
				}
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				childPool.Push(children[j]);
			}
			children.Clear();
			_AddChildren(base.transform, flag, shadowVec);
		}
		lastPosition = base.transform.position;
		Bounds bounds = new Bounds(base.transform.position, Vector3.zero);
		if (hasRenderBounds && (isSector || neverJoin))
		{
			totalBounds = renderBounds;
		}
		else if (hasRenderBounds && hasLightBounds)
		{
			totalBounds = renderBounds;
			totalBounds.Encapsulate(lightBounds);
		}
		else if (hasRenderBounds)
		{
			totalBounds = renderBounds;
			lightBounds = bounds;
		}
		else if (hasLightBounds)
		{
			totalBounds = lightBounds;
			renderBounds = bounds;
		}
		else
		{
			totalBounds = bounds;
			lightBounds = bounds;
			renderBounds = bounds;
		}
		totalBounds.Expand(ExtraBounds);
		if (isSector)
		{
			totalBounds.max += Vector3.up * 160f;
		}
		if (OverrideBounds)
		{
			totalBounds = BoundsOverride;
		}
	}

	private void _AddChildren(Transform childTransform, bool dirShadowCaster, Vector3 shadowVec)
	{
		if (!childTransform.gameObject.activeSelf || (!(childTransform == base.transform) && !(childTransform.GetComponent<SECTR_Member>() == null)) || !(childTransform.GetComponent<IgnoreSectrBounds>() == null))
		{
			return;
		}
		Light light = childTransform.GetComponent<Light>();
		Renderer component = childTransform.GetComponent<Renderer>();
		Terrain terrain = null;
		if (isSector)
		{
			terrain = childTransform.GetComponent<Terrain>();
		}
		if (bakedOnlyLights != null && (bool)light && light.bakingOutput.isBaked && LightmapSettings.lightmaps.Length != 0 && bakedOnlyTable != null && bakedOnlyTable.ContainsKey(light))
		{
			light = null;
		}
		if ((bool)component || (bool)light || (bool)terrain)
		{
			Child child = ((childPool.Count <= 0) ? new Child() : childPool.Pop());
			child.Init(childTransform.gameObject, component, light, terrain, this, dirShadowCaster, shadowVec);
			if ((bool)child.renderer)
			{
				bool flag = true;
				if (component.GetType() == typeof(ParticleSystemRenderer))
				{
					flag = false;
				}
				if (flag)
				{
					if (!hasRenderBounds)
					{
						renderBounds = child.rendererBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.rendererBounds);
					}
				}
				renderers.Add(child);
			}
			if ((bool)child.light)
			{
				if (SECTR_Modules.VIS && child.shadowLight)
				{
					shadowLights.Add(child);
					shadowLight = true;
				}
				if (!hasLightBounds)
				{
					lightBounds = child.lightBounds;
					hasLightBounds = true;
				}
				else
				{
					lightBounds.Encapsulate(child.lightBounds);
				}
				lights.Add(child);
			}
			if ((bool)child.terrain)
			{
				if (!hasRenderBounds)
				{
					renderBounds = child.terrainBounds;
					hasRenderBounds = true;
				}
				else
				{
					renderBounds.Encapsulate(child.terrainBounds);
				}
				terrains.Add(child);
			}
			if (SECTR_Modules.VIS && (child.terrainCastsShadows || child.rendererCastsShadows))
			{
				shadowCasters.Add(child);
				shadowCaster = true;
			}
			children.Add(child);
		}
		int childCount = childTransform.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			_AddChildren(childTransform.GetChild(i), dirShadowCaster, shadowVec);
		}
	}

	private void _UpdateSectorMembership(bool checkAllSectorSets = false)
	{
		if (frozen || isSector || neverJoin)
		{
			return;
		}
		newSectors.Clear();
		leftSectors.Clear();
		if (PortalDetermined && sectors.Count > 0)
		{
			int count = sectors.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_Sector sECTR_Sector = sectors[i];
				SECTR_Portal sECTR_Portal = _CrossedPortal(sECTR_Sector);
				if ((bool)sECTR_Portal)
				{
					SECTR_Sector item = ((sECTR_Portal.FrontSector == sECTR_Sector) ? sECTR_Portal.BackSector : sECTR_Portal.FrontSector);
					if (!newSectors.Contains(item))
					{
						newSectors.Add(item);
					}
					leftSectors.Add(sECTR_Sector);
				}
			}
			count = newSectors.Count;
			for (int j = 0; j < count; j++)
			{
				SECTR_Sector sECTR_Sector2 = newSectors[j];
				sECTR_Sector2.Register(this);
				sectors.Add(sECTR_Sector2);
			}
			count = leftSectors.Count;
			for (int k = 0; k < count; k++)
			{
				SECTR_Sector sECTR_Sector3 = leftSectors[k];
				sECTR_Sector3.Deregister(this);
				sectors.Remove(sECTR_Sector3);
			}
		}
		else if (PortalDetermined && (bool)ForceStartSector && !usedStartSector)
		{
			ForceStartSector.Register(this);
			sectors.Add(ForceStartSector);
			newSectors.Add(ForceStartSector);
			usedStartSector = true;
		}
		else
		{
			SECTR_Sector.GetContaining(ref newSectors, TotalBounds, checkAllSectorSets);
			int num = 0;
			int num2 = sectors.Count;
			while (num < num2)
			{
				SECTR_Sector sECTR_Sector4 = sectors[num];
				if (newSectors.Contains(sECTR_Sector4))
				{
					newSectors.Remove(sECTR_Sector4);
					num++;
					continue;
				}
				sECTR_Sector4.Deregister(this);
				leftSectors.Add(sECTR_Sector4);
				sectors.RemoveAt(num);
				num2--;
			}
			num2 = newSectors.Count;
			if (num2 > 0)
			{
				for (num = 0; num < num2; num++)
				{
					SECTR_Sector sECTR_Sector5 = newSectors[num];
					sECTR_Sector5.Register(this);
					sectors.Add(sECTR_Sector5);
				}
			}
		}
		if (this.Changed != null && (leftSectors.Count > 0 || newSectors.Count > 0))
		{
			this.Changed(leftSectors, newSectors);
		}
	}

	private SECTR_Portal _CrossedPortal(SECTR_Sector sector)
	{
		if ((bool)sector)
		{
			Vector3 lhs = base.transform.position - lastPosition;
			int count = sector.Portals.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_Portal sECTR_Portal = sector.Portals[i];
				if ((bool)sECTR_Portal)
				{
					bool num = sECTR_Portal.FrontSector == sector;
					Plane plane = (num ? sECTR_Portal.HullPlane : sECTR_Portal.ReverseHullPlane);
					if ((bool)(num ? sECTR_Portal.BackSector : sECTR_Portal.FrontSector) && Vector3.Dot(lhs, plane.normal) < 0f && plane.GetSide(base.transform.position) != plane.GetSide(lastPosition) && sECTR_Portal.IsPointInHull(base.transform.position, lhs.magnitude))
					{
						return sECTR_Portal;
					}
				}
			}
		}
		return null;
	}
}
