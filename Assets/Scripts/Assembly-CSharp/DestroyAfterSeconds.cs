using UnityEngine;

public class DestroyAfterSeconds : SRBehaviour
{
	public float time;

	private float awakeTime;

	public void Awake()
	{
		awakeTime = Time.time;
	}

	public void Update()
	{
		if (Time.time >= awakeTime + time)
		{
			Destroyer.DestroyActor(base.gameObject, "DestroyAfterSeconds.Update");
		}
	}
}
