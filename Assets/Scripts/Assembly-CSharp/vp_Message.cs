using System;
using System.Collections.Generic;
using System.Reflection;

public class vp_Message : vp_Event
{
	public delegate void Sender();

	public Sender Send;

	protected static void Empty()
	{
	}

	public vp_Message(string name, Type eventArgumentType = null, Type eventReturnType = null)
		: base(name, eventArgumentType, eventReturnType)
	{
		InitFields();
		EventType = vp_EventType.Message;
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Send") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { GetType().GetMethod("Empty") };
		m_DelegateTypes = new Type[1] { typeof(Sender) };
		Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		Send = Empty;
	}

	public void Register(Sender sender)
	{
		Send = (Sender)Delegate.Combine(Send, sender);
		Refresh();
	}
}
public class vp_Message<V> : vp_Message
{
	public new delegate void Sender<T>(T value);

	public new Sender<V> Send;

	protected static void Empty<T>(T value)
	{
	}

	public void EmptySender(V value)
	{
	}

	public vp_Message(string name)
		: base(name, typeof(V))
	{
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Send") };
		StoreInvokerFieldNames();
		m_DelegateTypes = new Type[1] { typeof(Sender<>) };
		Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		Send = EmptySender;
	}
}
public class vp_Message<V, VResult> : vp_Message
{
	public new delegate TResult Sender<T, TResult>(T value);

	public new Sender<V, VResult> Send;

	protected static TResult Empty<T, TResult>(T value)
	{
		return default(TResult);
	}

	protected VResult EmptySender(V value)
	{
		return default(VResult);
	}

	public vp_Message(string name)
		: base(name, typeof(V), typeof(VResult))
	{
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Send") };
		StoreInvokerFieldNames();
		m_DelegateTypes = new Type[1] { typeof(Sender<, >) };
		Prefixes = new Dictionary<string, int> { { "OnMessage_", 0 } };
		Send = EmptySender;
	}
}
