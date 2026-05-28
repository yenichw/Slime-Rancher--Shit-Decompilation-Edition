using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class SlimeVarietyModulesStatic
{
	private static readonly string[] SKIP_PROP_NAMES = new string[3] { "sleepAngularVelocity", "sleepVelocity", "useConeFriction" };

	public static T GetCopyOf<T>(this Component copyInto, T copyFrom) where T : Component
	{
		Type type = copyInto.GetType();
		if (type != copyFrom.GetType())
		{
			return null;
		}
		while (type != typeof(Component) && type != null)
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo[] properties = type.GetProperties(bindingAttr);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!SKIP_PROP_NAMES.Contains(propertyInfo.Name) && propertyInfo.CanWrite && propertyInfo.CanRead && propertyInfo.Name != "material")
				{
					try
					{
						propertyInfo.SetValue(copyInto, propertyInfo.GetValue(copyFrom, null), null);
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat("ZOMG! Cannot set property when copying component. ", propertyInfo, " err: ", ex));
					}
				}
			}
			FieldInfo[] fields = type.GetFields(bindingAttr);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), inherit: true).Length == 0 && (fieldInfo.IsPublic || fieldInfo.GetCustomAttributes(typeof(SerializeField), inherit: true).Length != 0))
				{
					if (fieldInfo.FieldType.IsValueType)
					{
						fieldInfo.SetValue(copyInto, fieldInfo.GetValue(copyFrom));
					}
					else if (fieldInfo.FieldType.IsSerializable)
					{
						fieldInfo.SetValue(copyInto, ObjectCopier.Clone(fieldInfo.GetValue(copyFrom)));
					}
					else
					{
						fieldInfo.SetValue(copyInto, fieldInfo.GetValue(copyFrom));
					}
				}
			}
			type = type.BaseType;
		}
		return copyInto as T;
	}
}
