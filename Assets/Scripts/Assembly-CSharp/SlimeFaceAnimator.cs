using System.Collections.Generic;
using UnityEngine;

public class SlimeFaceAnimator : RegisteredActorBehaviour, RegistryUpdateable
{
	private class State
	{
		public delegate void UpdateDelegate(State state);

		public SlimeFaceAnimator anim;

		protected SlimeFace.SlimeExpression facialExpression;

		protected SECTR_AudioCue cue;

		public float startTime;

		protected UpdateDelegate updateDel;

		protected Dictionary<string, State> reacts = new Dictionary<string, State>();

		public override string ToString()
		{
			return facialExpression.ToString();
		}

		public State(SlimeFaceAnimator anim, SlimeFace.SlimeExpression facialExpression, SECTR_AudioCue cue, UpdateDelegate update)
		{
			this.facialExpression = facialExpression;
			this.anim = anim;
			this.cue = cue;
			updateDel = update;
		}

		public virtual void Init()
		{
			startTime = Time.fixedTime;
			ApplyFacialExpression(facialExpression);
			if (cue != null)
			{
				anim.slimeAudio.Play(cue);
			}
			AddReact("triggerAlarm", anim.ALARM);
			AddReact("triggerAttackTelegraph", anim.ATTACK_TELEGRAPH);
			AddReact("triggerChompOpen", anim.CHOMP_OPEN);
			AddReact("triggerChompOpenQuick", anim.CHOMP_OPEN_QUICK);
			AddReact("triggerChompClosed", anim.CHOMP_CLOSED);
			AddReact("triggerWince", anim.WINCE);
			AddReact("triggerMinorWince", anim.MINOR_WINCE);
			AddReact("triggerConcentrate", anim.INVOKE);
			AddReact("triggerGrimace", anim.GRIMACE);
			AddReact("triggerFried", anim.FRIED);
			AddReact("triggerSneeze", anim.SNEEZE);
		}

		public virtual void Update()
		{
			if (!React())
			{
				updateDel(this);
			}
		}

		protected virtual bool React()
		{
			if (anim.triggers.Count > 0)
			{
				foreach (string trigger in anim.triggers)
				{
					if (reacts.ContainsKey(trigger))
					{
						anim.SetState(reacts[trigger]);
						anim.triggers.Remove(trigger);
						return true;
					}
				}
			}
			return false;
		}

		public void AddReact(string trigger, State state)
		{
			reacts[trigger] = state;
		}

		protected void ApplyFacialExpression(SlimeFace.SlimeExpression faceExpression)
		{
			anim.appearanceApplicator.SetExpression(faceExpression);
		}
	}

	private class BlinkingState : State
	{
		private float blinkTime;

		private float unblinkTime = float.PositiveInfinity;

		private const float MIN_BLINK_GAP = 0.5f;

		private const float MAX_BLINK_GAP = 4.5f;

		private const float BLINK_TIME = 0.1f;

		public BlinkingState(SlimeFaceAnimator anim, SlimeFace.SlimeExpression facialExpression, SECTR_AudioCue cue, UpdateDelegate del)
			: base(anim, facialExpression, cue, del)
		{
		}

		public override void Init()
		{
			base.Init();
			blinkTime = Time.time + Random.Range(0.5f, 4.5f);
		}

		public override void Update()
		{
			base.Update();
			if (anim.currState != this)
			{
				return;
			}
			if (Time.time >= unblinkTime)
			{
				ApplyFacialExpression(facialExpression);
				if (anim.shouldBlush)
				{
					ApplyFacialExpression(SlimeFace.SlimeExpression.Blush);
				}
				unblinkTime = float.PositiveInfinity;
				blinkTime = Time.time + Random.Range(0.5f, 4.5f);
			}
			else if (Time.time >= blinkTime)
			{
				if (!anim.feral)
				{
					ApplyFacialExpression(facialExpression);
					ApplyFacialExpression(anim.shouldBlush ? SlimeFace.SlimeExpression.BlushBlink : SlimeFace.SlimeExpression.Blink);
				}
				unblinkTime = Time.time + 0.1f;
				blinkTime = float.PositiveInfinity;
			}
		}

		public void ClearBlinkTime()
		{
			unblinkTime = 0f;
			blinkTime = 0f;
		}
	}

