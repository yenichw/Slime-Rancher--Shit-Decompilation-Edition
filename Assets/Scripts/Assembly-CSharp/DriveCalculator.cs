using UnityEngine;

public class DriveCalculator
{
	public SlimeEmotions.Emotion emotion;

	public float extraDrive;

	public float minDrive;

	public DriveCalculator(SlimeEmotions.Emotion emotion, float extraDrive, float minDrive)
	{
		this.emotion = emotion;
		this.extraDrive = extraDrive;
		this.minDrive = minDrive;
	}

	public virtual float Drive(SlimeEmotions emotions, Identifiable.Id id)
	{
		return Mathf.Max(0f, Mathf.Max(minDrive, emotions.GetCurr(emotion)) + extraDrive);
	}
}
