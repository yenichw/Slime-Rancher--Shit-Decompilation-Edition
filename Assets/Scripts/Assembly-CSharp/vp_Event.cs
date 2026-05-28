using System;
using System.Collections.Generic;
using System.Reflection;

public abstract class vp_Event
{
	protected string m_Name;

	protected Type m_ArgumentType;

	protected Type m_ReturnType;

	protected FieldInfo[] m_Fields;

	protected Type[] m_DelegateTypes;

	protected MethodInfo[] m_DefaultMethods;

	public string[] InvokerFieldNames;

	public Dictionary<string, int> Prefixes;

	public vp_EventType EventType;

	public string EventName => m_Name;

	public Type ArgumentType => m_ArgumentType;

	public Type ReturnType => m_ReturnType;

	protected abstract void InitFields();

	public vp_Event(string name = "", Type eventArgumentType = null, Type eventReturnType = null)
	{
		EventType = vp_EventType.Event;
		m_ArgumentType = eventArgumentType;
		if (eventReturnType == null)
		{
			m_ReturnType = typeof(void);
		}
		else
		{
			m_ReturnType = eventReturnType;
		}
		m_Name = name;
	}

	protected void StoreInvokerFieldNames()
	{
		InvokerFieldNames = new string[m_Fields.Length];
		for (int i = 0; i < m_Fields.Length; i++)
		{
			InvokerFieldNames[i] = m_Fields[i].Name;
		}
	}

	protected void Refresh()
	{
	}
}