	private class GlitchState : State
	{
		public GlitchState(SlimeFaceAnimator anim, SlimeFace.SlimeExpression facialExpression)
			: base(anim, facialExpression, null, delegate
			{
			})
		{
		}

		protected override bool React()
		{
			return false;
		}
	}

	private bool feral;

	private bool glitch;

	private bool seekingFood;

	private bool shouldBlush;

	public SlimeAppearanceApplicator appearanceApplicator;

	private State HAPPY;

	private State ANGRY;

	private State HUNGRY;

	private State STARVING;

	private State SCARED;

	private State ELATED;

	private State FERAL;

	private State AWE;

	private State LONG_AWE;

	private State ALARM;

	private State WINCE;

	private State MINOR_WINCE;

	private State ATTACK_TELEGRAPH;

	private State CHOMP_OPEN;

	private State CHOMP_OPEN_QUICK;

	private State CHOMP_CLOSED;

	private State INVOKE;

	private State GRIMACE;

	private State FRIED;

	private State SNEEZE;

	private State GLITCH;

	private State currState;

	private SlimeEmotions emotions;

	private SlimeAudio slimeAudio;

	private HashSet<string> triggers = new HashSet<string>();

	public void Awake()
	{
		emotions = GetComponentInParent<SlimeEmotions>();
		slimeAudio = GetComponentInParent<SlimeAudio>();
	}

	public override void Start()
	{
		base.Start();
		InitStates();
		SetState(HAPPY);
	}

