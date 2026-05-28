using UnityEngine;

public class QuantumVibration : MonoBehaviour
{
	private enum State
	{
		Vibrating = 0,
		Calm = 1
	}

	private State currentState;

	private SlimeEmotions slimeEmotions;

	private QuantumVibrationMarker vibrationMarker;

	public GameObject VibratingFX;

	public float AgitationCutoff = 0.1f;

	public void Awake()
	{
		slimeEmotions = base.gameObject.GetComponent<SlimeEmotions>();
	}

	public void Start()
	{
		vibrationMarker = base.gameObject.GetComponentInChildren<QuantumVibrationMarker>();
		VibratingFX = vibrationMarker.gameObject;
		switch (currentState)
		{
		case State.Calm:
			vibrationMarker.PlayCalm();
			break;
		case State.Vibrating:
			vibrationMarker.PlayVibrating();
			break;
		}
	}

	public bool IsVibrating()
	{
		return currentState == State.Vibrating;
	}

	public float GetVibrationLevel()
	{
		if (slimeEmotions.GetCurr(SlimeEmotions.Emotion.AGITATION) < AgitationCutoff)
		{
			return 0f;
		}
		float num = 1f / (1f - AgitationCutoff);
		float num2 = 0f - num * AgitationCutoff;
		return Mathf.Clamp(num * slimeEmotions.GetCurr(SlimeEmotions.Emotion.AGITATION) + num2, 0f, 1f);
	}

	public void FixedUpdate()
	{
		switch (currentState)
		{
		case State.Calm:
			CalmUpdate();
			break;
		case State.Vibrating:
			VibratingUpdate();
			break;
		default:
			Log.Warning("Unexpected state in QuantumVibration.");
			break;
		}
	}

	private void CalmUpdate()
	{
		if (IsAgitated())
		{
			currentState = State.Vibrating;
			VibratingFX.SetActive(value: true);
			vibrationMarker.PlayVibrating();
		}
	}

	private void VibratingUpdate()
	{
		if (!IsAgitated())
		{
			currentState = State.Calm;
			VibratingFX.SetActive(value: false);
			vibrationMarker.PlayCalm();
		}
	}

	private bool IsAgitated()
	{
		return slimeEmotions.GetCurr(SlimeEmotions.Emotion.AGITATION) > AgitationCutoff;
	}
}
