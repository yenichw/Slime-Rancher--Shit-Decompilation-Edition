using System.Collections.Generic;
using UnityEngine;

public class GordoFaceAnimator : MonoBehaviour
{
	private class State
	{
		public delegate void UpdateDelegate(State state);

		public GordoFaceAnimator anim;

		protected Material eyes;

		protected Material mouth;

		protected SECTR_AudioCue cue;

		public float startTime;

		protected UpdateDelegate updateDel;

		protected Dictionary<string, State> reacts = new Dictionary<string, State>();

		public override string ToString()
		{
			return eyes.name + ":" + mouth.name;
		}

		public State(GordoFaceAnimator anim, Material eyes, Material mouth, SECTR_AudioCue cue, UpdateDelegate update)
		{
			this.anim = anim;
			this.eyes = eyes;
			this.mouth = mouth;
			this.cue = cue;
			updateDel = update;
		}

		public virtual void Init()
		{
			startTime = Time.fixedTime;
			ApplyMats(eyes, mouth);
			if (cue != null)
			{
				anim.slimeAudio.Play(cue);
			}
			AddReact("Strain", anim.STRAIN);
		}

		public virtual void Update()
		{
			if (!React())
			{
				updateDel(this);
			}
		}

		private bool React()
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

		protected void ApplyMats(Material eyes, Material mouth)
		{
			Renderer[] renderers = anim.renderers;
			foreach (Renderer obj in renderers)
			{
				Material[] sharedMaterials = obj.sharedMaterials;
				sharedMaterials[1] = eyes;
				sharedMaterials[2] = mouth;
				obj.sharedMaterials = sharedMaterials;
			}
		}
	}

	private class BlinkingState : State
	{
		private float blinkTime;

		private float unblinkTime = float.PositiveInfinity;

		private float MIN_BLINK_GAP = 0.5f;

		private float MAX_BLINK_GAP = 1f;

		private float BLINK_TIME = 0.1f;

		public BlinkingState(GordoFaceAnimator anim, Material eyes, Material mouth, SECTR_AudioCue cue, UpdateDelegate del)
			: base(anim, eyes, mouth, cue, del)
		{
		}

		public override void Init()
		{
			base.Init();
			blinkTime = Time.time + Random.Range(MIN_BLINK_GAP, MAX_BLINK_GAP);
		}

		public override void Update()
		{
			base.Update();
			if (Time.time >= unblinkTime)
			{
				ApplyMats(eyes, mouth);
				unblinkTime = float.PositiveInfinity;
				blinkTime = Time.time + Random.Range(MIN_BLINK_GAP, MAX_BLINK_GAP);
			}
			else if (Time.time >= blinkTime)
			{
				ApplyMats(anim.comps.blinkEyes, mouth);
				unblinkTime = Time.time + BLINK_TIME;
				blinkTime = float.PositiveInfinity;
			}
		}
	}

	public const string STRAIN_TRIGGER = "Strain";

	private bool inVac;

	private State HAPPY;

	private State HUNGRY;

	private State STRAIN;

	private State currState;

	private SlimeAudio slimeAudio;

	private HashSet<string> triggers = new HashSet<string>();

	private GordoFaceComponents comps;

	private Renderer[] renderers;

	public void Awake()
	{
		comps = GetComponentInParent<GordoFaceComponents>();
		slimeAudio = GetComponentInParent<SlimeAudio>();
		List<Renderer> list = new List<Renderer>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.sharedMaterials.Length == 3)
			{
				list.Add(renderer);
			}
		}
		renderers = list.ToArray();
	}

	public void Start()
	{
		InitStates();
		SetDefaultState();
	}

	private void InitStates()
	{
		HAPPY = new State(this, comps.blinkEyes, comps.happyMouth, null, delegate
		{
			if (inVac)
			{
				SetState(HUNGRY);
			}
		});
		HUNGRY = new State(this, comps.blinkEyes, comps.chompOpenMouth, null, delegate
		{
			if (!inVac)
			{
				SetState(HAPPY);
			}
		});
		STRAIN = new State(this, comps.strainEyes, comps.strainMouth, null, delegate
		{
		});
	}

	public void Update()
	{
		currState.Update();
	}

	public void SetInVac(bool val)
	{
		inVac = val;
	}

	public void SetTrigger(string trigger)
	{
		triggers.Add(trigger);
	}

	public void SetDefaultState()
	{
		SetState(HAPPY);
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
