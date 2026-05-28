using System;
using DLCPackage;

public class DLCPurgedException : Exception
{
	public readonly Id[] packages;

	public DLCPurgedException(Id[] packages)
	{
		this.packages = packages;
	}
}
