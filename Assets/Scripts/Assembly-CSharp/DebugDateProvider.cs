using System;

public class DebugDateProvider : IDateProvider
{
	private DateTime date;

	public DebugDateProvider(DateTime date)
	{
		this.date = new DateTime(date.Year, date.Month, date.Day);
	}

	public DateTime GetToday()
	{
		return date;
	}
}
