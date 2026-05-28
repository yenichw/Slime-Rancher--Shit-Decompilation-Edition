using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class vp_EventHandler : MonoBehaviour
{
	protected bool m_Initialized;

	protected Dictionary<string, vp_Event> m_HandlerEvents = new Dictionary<string, vp_Event>();

	protected Queue<EventHandlerRegistrable> m_PendingRegistrants = new Queue<EventHandlerRegistrable>();

	public vp_EventHandlerType EventHandlerType;

	protected virtual void Awake()
	{
		EventHandlerType = vp_EventHandlerType.EventHandler;
		m_Initialized = true;
		while (m_PendingRegistrants.Count > 0)
		{
			m_PendingRegistrants.Dequeue()?.Register(this);
		}
	}

	private T GetEvent<T>(string name) where T : vp_Event
	{
		vp_Event value = null;
		if (!m_HandlerEvents.TryGetValue(name, out value))
		{
			throw new Exception("Failed to find event " + name);
		}
		if (!(value is T))
		{
			throw new Exception($"Expected event {name} to be of type {typeof(T)} but was {value.GetType()}");
		}
		return (T)value;
	}

	public void RegisterMessage(string name, vp_Message.Sender onMessage)
	{
		vp_Message @event = GetEvent<vp_Message>(name);
		if (onMessage != null)
		{
			@event.Send = (vp_Message.Sender)Delegate.Combine(@event.Send, onMessage);
		}
	}

	public void RegisterMessage<T>(string name, vp_Message<T>.Sender<T> onMessage)
	{
		vp_Message<T> @event = GetEvent<vp_Message<T>>(name);
		if (onMessage != null)
		{
			@event.Send = (vp_Message<T>.Sender<T>)Delegate.Combine(@event.Send, onMessage);
		}
	}

	public void RegisterMessage<T, V>(string name, vp_Message<T, V>.Sender<T, V> onMessage)
	{
		vp_Message<T, V> @event = GetEvent<vp_Message<T, V>>(name);
		if (onMessage != null)
		{
			@event.Send = (vp_Message<T, V>.Sender<T, V>)Delegate.Combine(@event.Send, onMessage);
		}
	}

	public void UnregisterMessage(string name, vp_Message.Sender onMessage)
	{
		vp_Message @event = GetEvent<vp_Message>(name);
		if (onMessage != null)
		{
			@event.Send = (vp_Message.Sender)Delegate.Remove(@event.Send, onMessage);
		}
	}

	public void UnregisterMessage<T>(string name, vp_Message<T>.Sender<T> onMessage)
	{
		vp_Message<T> @event = GetEvent<vp_Message<T>>(name);
		if (onMessage != null)
		{
			@event.Send = (vp_Message<T>.Sender<T>)Delegate.Remove(@event.Send, onMessage);
		}
	}

	public void UnregisterMessage<T, V>(string name, vp_Message<T, V>.Sender<T, V> onMessage)
	{
		vp_Message<T, V> @event = GetEvent<vp_Message<T, V>>(name);
		if (onMessage != null)
		{
			@event.Send = (vp_Message<T, V>.Sender<T, V>)Delegate.Remove(@event.Send, onMessage);
		}
	}

	public void RegisterActivity(string name, vp_Activity.Callback onStart, vp_Activity.Callback onStop, vp_Activity.Condition canStart, vp_Activity.Condition canStop, vp_Activity.Callback onFailStart, vp_Activity.Callback onFailStop)
	{
		vp_Activity @event = GetEvent<vp_Activity>(name);
		if (onStart != null)
		{
			@event.StartCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.StartCallbacks, onStart);
		}
		if (onStop != null)
		{
			@event.StopCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.StopCallbacks, onStop);
		}
		if (canStart != null)
		{
			@event.StartConditions = (vp_Activity.Condition)Delegate.Combine(@event.StartConditions, canStart);
		}
		if (canStop != null)
		{
			@event.StopConditions = (vp_Activity.Condition)Delegate.Combine(@event.StopConditions, canStop);
		}
		if (onFailStart != null)
		{
			@event.FailStartCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.FailStartCallbacks, onFailStart);
		}
		if (onFailStop != null)
		{
			@event.FailStopCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.FailStopCallbacks, onFailStart);
		}
	}

	public void RegisterActivity<T>(string name, vp_Activity.Callback onStart, vp_Activity.Callback onStop, vp_Activity.Condition canStart, vp_Activity.Condition canStop, vp_Activity.Callback onFailStart, vp_Activity.Callback onFailStop)
	{
		vp_Activity<T> @event = GetEvent<vp_Activity<T>>(name);
		if (onStart != null)
		{
			@event.StartCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.StartCallbacks, onStart);
		}
		if (onStop != null)
		{
			@event.StopCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.StopCallbacks, onStop);
		}
		if (canStart != null)
		{
			@event.StartConditions = (vp_Activity.Condition)Delegate.Combine(@event.StartConditions, canStart);
		}
		if (canStop != null)
		{
			@event.StopConditions = (vp_Activity.Condition)Delegate.Combine(@event.StopConditions, canStop);
		}
		if (onFailStart != null)
		{
			@event.FailStartCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.FailStartCallbacks, onFailStart);
		}
		if (onFailStop != null)
		{
			@event.FailStopCallbacks = (vp_Activity.Callback)Delegate.Combine(@event.FailStopCallbacks, onFailStart);
		}
	}

	public void UnregisterActivity(string name, vp_Activity.Callback onStart, vp_Activity.Callback onStop, vp_Activity.Condition canStart, vp_Activity.Condition canStop, vp_Activity.Callback onFailStart, vp_Activity.Callback onFailStop)
	{
		vp_Activity @event = GetEvent<vp_Activity>(name);
		if (onStart != null)
		{
			@event.StartCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.StartCallbacks, onStart);
		}
		if (onStop != null)
		{
			@event.StopCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.StopCallbacks, onStop);
		}
		if (canStart != null)
		{
			@event.StartConditions = (vp_Activity.Condition)Delegate.Remove(@event.StartConditions, canStart);
		}
		if (canStop != null)
		{
			@event.StopConditions = (vp_Activity.Condition)Delegate.Remove(@event.StopConditions, canStop);
		}
		if (onFailStart != null)
		{
			@event.FailStartCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.FailStartCallbacks, onFailStart);
		}
		if (onFailStop != null)
		{
			@event.FailStopCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.FailStopCallbacks, onFailStart);
		}
	}

	public void UnregisterActivity<T>(string name, vp_Activity.Callback onStart, vp_Activity.Callback onStop, vp_Activity.Condition canStart, vp_Activity.Condition canStop, vp_Activity.Callback onFailStart, vp_Activity.Callback onFailStop)
	{
		vp_Activity<T> @event = GetEvent<vp_Activity<T>>(name);
		if (onStart != null)
		{
			@event.StartCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.StartCallbacks, onStart);
		}
		if (onStop != null)
		{
			@event.StopCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.StopCallbacks, onStop);
		}
		if (canStart != null)
		{
			@event.StartConditions = (vp_Activity.Condition)Delegate.Remove(@event.StartConditions, canStart);
		}
		if (canStop != null)
		{
			@event.StopConditions = (vp_Activity.Condition)Delegate.Remove(@event.StopConditions, canStop);
		}
		if (onFailStart != null)
		{
			@event.FailStartCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.FailStartCallbacks, onFailStart);
		}
		if (onFailStop != null)
		{
			@event.FailStopCallbacks = (vp_Activity.Callback)Delegate.Remove(@event.FailStopCallbacks, onFailStart);
		}
	}

	public void RegisterAttempt(string name, vp_Attempt.Tryer onAttempt)
	{
		vp_Attempt @event = GetEvent<vp_Attempt>(name);
		if (onAttempt != null)
		{
			@event.Try = onAttempt;
		}
	}

	public void RegisterAttempt<T>(string name, vp_Attempt<T>.Tryer<T> onAttempt)
	{
		vp_Attempt<T> @event = GetEvent<vp_Attempt<T>>(name);
		if (onAttempt != null)
		{
			@event.Try = onAttempt;
		}
	}

	public void UnregisterAttempt<T>(string name, vp_Attempt<T>.Tryer<T> onAttempt)
	{
		vp_Attempt<T> @event = GetEvent<vp_Attempt<T>>(name);
		if (onAttempt != null)
		{
			@event.Try = @event.AttemptAlwaysOK;
		}
	}

	public void UnregisterAttempt(string name, vp_Attempt.Tryer onAttempt)
	{
		vp_Attempt @event = GetEvent<vp_Attempt>(name);
		if (onAttempt != null)
		{
			@event.Try = vp_Attempt.AlwaysOK;
		}
	}

	public void RegisterValue<T>(string name, vp_Value<T>.Getter<T> onValueGet, vp_Value<T>.Setter<T> onValueSet)
	{
		vp_Value<T> @event = GetEvent<vp_Value<T>>(name);
		if (onValueGet != null)
		{
			@event.Get = onValueGet;
		}
		if (onValueSet != null)
		{
			@event.Set = onValueSet;
		}
	}

	public void UnregisterValue<T>(string name, vp_Value<T>.Getter<T> onValueGet, vp_Value<T>.Setter<T> onValueSet)
	{
		vp_Value<T> @event = GetEvent<vp_Value<T>>(name);
		if (onValueGet != null)
		{
			@event.Get = @event.GetEmpty;
		}
		if (onValueSet != null)
		{
			@event.Set = @event.SetEmpty;
		}
	}
}
