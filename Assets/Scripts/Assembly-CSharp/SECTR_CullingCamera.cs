using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("SECTR/Vis/SECTR Culling Camera")]
public class SECTR_CullingCamera : MonoBehaviour
{
	private struct VisibilityNode
	{
		public SECTR_Sector sector;

		public SECTR_Portal portal;

		public List<Plane> frustumPlanes;

		public bool forwardTraversal;

		public VisibilityNode(SECTR_CullingCamera cullingCamera, SECTR_Sector sector, SECTR_Portal portal, Plane[] frustumPlanes, bool forwardTraversal)
		{
			this.sector = sector;
			this.portal = portal;
			if (frustumPlanes == null)
			{
				this.frustumPlanes = null;
			}
			else if (cullingCamera.frustumPool.Count > 0)
			{
				this.frustumPlanes = cullingCamera.frustumPool.Pop();
				this.frustumPlanes.AddRange(frustumPlanes);
			}
			else
			{
				this.frustumPlanes = new List<Plane>(frustumPlanes);
			}
			this.forwardTraversal = forwardTraversal;
		}

		public VisibilityNode(SECTR_CullingCamera cullingCamera, SECTR_Sector sector, SECTR_Portal portal, List<Plane> frustumPlanes, bool forwardTraversal)
		{
			this.sector = sector;
			this.portal = portal;
			if (frustumPlanes == null)
			{
				this.frustumPlanes = null;
			}
			else if (cullingCamera.frustumPool.Count > 0)
			{
				this.frustumPlanes = cullingCamera.frustumPool.Pop();
				this.frustumPlanes.AddRange(frustumPlanes);
			}
			else
			{
				this.frustumPlanes = new List<Plane>(frustumPlanes);
			}
			this.forwardTraversal = forwardTraversal;
		}
	}

	private struct ClipVertex
	{
		public Vector4 vertex;

		public float side;

		public ClipVertex(Vector4 vertex, float side)
		{
			this.vertex = vertex;
			this.side = side;
		}
	}

	private struct ThreadCullData
	{
		public enum CullingModes
		{
			None = 0,
			Graph = 1,
			Shadow = 2
		}

		public SECTR_Sector sector;

		public Vector3 cameraPos;

		public List<Plane> cullingPlanes;

		public List<List<Plane>> occluderFrustums;

		public int baseMask;

		public float shadowDistance;

		public bool cullingSimpleCulling;

		public List<SECTR_Member.Child> sectorShadowLights;

		public CullingModes cullingMode;

		public ThreadCullData(SECTR_Sector sector, SECTR_CullingCamera cullingCamera, Vector3 cameraPos, List<Plane> cullingPlanes, List<List<Plane>> occluderFrustums, int baseMask, float shadowDistance, bool cullingSimpleCulling)
		{
			this.sector = sector;
			this.cameraPos = cameraPos;
			this.baseMask = baseMask;
			this.shadowDistance = shadowDistance;
			this.cullingSimpleCulling = cullingSimpleCulling;
			sectorShadowLights = null;
			lock (cullingCamera.threadOccluderPool)
			{
				this.occluderFrustums = ((cullingCamera.threadOccluderPool.Count > 0) ? cullingCamera.threadOccluderPool.Pop() : new List<List<Plane>>(occluderFrustums.Count));
			}
			lock (cullingCamera.threadFrustumPool)
			{
				if (cullingCamera.threadFrustumPool.Count > 0)
				{
					this.cullingPlanes = cullingCamera.threadFrustumPool.Pop();
					this.cullingPlanes.AddRange(cullingPlanes);
				}
				else
				{
					this.cullingPlanes = new List<Plane>(cullingPlanes);
				}
				int count = occluderFrustums.Count;
				for (int i = 0; i < count; i++)
				{
					List<Plane> list = null;
					if (cullingCamera.threadFrustumPool.Count > 0)
					{
						list = cullingCamera.threadFrustumPool.Pop();
						list.AddRange(occluderFrustums[i]);
					}
					else
					{
						list = new List<Plane>(occluderFrustums[i]);
					}
					this.occluderFrustums.Add(list);
				}
			}
			cullingMode = CullingModes.Graph;
		}

		public ThreadCullData(SECTR_Sector sector, Vector3 cameraPos, List<SECTR_Member.Child> sectorShadowLights)
		{
			this.sector = sector;
			this.cameraPos = cameraPos;
			cullingPlanes = null;
			occluderFrustums = null;
			baseMask = 0;
			shadowDistance = 0f;
			cullingSimpleCulling = false;
			this.sectorShadowLights = sectorShadowLights;
			cullingMode = CullingModes.Shadow;
		}
	}

	private List<Renderer> hiddenRenderers = new List<Renderer>(16);

	private List<Light> hiddenLights = new List<Light>(16);

	private List<Terrain> hiddenTerrains = new List<Terrain>(2);

	private int renderersCulled;

	private int lightsCulled;

	private int terrainsCulled;

	private bool didCull;

