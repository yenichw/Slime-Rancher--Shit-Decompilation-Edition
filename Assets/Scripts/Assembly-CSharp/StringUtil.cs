using System;
using System.Linq;

public class StringUtil
{
	public static string ToString(object arg)
	{
		if (arg is string[])
		{
			return string.Join(",", (string[])arg);
		}
		if (arg is object[])
		{
			return string.Join(",", ((object[])arg).Select((object XlateText) => XlateText.ToString()).ToArray());
		}
		return Convert.ToString(arg);
	}

	public static string Pad(int val, int numDigits)
	{
		string text = string.Concat(val);
		while (text.Length < numDigits)
		{
			text = "0" + text;
		}
		return text;
	}
}
