using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SRAnimator : SRBehaviour
{
	private class HibernatingParameter
	{
		public readonly Action Restore;

		public HibernatingParameter(Animator animator, AnimatorControllerParameter parameter)
		{
			switch (parameter.type)
			{
			case AnimatorControllerParameterType.Bool:
			{
				bool current3 = animator.GetBool(parameter.nameHash);
				Restore = delegate
				{
					animator.SetBool(parameter.nameHash, current3);
				};
				break;
			}
			case AnimatorControllerParameterType.Float:
			{
				float current2 = animator.GetFloat(parameter.nameHash);
				Restore = delegate
				{
					animator.SetFloat(parameter.nameHash, current2);
				};
				break;
			}
			case AnimatorControllerParameterType.Int:
			{
				int current = animator.GetInteger(parameter.nameHash);
				Restore = delegate
				{
					animator.SetInteger(parameter.nameHash, current);
				};
				break;
			}
			case AnimatorControllerParameterType.Trigger:
				Restore = delegate
				{
				};
				break;
			default:
				throw new NotImplementedException($"Failed to hibernate SRAnimator parameter. [animator={animator.name}; parameter={parameter.name}; type={parameter.type}]");
			}
		}
	}

	private List<HibernatingParameter> hibernatingParameters;

	private List<AnimatorStateInfo> hibernatingStates;

	public Animator animator { get; private set; }

	public virtual void Awake()
	{
		animator = GetRequiredComponent<Animator>();
	}

	public virtual void OnEnable()
	{
		if (hibernatingParameters != null)
		{
			hibernatingParameters.ForEach(delegate(HibernatingParameter p)
			{
				p.Restore();
			});
			hibernatingParameters = null;
		}
		if (hibernatingStates != null)
		{
			for (int i = 0; i < hibernatingStates.Count; i++)
			{
				AnimatorStateInfo animatorStateInfo = hibernatingStates[i];
				animator.Play(animatorStateInfo.shortNameHash, i, animatorStateInfo.normalizedTime);
			}
			hibernatingStates = null;
		}
	}

	public virtual void OnDisable()
	{
		hibernatingParameters = animator.parameters.Select((AnimatorControllerParameter p) => new HibernatingParameter(animator, p)).ToList();
		hibernatingStates = (from ii in Enumerable.Range(0, animator.layerCount)
			select animator.GetCurrentAnimatorStateInfo(ii)).ToList();
	}
}
public abstract class SRAnimator<T> : SRAnimator
{
	public T parent { get; private set; }

	public override void Awake()
	{
		base.Awake();
		parent = GetComponentInParent<T>();
	}
}
