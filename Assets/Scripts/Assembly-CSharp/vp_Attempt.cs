using System;
using System.Collections.Generic;
using System.Reflection;

public class vp_Attempt : vp_Event
{
	public delegate bool Tryer();

	public Tryer Try;

	public static bool AlwaysOK()
	{
		return true;
	}

	public vp_Attempt(string name, Type eventArgumentType = null)
		: base(name, eventArgumentType, typeof(bool))
	{
		InitFields();
		EventType = vp_EventType.Attempt;
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Try") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { GetType().GetMethod("AlwaysOK") };
		m_DelegateTypes = new Type[1] { typeof(Tryer) };
		Prefixes = new Dictionary<string, int> { { "OnAttempt_", 0 } };
		Try = AlwaysOK;
	}
}
public class vp_Attempt<V> : vp_Attempt
{
	public new delegate bool Tryer<T>(T value);

	public new Tryer<V> Try;

	protected static bool AlwaysOK<T>(T value)
	{
		return true;
	}

	public bool AttemptAlwaysOK(V value)
	{
		return true;
	}

	public vp_Attempt(string name)
		: base(name, typeof(V))
	{
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { GetType().GetField("Try") };
		StoreInvokerFieldNames();
		m_DelegateTypes = new Type[1] { typeof(Tryer<>) };
		Prefixes = new Dictionary<string, int> { { "OnAttempt_", 0 } };
		Try = AttemptAlwaysOK;
	}
}
