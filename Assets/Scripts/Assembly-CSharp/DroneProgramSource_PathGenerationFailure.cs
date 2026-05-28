using UnityEngine;

public class DroneProgramSource_PathGenerationFailure : SRBehaviour
{
	private const float MIN_DISTANCE = 3f;

	private const float MIN_DISTANCE_SQR = 9f;

	private Vector3 startPosition;

	public void Awake()
	{
		startPosition = base.transform.position;
	}

	public void OnDestroy()
	{
		DroneProgramSource.BLACKLIST.Remove(base.gameObject);
	}

	public void Update()
	{
		if ((base.transform.position - startPosition).sqrMagnitude >= 9f)
		{
			Destroyer.Destroy(this, "DroneProgramSource_PathGenerationFailure.Update");
		}
	}
}
