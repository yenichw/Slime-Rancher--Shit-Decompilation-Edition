using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateManager<TState, VComp> : StateManager where TState : BaseState where VComp : vp_Component
{
	protected VComp managedComponent;

	protected TState[] states;

	protected Dictionary<string, int> stateNameIndex = new Dictionary<string, int>();

	public BaseStateManager(VComp managedComponent)
	{
		this.managedComponent = managedComponent;
	}

	protected void AddState(TState state, int index)
	{
		states[index] = state;
		stateNameIndex.Add(state.name, index);
	}

	public void SetState(string stateName, bool setEnabled = true)
	{
		int value = -1;
		if (stateNameIndex.TryGetValue(stateName, out value))
		{
			if (value == states.Length - 1 && !setEnabled)
			{
				Debug.LogWarning("Warning: The 'Default' state cannot be disabled.");
				return;
			}
			states[value].Enabled = setEnabled;
			ApplyStates();
		}
	}

	public void Reset()
	{
		for (int i = 0; i < states.Length - 1; i++)
		{
			states[i].Enabled = false;
		}
		states[states.Length - 1].Enabled = true;
		ApplyStates();
	}

	public void ApplyStates()
	{
		int num = states.Length - 1;
		for (int num2 = states.Length - 1; num2 >= 0; num2--)
		{
			TState val = states[num2];
			if (val.Enabled || num2 == num)
			{
				ApplyState(val);
			}
		}
	}

	public void CombineStates()
	{
		ApplyStates();
	}

	public bool IsEnabled(string stateName)
	{
		int value = -1;
		if (stateNameIndex.TryGetValue(stateName, out value))
		{
			return states[value].Enabled;
		}
		return false;
	}

	public abstract void ApplyState(TState state);
}
