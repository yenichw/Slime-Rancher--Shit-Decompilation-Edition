using System;
using UnityEngine;
using UnityEngine.UI;

public class SlimeHoop : Attractor
{
	private enum Mode
	{
		IDLE = 0,
		ACTIVE = 1,
		RESETTING = 2
	}

	public Transform hoopBone;

	public GameObject scoreFX;

	public Transform scoreFxTransform;

	public GameObject endFX;

	public SECTR_AudioCue scoreCue;

	public SECTR_AudioCue endCue;

	public Text scoreText;

	public Text timeText;

	private Mode mode;

	private TimeDirector timeDir;

	private MusicDirector musicDir;

	private AchievementsDirector achieveDir;

	private float defaultVert;

	private float defaultRot;

	private double startTime;

	private double endTime;

	private int currScore;

	private int scoreToDisplay = 999;

	private int timeLeftToDisplay;

	private const float VERT_PERIOD = 660f;

	private const float VERT_FACTOR = (float)Math.PI / 330f;

	private const float VERT_RANGE = 1f;

	private const float VERT_RESET_SPD = 1f;

	private const float ROT_PERIOD = 780f;

	private const float ROT_FACTOR = 0.008055366f;

	private const float ROT_RANGE = 45f;

	private const float ROT_RESET_SPD = 90f;

	private const float DURATION = 3600f;

	private const float DOWN_FORCE = 5f;

	private const float MAX_AWE_SCORE = 10f;

	public void Awake()
	{
		timeDir = SRSingleton<SceneContext>.Instance.TimeDirector;
		musicDir = SRSingleton<GameContext>.Instance.MusicDirector;
		achieveDir = SRSingleton<SceneContext>.Instance.AchievementsDirector;
		defaultVert = hoopBone.transform.localPosition.x;
		defaultRot = hoopBone.transform.localRotation.eulerAngles.x;
	}

	public void FixedUpdate()
	{
		double num = timeDir.WorldTime();
		if (mode == Mode.ACTIVE)
		{
			hoopBone.transform.localPosition = new Vector3(CurrVert(num - startTime), hoopBone.transform.localPosition.y, hoopBone.transform.localPosition.z);
			hoopBone.transform.localRotation = Quaternion.Euler(CurrRot(num - startTime), 0f, 90f);
			if (num >= endTime)
			{
				mode = Mode.RESETTING;
				SetAweFactor(0f);
				if (endFX != null)
				{
					SRBehaviour.SpawnAndPlayFX(endFX, scoreFxTransform.position, scoreFxTransform.rotation);
				}
				if (endCue != null)
				{
					SECTR_AudioSystem.Play(endCue, scoreFxTransform.position, loop: false);
				}
			}
		}
		else if (mode == Mode.RESETTING)
		{
			bool flag = false;
			Vector3 localPosition = hoopBone.transform.localPosition;
			if (localPosition.x - defaultVert > 0.01f)
			{
				localPosition.x = Mathf.Max(defaultVert, localPosition.x - Time.fixedDeltaTime * 1f);
				flag = true;
			}
			else if (localPosition.x - defaultVert < -0.01f)
			{
				localPosition.x = Mathf.Min(defaultVert, localPosition.x + Time.fixedDeltaTime * 1f);
				flag = true;
			}
			Vector3 eulerAngles = hoopBone.transform.localRotation.eulerAngles;
			if (eulerAngles.x - defaultRot > 0.1f)
			{
				eulerAngles.x = Mathf.Max(defaultRot, eulerAngles.x - Time.fixedDeltaTime * 90f);
				flag = true;
			}
			else if (eulerAngles.x - defaultRot < -0.1f)
			{
				eulerAngles.x = Mathf.Min(defaultRot, eulerAngles.x + Time.fixedDeltaTime * 90f);
				flag = true;
			}
			if (!flag)
			{
				localPosition.x = defaultVert;
				eulerAngles.x = defaultRot;
				mode = Mode.IDLE;
			}
			hoopBone.transform.localPosition = localPosition;
			hoopBone.transform.localRotation = Quaternion.Euler(eulerAngles);
		}
		if (mode == Mode.ACTIVE)
		{
			int num2 = (int)Math.Floor(endTime - num) / 60;
			if (num2 != timeLeftToDisplay)
			{
				timeLeftToDisplay = num2;
				timeText.text = timeLeftToDisplay.ToString();
			}
		}
		else
		{
			timeText.text = "--:--";
		}
		if (scoreToDisplay != currScore)
		{
			scoreToDisplay = currScore;
			scoreText.text = scoreToDisplay.ToString();
		}
	}

	public void AddScore()
	{
		if (mode == Mode.IDLE)
		{
			mode = Mode.ACTIVE;
			startTime = timeDir.WorldTime();
			endTime = startTime + 3600.0;
			currScore = 0;
			musicDir.EnableSlimeHoopMode(endTime);
		}
		if (mode != Mode.RESETTING)
		{
			SRBehaviour.SpawnAndPlayFX(scoreFX, scoreFxTransform.position, scoreFxTransform.rotation);
			if (scoreCue != null)
			{
				SECTR_AudioSystem.Play(scoreCue, scoreFxTransform.position, loop: false);
			}
			currScore++;
			SetAweFactor(Mathf.Min(1f, (float)currScore / 10f));
			achieveDir.MaybeUpdateMaxStat(AchievementsDirector.IntStat.SLIMEBALL_SCORE, currScore);
		}
	}

	private float CurrVert(double time)
	{
		return (float)(Math.Sin(time * 0.009519978426396847) * 1.0) + defaultVert;
	}

	private float CurrRot(double time)
	{
		return (float)(Math.Sin(time * 0.008055365644395351) * 45.0) + defaultRot;
	}
}
