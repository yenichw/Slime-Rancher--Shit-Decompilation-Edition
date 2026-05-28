using System;
using System.Collections.Generic;
using System.Reflection;

public class vp_Value<V> : vp_Event
{
	public delegate T Getter<T>();

	public delegate void Setter<T>(T o);

	public Getter<V> Get;

	public Setter<V> Set;

	private FieldInfo[] Fields => m_Fields;

	protected static T Empty<T>()
	{
		return default(T);
	}

	protected static void Empty<T>(T value)
	{
	}

	public V GetEmpty()
	{
		return default(V);
	}

	public void SetEmpty(V value)
	{
	}

	public vp_Value(string name)
		: base(name, typeof(V))
	{
		InitFields();
		EventType = vp_EventType.Value;
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[2]
		{
			GetType().GetField("Get"),
			GetType().GetField("Set")
		};
		StoreInvokerFieldNames();
		m_DelegateTypes = new Type[2]
		{
			typeof(Getter<>),
			typeof(Setter<>)
		};
		Prefixes = new Dictionary<string, int>
		{
			{ "get_OnValue_", 0 },
			{ "set_OnValue_", 1 }
		};
	}
}
