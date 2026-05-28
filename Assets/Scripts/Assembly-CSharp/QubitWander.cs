using UnityEngine;

public class QubitWander : MonoBehaviour
{
	public static float TravelTime = 5f;

	public static float LifeSpan = 16f;

	public float WanderAmplitude = 1f;

	public float WanderFrequency = 2f;

	public Vector3 EndPosition;

	private bool hasArrived;

	private float travelEndTime;

	private float dissipationTime;

	private Vector3 startPosition = Vector3.zero;

	private Vector3 orthogonal;

	public GenerateQuantumQubit parentQuantumGenerator;

	private void Start()
	{
		travelEndTime = Time.time + TravelTime;
		dissipationTime = Time.time + LifeSpan;
		startPosition = base.gameObject.transform.position;
		Vector3 normalized = (EndPosition - startPosition).normalized;
		LookAtDestination();
		orthogonal = new Vector3(0f - normalized.z, 0f, normalized.x);
	}

	private void Update()
	{
		if (!hasArrived && travelEndTime > Time.time)
		{
			float num = travelEndTime - Time.time;
			base.gameObject.transform.position = Vector3.Lerp(startPosition, EndPosition, 1f - num / TravelTime) + orthogonal * WanderAmplitude * Mathf.Sin(WanderFrequency * num);
			LookAtDestination();
		}
		else if (!hasArrived)
		{
			base.gameObject.transform.position = EndPosition;
			hasArrived = true;
		}
		else if (dissipationTime < Time.time)
		{
			parentQuantumGenerator.DissipateQubit(this);
		}
	}

	private void LookAtDestination()
	{
		base.gameObject.transform.LookAt(new Vector3(EndPosition.x, base.gameObject.transform.position.y, EndPosition.z));
	}

	public bool HasArrived()
	{
		return hasArrived;
	}
}