	private List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);

	private Stack<VisibilityNode> nodeStack = new Stack<VisibilityNode>(10);

	private List<ClipVertex> portalVertices = new List<ClipVertex>(16);

	private List<Plane> newFrustum = new List<Plane>(16);

	private List<Plane> cullingPlanes = new List<Plane>(16);

	private List<List<Plane>> occluderFrustums = new List<List<Plane>>(10);

	private Dictionary<SECTR_Occluder, SECTR_Occluder> activeOccluders = new Dictionary<SECTR_Occluder, SECTR_Occluder>(10);

	private List<ClipVertex> occluderVerts = new List<ClipVertex>(10);

	private Dictionary<SECTR_Member.Child, int> shadowLights = new Dictionary<SECTR_Member.Child, int>(10);

	private List<SECTR_Sector> shadowSectors = new List<SECTR_Sector>(4);

	private Dictionary<SECTR_Sector, List<SECTR_Member.Child>> shadowSectorTable = new Dictionary<SECTR_Sector, List<SECTR_Member.Child>>(4);

	private Dictionary<int, int> visibleRenderers = new Dictionary<int, int>(1024);

	private Dictionary<int, int> visibleLights = new Dictionary<int, int>(256);

	private Dictionary<int, int> visibleTerrains = new Dictionary<int, int>(32);

	private Stack<List<Plane>> frustumPool = new Stack<List<Plane>>(32);

	private Stack<List<SECTR_Member.Child>> shadowLightPool = new Stack<List<SECTR_Member.Child>>(32);

	private Stack<Dictionary<int, int>> threadVisibleListPool = new Stack<Dictionary<int, int>>(32);

	private Stack<Dictionary<SECTR_Member.Child, int>> threadShadowLightPool = new Stack<Dictionary<SECTR_Member.Child, int>>(32);

	private Stack<List<Plane>> threadFrustumPool = new Stack<List<Plane>>(32);

	private Stack<List<List<Plane>>> threadOccluderPool = new Stack<List<List<Plane>>>(32);

	private List<Thread> workerThreads = new List<Thread>();

	private Queue<ThreadCullData> cullingWorkQueue = new Queue<ThreadCullData>(32);

	private int remainingThreadWork;

	private static List<SECTR_CullingCamera> allCullingCameras = new List<SECTR_CullingCamera>(4);

	[SECTR_ToolTip("Forces culling into a mode designed for 2D and iso games where the camera is always outside the scene.")]
	public bool SimpleCulling;

	[SECTR_ToolTip("The layer that culled objects should be assigned to.", false, true)]
	[HideInInspector]
	public int InvisibleLayer;

	[SECTR_ToolTip("Distance to draw clipped frustums.", 0f, 100f)]
	public float GizmoDistance = 10f;

	[SECTR_ToolTip("Material to use to render the debug frustum mesh.")]
	public Material GizmoMaterial;

	[SECTR_ToolTip("Makes the Editor camera display the Game view's culling while playing in editor.")]
	public bool CullInEditor;

	[SECTR_ToolTip("Set to false to disable shadow culling post pass.", true)]
	public bool CullShadows = true;

	[SECTR_ToolTip("Use another camera for culling properties.", true)]
	public Camera cullingProxy;

	[SECTR_ToolTip("Number of worker threads for culling. Do not set this too high or you may see hitching.", 0f, -1f)]
	public int NumWorkerThreads;

	public static List<SECTR_CullingCamera> All => allCullingCameras;

	public Camera CullingCamera
	{
		set
		{
			cullingProxy = value;
		}
	}

	public int RenderersCulled => renderersCulled;

	public int LightsCulled => lightsCulled;

	public int TerrainsCulled => terrainsCulled;

	public void ResetStats()
	{
		renderersCulled = 0;
		lightsCulled = 0;
		terrainsCulled = 0;
	}

	private void OnEnable()
	{
		allCullingCameras.Add(this);
		int num = Mathf.Min(NumWorkerThreads, SystemInfo.processorCount);
		for (int i = 0; i < num; i++)
		{
			Thread thread = new Thread(_CullingWorker);
			thread.IsBackground = true;
			thread.Priority = System.Threading.ThreadPriority.Highest;
			thread.Start();
			workerThreads.Add(thread);
		}
	}

	private void OnDisable()
	{
		allCullingCameras.Remove(this);
		int count = workerThreads.Count;
		for (int i = 0; i < count; i++)
		{
			workerThreads[i].Abort();
		}
	}

	private void OnDestroy()
	{
	}

	private void OnPreCull()
	{
		Camera camera = ((cullingProxy != null) ? cullingProxy : GetComponent<Camera>());
		Vector3 position = camera.transform.position;
		float num = Mathf.Cos(Mathf.Max(camera.fieldOfView, camera.fieldOfView * camera.aspect) * 0.5f * ((float)Math.PI / 180f));
		float num2 = camera.nearClipPlane / num * 1.001f;
		int invisibleLayer = InvisibleLayer;
		bool simpleCulling = SimpleCulling;
		if ((bool)cullingProxy)
		{
			SECTR_CullingCamera component = cullingProxy.GetComponent<SECTR_CullingCamera>();
			if ((bool)component)
			{
				invisibleLayer = component.InvisibleLayer;
				simpleCulling = component.SimpleCulling;
			}
		}
		int count = SECTR_LOD.All.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_LOD.All[i].SelectLOD(camera);
		}
		SECTR_Member component2 = GetComponent<SECTR_Member>();
		if (simpleCulling)
		{
			initialSectors.Clear();
			initialSectors.AddRange(SECTR_Sector.All);
		}
		else if ((bool)component2 && component2.enabled)
		{
			initialSectors.Clear();
			initialSectors.AddRange(component2.Sectors);
		}
		else
		{
			SECTR_Sector.GetContaining(ref initialSectors, new Bounds(position, new Vector3(num2, num2, num2)));
		}
		int count2 = initialSectors.Count;
		if (!base.enabled || !camera.enabled || count2 <= 0)
		{
			return;
		}
		didCull = true;
		int count3 = workerThreads.Count;
		float shadowDistance = QualitySettings.shadowDistance;
		int count4 = SECTR_Member.All.Count;
		for (int j = 0; j < count4; j++)
		{
			SECTR_Member sECTR_Member = SECTR_Member.All[j];
			if (!sECTR_Member.ShadowLight)
			{
				continue;
			}
			int count5 = sECTR_Member.ShadowLights.Count;
			for (int k = 0; k < count5; k++)
			{
				SECTR_Member.Child child = sECTR_Member.ShadowLights[k];
				if ((bool)child.light)
				{
					child.shadowLightPosition = child.light.transform.position;
					child.shadowLightRange = child.light.range;
				}
				sECTR_Member.ShadowLights[k] = child;
			}
		}
		nodeStack.Clear();
		shadowLights.Clear();
		visibleRenderers.Clear();
		visibleLights.Clear();
		visibleTerrains.Clear();
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
		for (int l = 0; l < count2; l++)
		{
			SECTR_Sector sector = initialSectors[l];
			nodeStack.Push(new VisibilityNode(this, sector, null, array, forwardTraversal: true));
		}
		while (nodeStack.Count > 0)
		{
			VisibilityNode visibilityNode = nodeStack.Pop();
			if (visibilityNode.frustumPlanes != null)
			{
				cullingPlanes.Clear();
				cullingPlanes.AddRange(visibilityNode.frustumPlanes);
				int count6 = cullingPlanes.Count;
				for (int m = 0; m < count6; m++)
				{
					Plane plane = cullingPlanes[m];
					Plane plane2 = cullingPlanes[(m + 1) % cullingPlanes.Count];
					float num3 = Vector3.Dot(plane.normal, plane2.normal);
					if (num3 < -0.9f && num3 > -0.99f)
					{
						Vector3 vector = plane.normal + plane2.normal;
						Vector3 vector2 = Vector3.Cross(plane.normal, plane2.normal);
						Vector3 inNormal = vector - Vector3.Dot(vector, vector2) * vector2;
						inNormal.Normalize();
						Matrix4x4 matrix4x = default(Matrix4x4);
						matrix4x.SetRow(0, new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, 0f));
						matrix4x.SetRow(1, new Vector4(plane2.normal.x, plane2.normal.y, plane2.normal.z, 0f));
						matrix4x.SetRow(2, new Vector4(vector2.x, vector2.y, vector2.z, 0f));
						matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
						Vector3 inPoint = matrix4x.inverse.MultiplyPoint3x4(new Vector3(0f - plane.distance, 0f - plane2.distance, 0f));
						cullingPlanes.Insert(++m, new Plane(inNormal, inPoint));
					}
				}
				count6 = cullingPlanes.Count;
				int num4 = 0;
				for (int n = 0; n < count6; n++)
				{
					num4 |= 1 << n;
				}
				SECTR_Sector sector2 = visibilityNode.sector;
				if (SECTR_Occluder.All.Count > 0)
				{
					List<SECTR_Occluder> occludersInSector = SECTR_Occluder.GetOccludersInSector(sector2);
					if (occludersInSector != null)
					{
						int count7 = occludersInSector.Count;
						for (int num5 = 0; num5 < count7; num5++)
						{
							SECTR_Occluder sECTR_Occluder = occludersInSector[num5];
							if (!sECTR_Occluder.HullMesh || activeOccluders.ContainsKey(sECTR_Occluder))
							{
								continue;
							}
							Matrix4x4 cullingMatrix = sECTR_Occluder.GetCullingMatrix(position);
							Vector3[] vertsCW = sECTR_Occluder.VertsCW;
							Vector3 normalized = cullingMatrix.MultiplyVector(-sECTR_Occluder.MeshNormal).normalized;
							if (vertsCW == null || SECTR_Geometry.IsPointInFrontOfPlane(position, sECTR_Occluder.Center, normalized))
							{
								continue;
							}
							int num6 = vertsCW.Length;
							occluderVerts.Clear();
							Bounds bounds = new Bounds(sECTR_Occluder.transform.position, Vector3.zero);
							for (int num7 = 0; num7 < num6; num7++)
							{
								Vector3 point = cullingMatrix.MultiplyPoint3x4(vertsCW[num7]);
								bounds.Encapsulate(point);
								occluderVerts.Add(new ClipVertex(new Vector4(point.x, point.y, point.z, 1f), 0f));
							}
							if (SECTR_Geometry.FrustumIntersectsBounds(sECTR_Occluder.BoundingBox, cullingPlanes, num4, out var _))
							{
								List<Plane> list;
								if (frustumPool.Count > 0)
								{
									list = frustumPool.Pop();
									list.Clear();
								}
								else
								{
									list = new List<Plane>(num6 + 1);
								}
								_BuildFrustumFromHull(camera, forwardTraversal: true, occluderVerts, ref list);
								list.Add(new Plane(normalized, sECTR_Occluder.Center));
								occluderFrustums.Add(list);
								activeOccluders[sECTR_Occluder] = sECTR_Occluder;
							}
						}
					}
				}
				if (count3 > 0)
				{
					lock (cullingWorkQueue)
					{
						cullingWorkQueue.Enqueue(new ThreadCullData(sector2, this, position, cullingPlanes, occluderFrustums, num4, shadowDistance, simpleCulling));
						Monitor.Pulse(cullingWorkQueue);
					}
					Interlocked.Increment(ref remainingThreadWork);
				}
				else
				{
					_FrustumCullSector(sector2, position, cullingPlanes, occluderFrustums, num4, shadowDistance, simpleCulling, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
				}
				int num8 = ((!simpleCulling) ? visibilityNode.sector.Portals.Count : 0);
				for (int num9 = 0; num9 < num8; num9++)
				{
					SECTR_Portal sECTR_Portal = visibilityNode.sector.Portals[num9];
					bool flag = (sECTR_Portal.Flags & SECTR_Portal.PortalFlags.PassThrough) != 0;
					if (!((bool)sECTR_Portal.HullMesh || flag) || (sECTR_Portal.Flags & SECTR_Portal.PortalFlags.Closed) != 0)
					{
						continue;
					}
					bool flag2 = visibilityNode.sector == sECTR_Portal.FrontSector;
					SECTR_Sector sECTR_Sector = (flag2 ? sECTR_Portal.BackSector : sECTR_Portal.FrontSector);
					bool flag3 = !sECTR_Sector;
					if (!flag3)
					{
						flag3 = SECTR_Geometry.IsPointInFrontOfPlane(position, sECTR_Portal.Center, sECTR_Portal.Normal) != flag2;
					}
					if (!flag3 && (bool)visibilityNode.portal)
					{
						Vector3 normalized2 = (sECTR_Portal.Center - visibilityNode.portal.Center).normalized;
						Vector3 rhs = (visibilityNode.forwardTraversal ? visibilityNode.portal.ReverseNormal : visibilityNode.portal.Normal);
						flag3 = Vector3.Dot(normalized2, rhs) < 0f;
					}
					if (!flag3 && !flag)
					{
						int count8 = occluderFrustums.Count;
						for (int num10 = 0; num10 < count8; num10++)
						{
							if (SECTR_Geometry.FrustumContainsBounds(sECTR_Portal.BoundingBox, occluderFrustums[num10]))
							{
								flag3 = true;
								break;
							}
						}
					}
					if (flag3)
					{
						continue;
					}
					if (!flag)
					{
						portalVertices.Clear();
						Matrix4x4 localToWorldMatrix = sECTR_Portal.transform.localToWorldMatrix;
						Vector3[] array2 = (flag2 ? sECTR_Portal.VertsCCW : sECTR_Portal.VertsCW);
						if (array2 != null)
						{
							int num11 = array2.Length;
							for (int num12 = 0; num12 < num11; num12++)
							{
								Vector3 vector3 = localToWorldMatrix.MultiplyPoint3x4(array2[num12]);
								portalVertices.Add(new ClipVertex(new Vector4(vector3.x, vector3.y, vector3.z, 1f), 0f));
							}
						}
					}
					newFrustum.Clear();
					if (!flag && !sECTR_Portal.IsPointInHull(position, num2))
					{
						int count9 = visibilityNode.frustumPlanes.Count;
						for (int num13 = 0; num13 < count9; num13++)
						{
							Plane plane3 = visibilityNode.frustumPlanes[num13];
							Vector4 a = new Vector4(plane3.normal.x, plane3.normal.y, plane3.normal.z, plane3.distance);
							bool flag4 = true;
							bool flag5 = true;
							for (int num14 = 0; num14 < portalVertices.Count; num14++)
							{
								Vector4 vertex = portalVertices[num14].vertex;
								float num15 = Vector4.Dot(a, vertex);
								portalVertices[num14] = new ClipVertex(vertex, num15);
								flag4 = flag4 && num15 > 0f;
								flag5 = flag5 && num15 <= -0.001f;
							}
							if (flag5)
							{
								portalVertices.Clear();
								break;
							}
							if (flag4)
							{
								continue;
							}
							int num16 = portalVertices.Count;
							for (int num17 = 0; num17 < num16; num17++)
							{
								int index = (num17 + 1) % portalVertices.Count;
								float side = portalVertices[num17].side;
								float side2 = portalVertices[index].side;
								if ((side > 0f && side2 <= -0.001f) || (side2 > 0f && side <= -0.001f))
								{
									Vector4 vertex2 = portalVertices[num17].vertex;
									Vector4 vertex3 = portalVertices[index].vertex;
									float num18 = side / Vector4.Dot(a, vertex2 - vertex3);
									Vector4 vertex4 = vertex2 + num18 * (vertex3 - vertex2);
									vertex4.w = 1f;
									portalVertices.Insert(num17 + 1, new ClipVertex(vertex4, 0f));
									num16++;
								}
							}
							int num19 = 0;
							while (num19 < num16)
							{
								if (portalVertices[num19].side < -0.001f)
								{
									portalVertices.RemoveAt(num19);
									num16--;
								}
								else
								{
									num19++;
								}
							}
						}
						_BuildFrustumFromHull(camera, flag2, portalVertices, ref newFrustum);
					}
					else
					{
						newFrustum.AddRange(array);
					}
					if (newFrustum.Count > 2)
					{
						nodeStack.Push(new VisibilityNode(this, sECTR_Sector, sECTR_Portal, newFrustum, flag2));
					}
				}
			}
			if (visibilityNode.frustumPlanes != null)
			{
				visibilityNode.frustumPlanes.Clear();
				frustumPool.Push(visibilityNode.frustumPlanes);
			}
		}
		if (count3 > 0)
		{
			while (remainingThreadWork > 0)
			{
				while (cullingWorkQueue.Count > 0)
				{
					ThreadCullData cullData = default(ThreadCullData);
					lock (cullingWorkQueue)
					{
						if (cullingWorkQueue.Count > 0)
						{
							cullData = cullingWorkQueue.Dequeue();
						}
					}
					if (cullData.cullingMode == ThreadCullData.CullingModes.Graph)
					{
						_FrustumCullSectorThread(cullData);
						Interlocked.Decrement(ref remainingThreadWork);
					}
				}
			}
			remainingThreadWork = 0;
		}
		if (shadowLights.Count > 0 && CullShadows)
		{
			shadowSectorTable.Clear();
			Dictionary<SECTR_Member.Child, int>.Enumerator enumerator = shadowLights.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SECTR_Member.Child key = enumerator.Current.Key;
				List<SECTR_Sector> sectors;
				if ((bool)key.member && key.member.IsSector)
				{
					shadowSectors.Clear();
					shadowSectors.Add((SECTR_Sector)key.member);
					sectors = shadowSectors;
				}
				else if ((bool)key.member && key.member.Sectors.Count > 0)
				{
					sectors = key.member.Sectors;
				}
				else
				{
					SECTR_Sector.GetContaining(ref shadowSectors, key.lightBounds);
					sectors = shadowSectors;
				}
				int count10 = sectors.Count;
				for (int num20 = 0; num20 < count10; num20++)
				{
					SECTR_Sector key2 = sectors[num20];
					if (!shadowSectorTable.TryGetValue(key2, out var value))
					{
						value = ((shadowLightPool.Count > 0) ? shadowLightPool.Pop() : new List<SECTR_Member.Child>(16));
						shadowSectorTable[key2] = value;
					}
					value.Add(key);
				}
			}
			Dictionary<SECTR_Sector, List<SECTR_Member.Child>>.Enumerator enumerator2 = shadowSectorTable.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				SECTR_Sector key3 = enumerator2.Current.Key;
				List<SECTR_Member.Child> value2 = enumerator2.Current.Value;
				if (count3 > 0)
				{
					lock (cullingWorkQueue)
					{
						cullingWorkQueue.Enqueue(new ThreadCullData(key3, position, value2));
						Monitor.Pulse(cullingWorkQueue);
					}
					Interlocked.Increment(ref remainingThreadWork);
				}
				else
				{
					_ShadowCullSector(key3, value2, ref visibleRenderers, ref visibleTerrains);
				}
			}
			if (count3 > 0)
			{
				while (remainingThreadWork > 0)
				{
					while (cullingWorkQueue.Count > 0)
					{
						ThreadCullData cullData2 = default(ThreadCullData);
						lock (cullingWorkQueue)
						{
							if (cullingWorkQueue.Count > 0)
							{
								cullData2 = cullingWorkQueue.Dequeue();
							}
						}
						if (cullData2.cullingMode == ThreadCullData.CullingModes.Shadow)
						{
							_ShadowCullSectorThread(cullData2);
							Interlocked.Decrement(ref remainingThreadWork);
						}
					}
				}
				remainingThreadWork = 0;
			}
			enumerator2 = shadowSectorTable.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				List<SECTR_Member.Child> value3 = enumerator2.Current.Value;
				value3.Clear();
				shadowLightPool.Push(value3);
			}
		}
		_ApplyCulling(invisibleLayer);
		int count11 = occluderFrustums.Count;
		for (int num21 = 0; num21 < count11; num21++)
		{
			occluderFrustums[num21].Clear();
			frustumPool.Push(occluderFrustums[num21]);
		}
		occluderFrustums.Clear();
		activeOccluders.Clear();
	}

	private void OnPostRender()
	{
		if (didCull)
		{
			UndoCulling();
			didCull = false;
		}
	}

	private void _CullingWorker()
	{
		while (true)
		{
			ThreadCullData cullData = default(ThreadCullData);
			lock (cullingWorkQueue)
			{
				while (cullingWorkQueue.Count == 0)
				{
					Monitor.Wait(cullingWorkQueue);
				}
				cullData = cullingWorkQueue.Dequeue();
			}
			switch (cullData.cullingMode)
			{
			case ThreadCullData.CullingModes.Graph:
				_FrustumCullSectorThread(cullData);
				break;
			case ThreadCullData.CullingModes.Shadow:
				_ShadowCullSectorThread(cullData);
				break;
			}
			if (cullData.cullingMode == ThreadCullData.CullingModes.Graph || cullData.cullingMode == ThreadCullData.CullingModes.Shadow)
			{
				Interlocked.Decrement(ref remainingThreadWork);
			}
		}
	}

	private void _FrustumCullSectorThread(ThreadCullData cullData)
	{
		Dictionary<int, int> dictionary = null;
		Dictionary<int, int> dictionary2 = null;
		Dictionary<int, int> dictionary3 = null;
		Dictionary<SECTR_Member.Child, int> dictionary4 = null;
		lock (threadVisibleListPool)
		{
			dictionary = ((threadVisibleListPool.Count > 0) ? threadVisibleListPool.Pop() : new Dictionary<int, int>(32));
			dictionary2 = ((threadVisibleListPool.Count > 0) ? threadVisibleListPool.Pop() : new Dictionary<int, int>(32));
			dictionary3 = ((threadVisibleListPool.Count > 0) ? threadVisibleListPool.Pop() : new Dictionary<int, int>(32));
		}
		lock (threadShadowLightPool)
		{
			dictionary4 = ((threadShadowLightPool.Count > 0) ? threadShadowLightPool.Pop() : new Dictionary<SECTR_Member.Child, int>(32));
		}
		_FrustumCullSector(cullData.sector, cullData.cameraPos, cullData.cullingPlanes, cullData.occluderFrustums, cullData.baseMask, cullData.shadowDistance, cullData.cullingSimpleCulling, ref dictionary, ref dictionary2, ref dictionary3, ref dictionary4);
		lock (visibleRenderers)
		{
			Dictionary<int, int>.Enumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int key = enumerator.Current.Key;
				visibleRenderers[key] = key;
			}
		}
		lock (visibleLights)
		{
			Dictionary<int, int>.Enumerator enumerator = dictionary2.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int key2 = enumerator.Current.Key;
				visibleLights[key2] = key2;
			}
		}
		lock (visibleTerrains)
		{
			Dictionary<int, int>.Enumerator enumerator = dictionary3.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int key3 = enumerator.Current.Key;
				visibleTerrains[key3] = key3;
			}
		}
		lock (shadowLights)
		{
			Dictionary<SECTR_Member.Child, int>.Enumerator enumerator2 = dictionary4.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				shadowLights[enumerator2.Current.Key] = 0;
			}
		}
		lock (threadVisibleListPool)
		{
			dictionary.Clear();
			dictionary2.Clear();
			dictionary3.Clear();
			threadVisibleListPool.Push(dictionary);
			threadVisibleListPool.Push(dictionary2);
			threadVisibleListPool.Push(dictionary3);
		}
		lock (threadShadowLightPool)
		{
			dictionary4.Clear();
			threadShadowLightPool.Push(dictionary4);
		}
		lock (threadFrustumPool)
		{
			cullData.cullingPlanes.Clear();
			threadFrustumPool.Push(cullData.cullingPlanes);
			int count = cullData.occluderFrustums.Count;
			for (int i = 0; i < count; i++)
			{
				cullData.occluderFrustums[i].Clear();
				threadFrustumPool.Push(cullData.occluderFrustums[i]);
			}
		}
		lock (threadOccluderPool)
		{
			cullData.occluderFrustums.Clear();
			threadOccluderPool.Push(cullData.occluderFrustums);
		}
	}

	private void _ShadowCullSectorThread(ThreadCullData cullData)
	{
		Dictionary<int, int> dictionary = null;
		Dictionary<int, int> dictionary2 = null;
		lock (threadVisibleListPool)
		{
			dictionary = ((threadVisibleListPool.Count > 0) ? threadVisibleListPool.Pop() : new Dictionary<int, int>(32));
			dictionary2 = ((threadVisibleListPool.Count > 0) ? threadVisibleListPool.Pop() : new Dictionary<int, int>(32));
		}
		_ShadowCullSector(cullData.sector, cullData.sectorShadowLights, ref dictionary, ref dictionary2);
		lock (visibleRenderers)
		{
			Dictionary<int, int>.Enumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int key = enumerator.Current.Key;
				visibleRenderers[key] = key;
			}
		}
		lock (visibleTerrains)
		{
			Dictionary<int, int>.Enumerator enumerator = dictionary2.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int key2 = enumerator.Current.Key;
				visibleTerrains[key2] = key2;
			}
		}
		lock (threadVisibleListPool)
		{
			dictionary.Clear();
			dictionary2.Clear();
			threadVisibleListPool.Push(dictionary);
			threadVisibleListPool.Push(dictionary2);
		}
	}

	private static void _FrustumCullSector(SECTR_Sector sector, Vector3 cameraPos, List<Plane> cullingPlanes, List<List<Plane>> occluderFrustums, int baseMask, float shadowDistance, bool forceGroupCull, ref Dictionary<int, int> visibleRenderers, ref Dictionary<int, int> visibleLights, ref Dictionary<int, int> visibleTerrains, ref Dictionary<SECTR_Member.Child, int> shadowLights)
	{
		_FrustumCull(sector, cameraPos, cullingPlanes, occluderFrustums, baseMask, shadowDistance, forceGroupCull, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
		int count = sector.Members.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = sector.Members[i];
			if (sECTR_Member.HasRenderBounds || sECTR_Member.HasLightBounds)
			{
				_FrustumCull(sECTR_Member, cameraPos, cullingPlanes, occluderFrustums, baseMask, shadowDistance, forceGroupCull, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
			}
		}
	}

	private static void _FrustumCull(SECTR_Member member, Vector3 cameraPos, List<Plane> frustumPlanes, List<List<Plane>> occluders, int baseMask, float shadowDistance, bool forceGroupCull, ref Dictionary<int, int> visibleRenderers, ref Dictionary<int, int> visibleLights, ref Dictionary<int, int> visibleTerrains, ref Dictionary<SECTR_Member.Child, int> shadowLights)
	{
		int outMask = 0;
		int outMask2 = 0;
		bool flag = member.CullEachChild && !forceGroupCull;
		bool flag2 = member.HasRenderBounds && SECTR_Geometry.FrustumIntersectsBounds(member.RenderBounds, frustumPlanes, baseMask, out outMask);
		bool flag3 = member.HasLightBounds && SECTR_Geometry.FrustumIntersectsBounds(member.LightBounds, frustumPlanes, baseMask, out outMask2);
		int count = occluders.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(flag2 || flag3))
			{
				break;
			}
			List<Plane> frustum = occluders[i];
			if (flag2)
			{
				flag2 = !SECTR_Geometry.FrustumContainsBounds(member.RenderBounds, frustum);
			}
			if (flag3)
			{
				flag3 = !SECTR_Geometry.FrustumContainsBounds(member.LightBounds, frustum);
			}
		}
		if (flag2)
		{
			int count2 = member.Renderers.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Member.Child child = member.Renderers[j];
				if (child.renderHash != 0 && !visibleRenderers.ContainsKey(child.renderHash) && (!flag || _IsVisible(child.rendererBounds, frustumPlanes, outMask, occluders)))
				{
					visibleRenderers.Add(child.renderHash, child.renderHash);
				}
			}
			int count3 = member.Terrains.Count;
			for (int k = 0; k < count3; k++)
			{
				SECTR_Member.Child child2 = member.Terrains[k];
				if (child2.terrainHash != 0 && !visibleTerrains.ContainsKey(child2.terrainHash) && (!flag || _IsVisible(child2.terrainBounds, frustumPlanes, outMask, occluders)))
				{
					visibleTerrains.Add(child2.terrainHash, child2.terrainHash);
				}
			}
		}
		if (!flag3)
		{
			return;
		}
		int count4 = member.Lights.Count;
		for (int l = 0; l < count4; l++)
		{
			SECTR_Member.Child child3 = member.Lights[l];
			if (child3.lightHash != 0 && !visibleLights.ContainsKey(child3.lightHash) && (!flag || _IsVisible(child3.lightBounds, frustumPlanes, outMask, occluders)))
			{
				visibleLights.Add(child3.lightHash, child3.lightHash);
				if (child3.shadowLight && !shadowLights.ContainsKey(child3) && Vector3.Distance(cameraPos, child3.shadowLightPosition) - child3.shadowLightRange <= shadowDistance)
				{
					shadowLights.Add(child3, 0);
				}
			}
		}
	}

	private static void _ShadowCullSector(SECTR_Sector sector, List<SECTR_Member.Child> sectorShadowLights, ref Dictionary<int, int> visibleRenderers, ref Dictionary<int, int> visibleTerrains)
	{
		if (sector.ShadowCaster)
		{
			_ShadowCull(sector, sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
		}
		int count = sector.Members.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = sector.Members[i];
			if (sECTR_Member.ShadowCaster)
			{
				_ShadowCull(sECTR_Member, sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
			}
		}
	}

	private static void _ShadowCull(SECTR_Member member, List<SECTR_Member.Child> shadowLights, ref Dictionary<int, int> visibleRenderers, ref Dictionary<int, int> visibleTerrains)
	{
		int count = shadowLights.Count;
		int count2 = member.ShadowCasters.Count;
		if (member.CullEachChild)
		{
			for (int i = 0; i < count2; i++)
			{
				SECTR_Member.Child child = member.ShadowCasters[i];
				if (child.renderHash != 0 && !visibleRenderers.ContainsKey(child.renderHash))
				{
					for (int j = 0; j < count; j++)
					{
						SECTR_Member.Child child2 = shadowLights[j];
						if ((child2.shadowCullingMask & (1 << (int)child.layer)) != 0 && ((child2.shadowLightType == LightType.Spot && child.rendererBounds.Intersects(child2.lightBounds)) || (child2.shadowLightType == LightType.Point && SECTR_Geometry.BoundsIntersectsSphere(child.rendererBounds, child2.shadowLightPosition, child2.shadowLightRange))))
						{
							visibleRenderers.Add(child.renderHash, child.renderHash);
							break;
						}
					}
				}
				if (child.terrainHash == 0 || visibleTerrains.ContainsKey(child.terrainHash))
				{
					continue;
				}
				for (int k = 0; k < count; k++)
				{
					SECTR_Member.Child child3 = shadowLights[k];
					if ((child3.shadowCullingMask & (1 << (int)child.layer)) != 0 && ((child3.shadowLightType == LightType.Spot && child.terrainBounds.Intersects(child3.lightBounds)) || (child3.shadowLightType == LightType.Point && SECTR_Geometry.BoundsIntersectsSphere(child.terrainBounds, child3.shadowLightPosition, child3.shadowLightRange))))
					{
						visibleTerrains.Add(child.terrainHash, child.terrainHash);
						break;
					}
				}
			}
			return;
		}
		for (int l = 0; l < count; l++)
		{
			SECTR_Member.Child child4 = shadowLights[l];
			if (!((child4.shadowLightType == LightType.Spot) ? member.RenderBounds.Intersects(child4.lightBounds) : SECTR_Geometry.BoundsIntersectsSphere(member.RenderBounds, child4.shadowLightPosition, child4.shadowLightRange)))
			{
				continue;
			}
			int shadowCullingMask = child4.shadowCullingMask;
			for (int m = 0; m < count2; m++)
			{
				SECTR_Member.Child child5 = member.ShadowCasters[m];
				if (child5.renderHash != 0 && child5.terrainHash != 0)
				{
					if ((shadowCullingMask & (1 << (int)child5.layer)) != 0)
					{
						if (!visibleRenderers.ContainsKey(child5.renderHash))
						{
							visibleRenderers.Add(child5.renderHash, child5.renderHash);
						}
						if (!visibleTerrains.ContainsKey(child5.terrainHash))
						{
							visibleTerrains.Add(child5.terrainHash, child5.terrainHash);
						}
					}
				}
				else if (child5.renderHash != 0 && !visibleRenderers.ContainsKey(child5.renderHash) && (shadowCullingMask & (1 << (int)child5.layer)) != 0)
				{
					visibleRenderers.Add(child5.renderHash, child5.renderHash);
				}
				else if (child5.terrainHash != 0 && !visibleTerrains.ContainsKey(child5.terrainHash) && (shadowCullingMask & (1 << (int)child5.layer)) != 0)
				{
					visibleTerrains.Add(child5.terrainHash, child5.terrainHash);
				}
			}
		}
	}

	private static bool _IsVisible(Bounds childBounds, List<Plane> frustumPlanes, int parentMask, List<List<Plane>> occluders)
	{
		if (SECTR_Geometry.FrustumIntersectsBounds(childBounds, frustumPlanes, parentMask, out var _))
		{
			int count = occluders.Count;
			for (int i = 0; i < count; i++)
			{
				if (SECTR_Geometry.FrustumContainsBounds(childBounds, occluders[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void _ApplyCulling(int cullingInvisibleLayer)
	{
		int count = SECTR_Member.All.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = SECTR_Member.All[i];
			int count2 = sECTR_Member.Children.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Member.Child child = sECTR_Member.Children[j];
				Renderer renderer = child.renderer;
				if ((bool)renderer && !visibleRenderers.ContainsKey(child.renderHash) && renderer.enabled)
				{
					hiddenRenderers.Add(renderer);
					renderer.enabled = false;
				}
				Light light = child.light;
				if ((bool)light && !visibleLights.ContainsKey(child.lightHash) && light.enabled)
				{
					hiddenLights.Add(light);
					light.enabled = false;
				}
				Terrain terrain = child.terrain;
				if ((bool)terrain && !visibleTerrains.ContainsKey(child.terrainHash) && terrain.enabled)
				{
					hiddenTerrains.Add(terrain);
					terrain.enabled = false;
				}
			}
		}
	}

	private void UndoCulling()
	{
		int count = hiddenRenderers.Count;
		for (int i = 0; i < count; i++)
		{
			hiddenRenderers[i].enabled = true;
		}
		hiddenRenderers.Clear();
		int count2 = hiddenLights.Count;
		for (int j = 0; j < count2; j++)
		{
			hiddenLights[j].enabled = true;
		}
		hiddenLights.Clear();
		int count3 = hiddenTerrains.Count;
		for (int k = 0; k < count3; k++)
		{
			hiddenTerrains[k].enabled = true;
		}
		hiddenTerrains.Clear();
		renderersCulled = count;
		lightsCulled = count2;
		terrainsCulled = count3;
	}

	private void _BuildFrustumFromHull(Camera cullingCamera, bool forwardTraversal, List<ClipVertex> portalVertices, ref List<Plane> newFrustum)
	{
		int count = portalVertices.Count;
		if (count <= 2)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = portalVertices[i].vertex;
			Vector3 vector2 = (Vector3)portalVertices[(i + 1) % count].vertex - vector;
			if (Vector3.SqrMagnitude(vector2) > 0.001f)
			{
				Vector3 vector3 = vector - cullingCamera.transform.position;
				Vector3 inNormal = (forwardTraversal ? Vector3.Cross(vector2, vector3) : Vector3.Cross(vector3, vector2));
				inNormal.Normalize();
				newFrustum.Add(new Plane(inNormal, vector));
			}
		}
	}
}
