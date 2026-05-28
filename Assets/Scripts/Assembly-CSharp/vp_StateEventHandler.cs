using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class vp_StateEventHandler : vp_EventHandler
{
	private List<vp_Component> m_StateTargets = new List<vp_Component>();

	protected override void Awake()
	{
		EventHandlerType = vp_EventHandlerType.StateEventHandler;
		base.Awake();
		vp_Component[] componentsInChildren = base.transform.root.GetComponentsInChildren<vp_Component>(includeInactive: true);
		foreach (vp_Component vp_Component2 in componentsInChildren)
		{
			if (vp_Component2.Parent == null || vp_Component2.Parent.GetComponent<vp_Component>() == null)
			{
				m_StateTargets.Add(vp_Component2);
			}
		}
	}

	protected void BindStateToActivity(vp_Activity a)
	{
		BindStateToActivityOnStart(a);
		BindStateToActivityOnStop(a);
	}

	protected void BindStateToActivityOnStart(vp_Activity a)
	{
		if (!ActivityInitialized(a))
		{
			return;
		}
		string s = a.EventName;
		a.StartCallbacks = (vp_Activity.Callback)Delegate.Combine(a.StartCallbacks, (vp_Activity.Callback)delegate
		{
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(s, enabled: true, recursive: true);
			}
		});
	}

	protected void BindStateToActivityOnStop(vp_Activity a)
	{
		if (!ActivityInitialized(a))
		{
			return;
		}
		string s = a.EventName;
		a.StopCallbacks = (vp_Activity.Callback)Delegate.Combine(a.StopCallbacks, (vp_Activity.Callback)delegate
		{
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(s, enabled: false, recursive: true);
			}
		});
	}

	public void RefreshActivityStates()
	{
		foreach (vp_Event value in m_HandlerEvents.Values)
		{
			if (value.EventType != vp_EventType.Activity)
			{
				continue;
			}
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(value.EventName, ((vp_Activity)value).Active, recursive: true);
			}
		}
	}

	public void ResetActivityStates()
	{
		foreach (vp_Component stateTarget in m_StateTargets)
		{
			stateTarget.ResetState();
		}
	}

	public void SetState(string state, bool setActive = true, bool recursive = true, bool includeDisabled = false)
	{
		foreach (vp_Component stateTarget in m_StateTargets)
		{
			stateTarget.SetState(state, setActive, recursive, includeDisabled);
		}
	}

	private bool ActivityInitialized(vp_Activity a)
	{
		if (a == null)
		{
			Debug.LogError(string.Concat("Error: (", this, ") Activity is null."));
			return false;
		}
		if (string.IsNullOrEmpty(a.EventName))
		{
			Debug.LogError(string.Concat("Error: (", this, ") Activity not initialized. Make sure the event handler has run its Awake call before binding layers."));
			return false;
		}
		return true;
	}
}
