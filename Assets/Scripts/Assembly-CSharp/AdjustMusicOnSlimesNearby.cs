using System;
using UnityEngine;

public class AdjustMusicOnSlimesNearby : MonoBehaviour
{
	private MusicDirector musicDir;

	private bool tarrMode;

	private float rethinkTime;

	private const float RETHINK_TIME_ON_TARR_START = 10f;

	private const float RETHINK_TIME_ON_TARR_END = 5f;

	private const float RETHINK_TIME_UNCHANGED = 0.5f;

	private const float TARR_DISTANCE_FOR_MUSIC = 30f;

	private CullingGroup tarrGroup;

	private int[] local_nearbyTarrIndices = new int[100];

	public void Awake()
	{
		musicDir = SRSingleton<GameContext>.Instance.MusicDirector;
		tarrGroup = new CullingGroup();
		tarrGroup.SetBoundingSpheres(TarrBoundingSphere.allSpheres);
		tarrGroup.SetBoundingDistances(new float[1] { 30f });
		tarrGroup.SetDistanceReferencePoint(base.transform);
		tarrGroup.SetBoundingSphereCount(TarrBoundingSphere.sphereCount);
	}

	public void OnDestroy()
	{
		tarrGroup.Dispose();
		TarrBoundingSphere.ResetTarrData();
	}

	public void Update()
	{
		RefreshCullingGroup();
		if (Time.time > rethinkTime)
		{
			RethinkMusic();
		}
		ResetTarrBoundingSphereCount();
	}

	private void RefreshCullingGroup()
	{
		if (local_nearbyTarrIndices.Length != TarrBoundingSphere.allSpheres.Length)
		{
			Array.Resize(ref local_nearbyTarrIndices, TarrBoundingSphere.allSpheres.Length);
		}
		tarrGroup.SetBoundingSphereCount(TarrBoundingSphere.sphereCount);
	}

	private void RethinkMusic()
	{
		bool flag = tarrGroup.QueryIndices(0, local_nearbyTarrIndices, 0) >= 1;
		if (flag != tarrMode)
		{
			tarrMode = flag;
			musicDir.SetTarrMode(tarrMode);
			rethinkTime = Time.time + (tarrMode ? 10f : 5f);
		}
		else
		{
			rethinkTime = Time.time + 0.5f;
		}
	}

	private void ResetTarrBoundingSphereCount()
	{
		TarrBoundingSphere.sphereCount = 0;
	}
}
