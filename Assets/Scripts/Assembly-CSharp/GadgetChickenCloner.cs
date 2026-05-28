using System.Collections.Generic;
using Assets.Script.Util.Extensions;
using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class GadgetChickenCloner : SRBehaviour
{
	[Tooltip("GameObject to be rotated along the z-axis; animation property.")]
	public GameObject spinner;

	[Tooltip("GameObject representing the blurred spinner; activated/deactivated based off current speed.")]
	public GameObject spinnerBlur;

	[Tooltip("Rotation speed of the 'spinner' GameObject; animation property.")]
	public float spinnerRotationSpeed;

	[Tooltip("FX played when a chicken enters the cloner, and is successfully cloned.")]
	public GameObject onSuccessFX;

	[Tooltip("FX played when a chicken enters the cloner, and is not cloned.")]
	public GameObject onFailureFX;

	[Tooltip("SFX played when then cloning animation begins. (non-looping)")]
	public SECTR_AudioCue onAnimationStartSFX;

	[Tooltip("SFX played while the cloning animation runs. (looping)")]
	public SECTR_AudioCue onAnimationRunSFX;

	[Tooltip("SFX played when then cloning animation ends. (non-looping)")]
	public SECTR_AudioCue onAnimationEndSFX;

	private static readonly int ANIMATION_ACTIVE = Animator.StringToHash("ACTIVE");

	private static readonly float MIN_BLUR_ROTATION_SPEED = 400f;

	private TimeDirector timeDirector;

	private SECTR_PointSource audioSource;

	private Animator animator;

	private HashSet<GameObject> ignored = new HashSet<GameObject>();

	private double animatorDeactivateTime;

	public void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		animator = GetComponentInParent<Animator>();
		animator.SetBool(ANIMATION_ACTIVE, value: false);
		audioSource = GetComponent<SECTR_PointSource>();
		spinnerBlur.SetActive(value: false);
	}

	public void Update()
	{
		if (timeDirector.OnPassedTime(animatorDeactivateTime))
		{
			animator.SetBool(ANIMATION_ACTIVE, value: false);
			audioSource.Cue = onAnimationEndSFX;
			audioSource.Play();
		}
		if (!Mathf.Approximately(spinnerRotationSpeed, 0f))
		{
			spinner.transform.Rotate(0f, 0f, Time.deltaTime * spinnerRotationSpeed);
			spinnerBlur.SetActive(spinnerRotationSpeed >= MIN_BLUR_ROTATION_SPEED);
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (!Identifiable.MEAT_CLASS.Contains(Identifiable.GetId(collider.gameObject)) || !ignored.Add(collider.gameObject))
		{
			return;
		}
		Destroyer.DestroyActor(collider.gameObject, "GadgetChickenCloner.OnTriggerEnter");
		Quaternion quaternion = Quaternion.LookRotation((Vector3.Angle(collider.gameObject.GetComponent<Rigidbody>().velocity, base.transform.forward) > 90f) ? Vector3.back : Vector3.forward);
		if (!animator.GetBool(ANIMATION_ACTIVE))
		{
			audioSource.Cue = onAnimationStartSFX;
			audioSource.Play();
			audioSource.Cue = onAnimationRunSFX;
			audioSource.Play();
		}
		animator.SetBool(ANIMATION_ACTIVE, value: true);
		animatorDeactivateTime = timeDirector.HoursFromNow(1f / 60f);
		if (Randoms.SHARED.GetProbability(0.5f))
		{
			SRBehaviour.SpawnAndPlayFX(onSuccessFX, base.gameObject, Vector3.zero, quaternion);
			RegionRegistry.RegionSetId setId = collider.gameObject.GetComponent<RegionMember>().setId;
			GameObject prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.GetId(collider.gameObject));
			GameObject[] array = new GameObject[2];
			List<Identifiable.Id> allFashions = collider.gameObject.GetRequiredComponent<AttachFashions>().GetAllFashions();
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject = (array[i] = SRBehaviour.InstantiateActor(prefab, setId, base.transform.position, base.transform.rotation * quaternion));
				gameObject.GetRequiredComponent<Vacuumable>().Launch(Vacuumable.LaunchSource.CHICKEN_CLONER);
				gameObject.GetRequiredComponent<AttachFashions>().SetFashions(allFashions);
				gameObject.GetComponent<Rigidbody>().velocity = (Quaternion.Euler(0f, (i == 0) ? 20 : (-20), 0f) * gameObject.transform.forward).normalized * 10f;
				gameObject.transform.DOScale(gameObject.transform.localScale, 0.2f).From(0.2f).SetEase(Ease.Linear);
				ignored.Add(gameObject);
			}
			PhysicsUtil.IgnoreCollision(array[0], array[1], 0.2f);
		}
		else
		{
			SRBehaviour.SpawnAndPlayFX(onFailureFX, base.gameObject, Vector3.zero, quaternion);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		ignored.RemoveWhere((GameObject go) => go == null || go == collider.gameObject);
	}
}
