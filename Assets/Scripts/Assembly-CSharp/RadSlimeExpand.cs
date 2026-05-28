using System.Collections;
using System.Linq;
using UnityEngine;

public class RadSlimeExpand : SlimeSubbehaviour
{
	public SECTR_AudioCue radNormalCue;

	public SECTR_AudioCue radExpandedCue;

	private GameObject[] radObjects;

	private SECTR_PointSource radObjAudio;

	private CalmedByWaterSpray calmer;

	private float nextPossibleExpand;

	private Randoms rand;

	private bool expanding;

	private bool expanded;

	private float radObjScale;

	private const float EXPAND_MIN_DELAY = 30f;

	private const float EXPAND_MAX_DELAY = 180f;

	public const float EXPANDING_TIME = 3f;

	private const float EXPANDED_TIME = 10f;

	private const float CALMING_TIME = 0.2f;

	private const float EXPAND_FACTOR = 1.5f;

	private const float CALMED_FACTOR = 0f;

	private SlimeFaceAnimator slimeFaceAnimator;

	public override void Awake()
	{
		base.Awake();
		rand = new Randoms();
		calmer = GetComponent<CalmedByWaterSpray>();
		slimeFaceAnimator = GetComponent<SlimeFaceAnimator>();
	}

	public override void Start()
	{
		base.Start();
		ExtractRadAura();
		GetComponent<SlimeAppearanceApplicator>().OnAppearanceChanged += delegate
		{
			ExtractRadAura();
		};
		nextPossibleExpand = Time.time + ExpandDelay();
	}

	public override float Relevancy(bool isGrounded)
	{
		if (!(Time.time > nextPossibleExpand) || calmer.IsCalmed())
		{
			return 0f;
		}
		return 1f;
	}

	public override void Action()
	{
	}

	public override void Selected()
	{
		StartCoroutine(ExpandThenShrink());
	}

	public void FixedUpdate()
	{
		if (calmer.IsCalmed())
		{
			nextPossibleExpand += Time.fixedDeltaTime;
		}
		float num = 1f;
		if (expanding)
		{
			num = 1.5f;
		}
		else if (calmer.IsCalmed())
		{
			num = 0f;
		}
		else if (expanded)
		{
			num = 1.5f;
		}
		if (radObjects != null && radObjects.Length != 0)
		{
			float num2 = radObjects[0].transform.localScale.x / radObjScale;
			float num3 = ((!(num < 1f) && !(num2 < 1f)) ? (Time.fixedDeltaTime / 3f) : (Time.fixedDeltaTime / 0.2f));
			float num4 = num2;
			if (num > num2)
			{
				num4 = Mathf.Min(num2 + num3, num);
			}
			else if (num < num2)
			{
				num4 = Mathf.Max(num2 - num3, num);
			}
			float num5 = radObjScale * num4;
			GameObject[] array = radObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].transform.localScale = new Vector3(num5, num5, num5);
			}
		}
	}

	private void ExtractRadAura()
	{
		radObjects = (from radMarker in GetComponentsInChildren<RadExpandMarker>()
			select radMarker.gameObject).ToArray();
		if (radObjects.Length != 0)
		{
			radObjScale = radObjects[0].transform.localScale.x;
		}
		GameObject[] array = radObjects;
		for (int i = 0; i < array.Length; i++)
		{
			SECTR_PointSource component = array[i].GetComponent<SECTR_PointSource>();
			if (component != null)
			{
				radObjAudio = component;
				break;
			}
		}
	}

	private float ExpandDelay()
	{
		return Mathf.Lerp(30f, 180f, Mathf.Clamp(rand.GetInRange(-0.1f, 0.1f) + (1f - emotions.GetCurr(SlimeEmotions.Emotion.AGITATION)), 0f, 1f));
	}

	private IEnumerator ExpandThenShrink()
	{
		slimeFaceAnimator.SetTrigger("triggerConcentrate");
		expanding = true;
		if (radObjAudio != null)
		{
			radObjAudio.Cue = radExpandedCue;
			radObjAudio.Play();
		}
		yield return new WaitForSeconds(3f);
		expanding = false;
		expanded = true;
		yield return new WaitForSeconds(10f);
		expanded = false;
		if (radObjAudio != null)
		{
			radObjAudio.Cue = radNormalCue;
			radObjAudio.Play();
		}
		nextPossibleExpand = Time.time + ExpandDelay();
	}

	public override bool CanRethink()
	{
		return !expanding;
	}
}