	private void InitStates()
	{
		HAPPY = new BlinkingState(this, SlimeFace.SlimeExpression.Happy, null, delegate
		{
			float curr6 = emotions.GetCurr(SlimeEmotions.Emotion.AGITATION);
			float curr7 = emotions.GetCurr(SlimeEmotions.Emotion.HUNGER);
			float curr8 = emotions.GetCurr(SlimeEmotions.Emotion.FEAR);
			if (feral)
			{
				SetState(FERAL);
			}
			else if (glitch)
			{
				SetState(GLITCH);
			}
			else if (curr6 > 0.9f)
			{
				SetState(ANGRY);
			}
			else if (curr7 > 0.666f && seekingFood)
			{
				SetState(HUNGRY);
			}
			else if (curr7 > 0.99f)
			{
				SetState(STARVING);
			}
			else if (curr8 > 0.6f)
			{
				SetState(SCARED);
			}
			else if (curr6 < 0.01f && curr7 < 0.33f && curr8 < 0.01f)
			{
				SetState(ELATED);
			}
		});
		ANGRY = new State(this, SlimeFace.SlimeExpression.Angry, null, delegate
		{
			if (emotions.GetCurr(SlimeEmotions.Emotion.AGITATION) < 0.7f)
			{
				SetState(HAPPY);
			}
		});
		FERAL = new BlinkingState(this, SlimeFace.SlimeExpression.Feral, null, delegate
		{
			if (!feral)
			{
				SetState(HAPPY);
			}
		});
		HUNGRY = new State(this, SlimeFace.SlimeExpression.Hungry, null, delegate
		{
			float curr5 = emotions.GetCurr(SlimeEmotions.Emotion.HUNGER);
			if (emotions.GetCurr(SlimeEmotions.Emotion.AGITATION) > 0.9f)
			{
				SetState(ANGRY);
			}
			else if (curr5 > 0.99f)
			{
				SetState(STARVING);
			}
			else if (curr5 < 0.4f || !seekingFood)
			{
				SetState(HAPPY);
			}
		});
		STARVING = new State(this, SlimeFace.SlimeExpression.Starving, null, delegate
		{
			float curr4 = emotions.GetCurr(SlimeEmotions.Emotion.HUNGER);
			if (emotions.GetCurr(SlimeEmotions.Emotion.AGITATION) > 0.9f)
			{
				SetState(ANGRY);
			}
			else if (curr4 < 0.98f)
			{
				SetState(HAPPY);
			}
		});
		SCARED = new State(this, SlimeFace.SlimeExpression.Scared, slimeAudio.slimeSounds.voiceFearCue, delegate
		{
			if (emotions.GetCurr(SlimeEmotions.Emotion.FEAR) < 0.4f)
			{
				SetState(HAPPY);
			}
		});
		ELATED = new BlinkingState(this, SlimeFace.SlimeExpression.Elated, slimeAudio.slimeSounds.voiceFunCue, delegate
		{
			float curr = emotions.GetCurr(SlimeEmotions.Emotion.AGITATION);
			float curr2 = emotions.GetCurr(SlimeEmotions.Emotion.HUNGER);
			float curr3 = emotions.GetCurr(SlimeEmotions.Emotion.FEAR);
			if (curr > 0.02f || curr2 > 0.35f || curr3 > 0.02f)
			{
				SetState(HAPPY);
			}
		});
		AWE = new State(this, SlimeFace.SlimeExpression.Awe, slimeAudio.slimeSounds.voiceAweCue, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 1f)
			{
				SetState(HAPPY);
			}
		});
		LONG_AWE = new State(this, SlimeFace.SlimeExpression.Awe, slimeAudio.slimeSounds.voiceAweCue, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 3f)
			{
				SetState(HAPPY);
			}
		});
		ALARM = new State(this, SlimeFace.SlimeExpression.Alarm, slimeAudio.slimeSounds.voiceAlarmCue, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 1f)
			{
				SetState(HAPPY);
			}
		});
		WINCE = new State(this, SlimeFace.SlimeExpression.Wince, slimeAudio.slimeSounds.voiceDamageCue, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 1f)
			{
				SetState(HAPPY);
			}
		});
		SNEEZE = new State(this, SlimeFace.SlimeExpression.Wince, slimeAudio.slimeSounds.sneezeCue, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 1f)
			{
				SetState(HAPPY);
			}
		});
		MINOR_WINCE = new State(this, SlimeFace.SlimeExpression.Wince, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 0.2f)
			{
				SetState(HAPPY);
			}
		});
		ATTACK_TELEGRAPH = new State(this, SlimeFace.SlimeExpression.AttackTelegraph, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 1f)
			{
				SetState(HAPPY);
			}
		});
		CHOMP_OPEN = new State(this, SlimeFace.SlimeExpression.ChompOpen, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 1f)
			{
				SetState(CHOMP_CLOSED);
			}
		});
		CHOMP_OPEN_QUICK = new State(this, SlimeFace.SlimeExpression.ChompOpen, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 0.25f)
			{
				SetState(CHOMP_CLOSED);
			}
		});
		CHOMP_CLOSED = new State(this, SlimeFace.SlimeExpression.ChompClosed, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 2f)
			{
				SetState(HAPPY);
			}
		});
		INVOKE = new State(this, SlimeFace.SlimeExpression.Invoke, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + 3f)
			{
				SetState(HAPPY);
			}
		});
		GRIMACE = new State(this, SlimeFace.SlimeExpression.Grimace, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + BoomSlimeExplode.EXPLOSION_PREP_TIME)
			{
				SetState(HAPPY);
			}
		});
		FRIED = new State(this, SlimeFace.SlimeExpression.Fried, null, delegate(State state)
		{
			if (Time.fixedTime > state.startTime + BoomSlimeExplode.EXPLOSION_RECOVERY_TIME)
			{
				SetState(HAPPY);
			}
		});
		GLITCH = new GlitchState(this, SlimeFace.SlimeExpression.Glitch);
		HAPPY.AddReact("triggerAwe", AWE);
		HUNGRY.AddReact("triggerAwe", AWE);
		HAPPY.AddReact("triggerLongAwe", LONG_AWE);
		HUNGRY.AddReact("triggerLongAwe", LONG_AWE);
	}

	public void RegistryUpdate()
	{
		if (hasStarted)
		{
			currState.Update();
		}
	}

	public void SetGlitch()
	{
		glitch = true;
		SetState(GLITCH);
	}

	public void SetFeral()
	{
		feral = true;
		triggers.Clear();
		if (currState is BlinkingState)
		{
			((BlinkingState)currState).ClearBlinkTime();
		}
	}

	public void ClearFeral()
	{
		feral = false;
		if (currState != CHOMP_OPEN && currState != CHOMP_OPEN_QUICK)
		{
			triggers.Clear();
			if (currState is BlinkingState)
			{
				((BlinkingState)currState).ClearBlinkTime();
			}
		}
	}

	public void SetSeekingFood(bool val)
	{
		seekingFood = val;
	}

	public void SetShouldBlush(bool blush)
	{
		shouldBlush = blush;
	}

	public void SetTrigger(string trigger)
	{
		triggers.Add(trigger);
	}

	private void SetState(State state)
	{
		if (state != currState)
		{
			triggers.Clear();
			currState = state;
			currState.Init();
			currState.Update();
		}
	}
}
