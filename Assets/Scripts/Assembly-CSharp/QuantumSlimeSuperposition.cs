using UnityEngine;

public class QuantumSlimeSuperposition : SlimeSubbehaviour
{
	private bool superposePerformed;

	private GenerateQuantumQubit qubitGenerator;

	private QuantumVibration quantumVibration;

	private SlimeEmotions slimeEmotions;

	private RubberBoneEffect rubberBoneEffect;

	private float nextPossibleSuperposeTime;

	public GameObject SuperposeParticleFx;

	private SlimeFaceAnimator slimeFaceAnimator;

	public float MinSuperposeDelay = 5f;

	public float MaxSuperposeDelay = 30f;

	public override void Awake()
	{
		base.Awake();
		qubitGenerator = base.gameObject.GetComponent<GenerateQuantumQubit>();
		slimeEmotions = base.gameObject.GetComponent<SlimeEmotions>();
		rubberBoneEffect = base.gameObject.GetComponentInChildren<RubberBoneEffect>();
		quantumVibration = base.gameObject.GetComponentInChildren<QuantumVibration>();
		slimeFaceAnimator = base.gameObject.GetComponent<SlimeFaceAnimator>();
		nextPossibleSuperposeTime = Time.time + MaxSuperposeDelay;
	}

	public override void Start()
	{
		nextPossibleSuperposeTime = Time.time + MaxSuperposeDelay;
	}

	public override void Action()
	{
		if (CanSuperpose())
		{
			QubitWander randomQubit = qubitGenerator.GetRandomQubit();
			if (randomQubit != null)
			{
				Superpose(randomQubit.gameObject);
				qubitGenerator.ClearQubits();
			}
		}
	}

	public override float Relevancy(bool isGrounded)
	{
		if (quantumVibration.IsVibrating() && Time.time > nextPossibleSuperposeTime && qubitGenerator.ReadyForSuperposition() && !plexer.IsCaptive())
		{
			return 1f;
		}
		return 0f;
	}

	public override void Selected()
	{
		superposePerformed = false;
	}

	public override void Deselected()
	{
		base.Deselected();
		nextPossibleSuperposeTime = Time.time + GetNextSuperposeDelay();
	}

	public override bool CanRethink()
	{
		return !CanSuperpose();
	}

	private float GetNextSuperposeDelay()
	{
		return Mathf.Lerp(MaxSuperposeDelay, MinSuperposeDelay, slimeEmotions.GetCurr(SlimeEmotions.Emotion.AGITATION));
	}

	private void Superpose(GameObject qubit)
	{
		SRBehaviour.SpawnAndPlayFX(SuperposeParticleFx, base.gameObject.transform.position, Quaternion.identity);
		SRBehaviour.SpawnAndPlayFX(SuperposeParticleFx, qubit.transform.position, Quaternion.identity);
		base.gameObject.transform.position = qubit.transform.position;
		rubberBoneEffect.Reset();
		superposePerformed = true;
	}

	private bool CanSuperpose()
	{
		if (!superposePerformed)
		{
			return !plexer.IsCaptive();
		}
		return false;
	}
}
