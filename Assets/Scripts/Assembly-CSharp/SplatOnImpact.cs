using UnityEngine;

public class SplatOnImpact : CollidableActorBehaviour, Collidable
{
	public GameObject splatPrefab;

	public GameObject splatFXPrefab;

	private SlimeAppearanceApplicator appearanceApplicator;

	private SlimeAudio slimeAudio;

	private SlimeFaceAnimator slimeFaceAnimator;

	private int penWallLayer;

	private const float SPLAT_THRESHOLD = 6f;

	private const float COLLISION_AUDIO_THRESHOLD = 6f;

	private const float COLLISION_VO_THRESHOLD = 10f;

	private const float MIN_SCALE_FACTOR = 0.75f;

	private const float MAX_SCALE_FACTOR = 2.25f;

	private const float SPEED_SCALE_POW = 0.25f;

	private const float MAX_SPEED_SCALE = 2.5f;

	private static ContactPoint[] local_contactResults = new ContactPoint[10];

	public override void Awake()
	{
		base.Awake();
		slimeAudio = GetComponent<SlimeAudio>();
		slimeFaceAnimator = GetComponent<SlimeFaceAnimator>();
		appearanceApplicator = GetComponent<SlimeAppearanceApplicator>();
		penWallLayer = LayerMask.NameToLayer("Pen Walls");
	}

	public void ProcessCollisionEnter(Collision col)
	{
		if (!(col.rigidbody == null))
		{
			return;
		}
		float num = float.NegativeInfinity;
		ContactPoint? contactPoint = null;
		int contacts = col.GetContacts(local_contactResults);
		for (int i = 0; i < contacts; i++)
		{
			ContactPoint value = local_contactResults[i];
			float num2 = Vector3.Dot(value.normal, col.relativeVelocity);
			if (num2 > num)
			{
				num = num2;
				contactPoint = value;
			}
		}
		if (num > 6f)
		{
			bool flag = col.gameObject.layer == penWallLayer;
			GameObject gameObject = ((!flag) ? SRBehaviour.InstantiateDynamic(splatPrefab, contactPoint.Value.point, Quaternion.LookRotation(contactPoint.Value.normal)) : SRBehaviour.SpawnAndPlayFX(splatFXPrefab, contactPoint.Value.point, Quaternion.LookRotation(contactPoint.Value.normal)));
			gameObject.transform.Rotate(Vector3.forward, Randoms.SHARED.GetFloat(360f), Space.Self);
			SlimeAppearance.Palette appearancePalette = appearanceApplicator.GetAppearancePalette();
			RecolorSlimeMaterial[] componentsInChildren = gameObject.GetComponentsInChildren<RecolorSlimeMaterial>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].SetColors(appearancePalette.Top, appearancePalette.Middle, appearancePalette.Bottom);
			}
			if (!flag)
			{
				float num3 = Mathf.Min(2.5f, Mathf.Pow(num / 6f, 0.25f));
				float inRange = Randoms.SHARED.GetInRange(0.75f, 2.25f);
				FadeAndDestroySplat component = gameObject.GetComponent<FadeAndDestroySplat>();
				component.SetScale(base.transform.localScale.x * inRange * num3);
				component.SetColors(appearancePalette.Top, appearancePalette.Middle, appearancePalette.Bottom);
			}
			if (slimeAudio != null)
			{
				slimeAudio.Play(slimeAudio.slimeSounds.splatCue);
			}
		}
		if (num > 10f)
		{
			if (slimeAudio != null)
			{
				slimeAudio.Play(slimeAudio.slimeSounds.voiceSplatCue);
			}
			if (slimeFaceAnimator != null)
			{
				slimeFaceAnimator.SetTrigger("triggerMinorWince");
			}
		}
	}

	public void ProcessCollisionExit(Collision col)
	{
	}
}
