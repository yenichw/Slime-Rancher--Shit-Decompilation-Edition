using UnityEngine;

public class AdjustMusicOnOasisNearby : MonoBehaviour
{
	private MusicDirector musicDir;

	private bool oasisMode;

	private float rethinkTime;

	private const float RETHINK_TIME_ON_OASIS_START = 10f;

	private const float RETHINK_TIME_ON_OASIS_END = 5f;

	private const float RETHINK_TIME_UNCHANGED = 0.5f;

	private CullingGroup oasisGroup;

	private bool firstFramePassed;

	private int[] local_sphereCheck = new int[20];

	public void Awake()
	{
		musicDir = SRSingleton<GameContext>.Instance.MusicDirector;
		oasisGroup = new CullingGroup();
		oasisGroup.SetBoundingSpheres(Oasis.oasisSpheres.Data);
		oasisGroup.SetBoundingSphereCount(Oasis.oasisSpheres.GetCount());
		oasisGroup.SetDistanceReferencePoint(base.gameObject.transform);
		oasisGroup.SetBoundingDistances(new float[1] { 50f });
	}

	public void OnEnable()
	{
		RefreshCullingGroup();
	}

	public void OnDestroy()
	{
		Oasis.oasisSpheres.Clear();
		oasisGroup.Dispose();
	}

	public void Update()
	{
		if (firstFramePassed && Time.time > rethinkTime)
		{
			RefreshCullingGroup();
			RethinkMusic();
		}
		else if (!firstFramePassed)
		{
			RefreshCullingGroup();
			firstFramePassed = true;
		}
	}

	private void RefreshCullingGroup()
	{
		oasisGroup.SetBoundingSphereCount(Oasis.oasisSpheres.GetCount());
	}

	private void RethinkMusic()
	{
		bool flag = oasisGroup.QueryIndices(0, local_sphereCheck, 0) >= 1;
		if (flag != oasisMode)
		{
			oasisMode = flag;
			musicDir.SetOasisMode(oasisMode);
			rethinkTime = Time.time + (oasisMode ? 10f : 5f);
		}
		else
		{
			rethinkTime = Time.time + 0.5f;
		}
	}
}
