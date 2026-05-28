using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SECTR_Sector))]
[AddComponentMenu("SECTR/Stream/SECTR Chunk")]
public class SECTR_Chunk : MonoBehaviour
{
	private enum LoadState
	{
		Unloaded = 0,
		Loading = 1,
		Loaded = 2,
		Unloading = 3,
		Active = 4
	}

	public delegate void LoadCallback(SECTR_Chunk source, bool loaded);

	private AsyncOperation asyncLoadOp;

	private LoadState loadState;

	private int refCount;

	private int wakeRefCount;

	private GameObject chunkRoot;

	private GameObject chunkSector;

	private bool recenterChunk;

	private SECTR_Sector cachedSector;

	private GameObject proxy;

	private bool quitting;

	private static bool sceneActivating;

	[SECTR_ToolTip("The path of the scene to load")]
	public string ScenePath;

	[SECTR_ToolTip("The unique name of the root object in the exported Sector.")]
	public string NodeName;

	[SECTR_ToolTip("Exports the Chunk in a way that allows it to be shared by multiple Sectors, but may take more CPU to load.")]
	public bool ExportForReuse;

	[SECTR_ToolTip("A mesh to display when this Chunk is unloaded. Will be hidden when loaded.")]
	public Mesh ProxyMesh;

	[SECTR_ToolTip("The per-submesh materials for the proxy.")]
	public Material[] ProxyMaterials;

	private bool canProxy;

	public SECTR_Sector Sector => cachedSector;

	public event LoadCallback Changed;

	public event LoadCallback ReferenceChange;

	public void SetCanProxy(bool canProxy)
	{
		this.canProxy = canProxy;
		if (!canProxy && proxy != null)
		{
			Destroyer.Destroy(proxy, "SECTR_Chunk.SetCanProxy");
			proxy = null;
		}
		else if (proxy == null && (bool)ProxyMesh && canProxy)
		{
			_CreateProxy();
		}
	}

	public void AddReference()
	{
		if (refCount == 0)
		{
			_Load();
			if (this.Changed != null)
			{
				this.Changed(this, loaded: true);
			}
		}
		refCount++;
		if (this.ReferenceChange != null)
		{
			this.ReferenceChange(this, loaded: true);
		}
	}

	public void RemoveReference()
	{
		if (this.ReferenceChange != null)
		{
			this.ReferenceChange(this, loaded: false);
		}
		refCount--;
		if (refCount <= 0)
		{
			if (this.Changed != null)
			{
				this.Changed(this, loaded: false);
			}
			_Unload();
			refCount = 0;
		}
	}

	public void AddWakeReference()
	{
		if (wakeRefCount == 0 && cachedSector != null)
		{
			cachedSector.Hibernate = false;
		}
		wakeRefCount++;
	}

	public void RemoveWakeReference()
	{
		wakeRefCount--;
		if (wakeRefCount <= 0)
		{
			if (cachedSector != null)
			{
				cachedSector.Hibernate = true;
			}
			wakeRefCount = 0;
		}
	}

	public void CheckReferences()
	{
		if (refCount <= 0)
		{
			_Unload();
			refCount = 0;
		}
		if (wakeRefCount <= 0)
		{
			if (cachedSector != null)
			{
				cachedSector.Hibernate = true;
			}
			wakeRefCount = 0;
		}
	}

	public bool IsLoaded()
	{
		return loadState == LoadState.Active;
	}

	public bool IsUnloaded()
	{
		return loadState == LoadState.Unloaded;
	}

	public float LoadProgress()
	{
		switch (loadState)
		{
		case LoadState.Loading:
			if (asyncLoadOp == null)
			{
				return 0.5f;
			}
			return asyncLoadOp.progress * 0.8f;
		case LoadState.Loaded:
			return 0.9f;
		case LoadState.Active:
			return 1f;
		default:
			return 0f;
		}
	}

	private void Awake()
	{
		SECTR_LightmapRef.InitRefCounts();
	}

	private void OnEnable()
	{
		cachedSector = GetComponent<SECTR_Sector>();
		cachedSector.Hibernate = wakeRefCount <= 0;
		if (cachedSector.Frozen && canProxy)
		{
			_CreateProxy();
		}
	}

	private void OnDisable()
	{
		if (!quitting && asyncLoadOp != null && !asyncLoadOp.isDone)
		{
			Debug.LogError("Chunk unloaded with async operation active. Do not disable chunks until async operations are complete or Unity will likely crash.");
		}
		if (loadState != 0)
		{
			_FindChunkRoot();
			if ((bool)chunkRoot)
			{
				_DestoryChunk(createProxy: false);
			}
		}
		cachedSector = null;
	}

	private void OnApplicationQuit()
	{
		quitting = true;
	}

	private void FixedUpdate()
	{
		switch (loadState)
		{
		case LoadState.Loading:
			_TrySceneActivation();
			if (asyncLoadOp == null || asyncLoadOp.isDone)
			{
				sceneActivating = false;
				asyncLoadOp = null;
				loadState = LoadState.Loaded;
				FixedUpdate();
			}
			break;
		case LoadState.Loaded:
			_SetupChunk();
			break;
		case LoadState.Unloading:
			_TrySceneActivation();
			_FindChunkRoot();
			if ((bool)chunkRoot)
			{
				_DestoryChunk(createProxy: true);
			}
			break;
		case LoadState.Active:
			break;
		}
	}

