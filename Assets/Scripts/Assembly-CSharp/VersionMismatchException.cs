using System;

public class VersionMismatchException : Exception
{
	public VersionMismatchException(string msg)
		: base(msg)
	{
	}
}
