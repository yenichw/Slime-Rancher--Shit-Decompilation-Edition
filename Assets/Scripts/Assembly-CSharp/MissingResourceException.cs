using System;

public class MissingResourceException : Exception
{
	public MissingResourceException(string resourceName)
		: base("Missing Resource: " + resourceName)
	{
	}
}
