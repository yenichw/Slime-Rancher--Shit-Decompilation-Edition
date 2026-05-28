using UnityEngine;

public class SwitchHandler
{
	public enum State
	{
		UP = 0,
		DOWN = 1
	}

	public abstract class Switchable : SRBehaviour
	{
		public abstract void SetState(State state, bool immediate);
	}

	private Animator anim;

	private int animUpId;

	private int animUpImmediateId;

	private int animDownImmediateId;

	public SwitchHandler(Animator anim, GameObject go)
	{
		this.anim = anim;
		animUpId = Animator.StringToHash("Up");
		animUpImmediateId = Animator.StringToHash("UpImmediate");
		animDownImmediateId = Animator.StringToHash("DownImmediate");
	}

	public void SetState(State state, bool immediate)
	{
		bool flag = state == State.UP;
		if (flag == anim.GetBool(animUpId))
		{
			return;
		}
		anim.SetBool(animUpId, flag);
		if (immediate)
		{
			if (flag)
			{
				anim.SetTrigger(animUpImmediateId);
			}
			else
			{
				anim.SetTrigger(animDownImmediateId);
			}
		}
	}
}
