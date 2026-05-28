using System;
using UnityEngine;

public class StyleNameAttribute : PropertyAttribute
{
	private Type styleType;

	public StyleNameAttribute(Type styleType)
	{
		this.styleType = styleType;
	}

	public Type GetStyleType()
	{
		return styleType;
	}
}
