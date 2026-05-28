using System;

public class StandardDateProvider : IDateProvider
{
	public DateTime GetToday()
	{
		return DateTime.Today;
	}
}