	private void _Load()
	{
		if (base.enabled && (loadState == LoadState.Unloaded || loadState == LoadState.Unloading))
		{
			chunkRoot.SetActive(value: true);
			loadState = LoadState.Loaded;
		}
	}

	public void SetRoot(GameObject root)
	{
		chunkRoot = root;
		chunkSector = root;
		loadState = LoadState.Active;
	}

	private void _Unload()
	{
		if (base.enabled && loadState != 0)
		{
			cachedSector.Frozen = true;
			if ((bool)chunkRoot)
			{
				_DestoryChunk(createProxy: true);
			}
			else
			{
				loadState = LoadState.Unloading;
			}
		}
	}

	private void _DestoryChunk(bool createProxy)
	{
		if ((bool)cachedSector.TopTerrain || (bool)cachedSector.BottomTerrain || (bool)cachedSector.RightTerrain || (bool)cachedSector.LeftTerrain)
		{
			cachedSector.DisonnectTerrainNeighbors();
		}
		chunkRoot.SetActive(value: false);
		loadState = LoadState.Unloaded;
		if (createProxy && (bool)ProxyMesh && canProxy)
		{
			_CreateProxy();
		}
	}

	private void _FindChunkRoot()
	{
		if (!(chunkRoot == null))
		{
			return;
		}
		SECTR_ChunkRef sECTR_ChunkRef = SECTR_ChunkRef.FindChunkRef(NodeName);
		if ((bool)sECTR_ChunkRef && (bool)sECTR_ChunkRef.RealSector)
		{
			recenterChunk = sECTR_ChunkRef.Recentered;
			if (recenterChunk)
			{
				sECTR_ChunkRef.RealSector.parent = base.transform;
				chunkRoot = sECTR_ChunkRef.RealSector.gameObject;
				chunkSector = chunkRoot;
				Destroyer.Destroy(sECTR_ChunkRef.gameObject, "SECTR_Chunk._FindChunkRoot#1");
			}
			else
			{
				chunkRoot = sECTR_ChunkRef.gameObject;
				chunkSector = sECTR_ChunkRef.RealSector.gameObject;
				Destroyer.Destroy(sECTR_ChunkRef, "SECTR_Chunk._FindChunkRoot#2");
			}
		}
		else if (!quitting)
		{
			chunkRoot = GameObject.Find(NodeName);
			chunkSector = chunkRoot;
			recenterChunk = false;
		}
	}

	private void _SetupChunk()
	{
		_FindChunkRoot();
		if (!chunkRoot)
		{
			return;
		}
		if (!chunkRoot.activeSelf)
		{
			chunkRoot.SetActive(value: true);
		}
		if (recenterChunk)
		{
			Transform obj = chunkRoot.transform;
			obj.localPosition = Vector3.zero;
			obj.localRotation = Quaternion.identity;
			obj.localScale = Vector3.one;
		}
		SECTR_Member sECTR_Member = chunkSector.GetComponent<SECTR_Member>();
		if (!sECTR_Member)
		{
			sECTR_Member = chunkSector.gameObject.AddComponent<SECTR_Member>();
			sECTR_Member.BoundsUpdateMode = SECTR_Member.BoundsUpdateModes.Static;
			sECTR_Member.ForceUpdate(updateChildren: true);
		}
		else if (recenterChunk)
		{
			sECTR_Member.ForceUpdate(updateChildren: true);
		}
		cachedSector.ChildProxy = sECTR_Member;
		cachedSector.Frozen = false;
		if ((bool)cachedSector.TopTerrain || (bool)cachedSector.BottomTerrain || (bool)cachedSector.LeftTerrain || (bool)cachedSector.RightTerrain)
		{
			cachedSector.ConnectTerrainNeighbors();
			if ((bool)cachedSector.TopTerrain)
			{
				cachedSector.TopTerrain.ConnectTerrainNeighbors();
			}
			if ((bool)cachedSector.BottomTerrain)
			{
				cachedSector.BottomTerrain.ConnectTerrainNeighbors();
			}
			if ((bool)cachedSector.LeftTerrain)
			{
				cachedSector.LeftTerrain.ConnectTerrainNeighbors();
			}
			if ((bool)cachedSector.RightTerrain)
			{
				cachedSector.RightTerrain.ConnectTerrainNeighbors();
			}
		}
		if ((bool)proxy)
		{
			Destroyer.Destroy(proxy, "SECTR_Chunk._SetupChunk");
			proxy = null;
		}
		loadState = LoadState.Active;
	}

	private void _CreateProxy()
	{
		if (proxy == null && (bool)ProxyMesh && !quitting)
		{
			proxy = new GameObject(base.name + " Proxy");
			proxy.AddComponent<MeshFilter>().sharedMesh = ProxyMesh;
			MeshRenderer meshRenderer = proxy.AddComponent<MeshRenderer>();
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.sharedMaterials = ProxyMaterials;
			proxy.transform.position = base.transform.position;
			proxy.transform.rotation = base.transform.rotation;
			proxy.transform.localScale = base.transform.lossyScale;
			proxy.transform.SetParent(base.transform, worldPositionStays: true);
		}
	}

	private void _TrySceneActivation()
	{
		if (asyncLoadOp != null && !asyncLoadOp.allowSceneActivation && !sceneActivating && asyncLoadOp.progress >= 0.9f)
		{
			sceneActivating = true;
			asyncLoadOp.allowSceneActivation = true;
		}
	}
}
