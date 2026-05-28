using UnityEngine;

public class SlimeIgniteReact : SRBehaviour, Ignitable
{
	public GameObject igniteFX;

	private SlimeFaceAnimator faceAnim;

	private SlimeEmotions emotions;

	private Rigidbody body;

	private bool selfIsIgniter;

	private float throttle;

	private const float BACK_FORCE = 1f;

	private const float UP_FORCE = 3f;

	private const float IGNITE_THROTTLE_TIME = 0.2f;

	private const float IGNITE_AGITATION = 0.5f;

	public void Awake()
	{
		faceAnim = GetComponent<SlimeFaceAnimator>();
		emotions = GetComponent<SlimeEmotions>();
		body = GetComponent<Rigidbody>();
		selfIsIgniter = GetComponent<FireSlimeIgnition>() != null;
	}

	public void Ignite(GameObject igniter)
	{
		if (!selfIsIgniter && !(Time.time < throttle))
		{
			throttle = Time.time + 0.2f;
			if (igniteFX != null)
			{
				SRBehaviour.SpawnAndPlayFX(igniteFX, base.transform.position, base.transform.rotation);
			}
			faceAnim.SetTrigger("triggerAlarm");
			emotions.Adjust(SlimeEmotions.Emotion.AGITATION, 0.5f);
			Vector3 vector = base.transform.position - igniter.transform.position;
			body.AddForce(vector.normalized * 1f + Vector3.up * 3f, ForceMode.Impulse);
		}
	}
}
