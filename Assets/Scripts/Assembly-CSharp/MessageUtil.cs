using System;
using System.Text;
using System.Text.RegularExpressions;

public class MessageUtil
{
	public const string QUAL_PREFIX = "%";

	public const string QUAL_SEP = ":";

	private const string TAINT_CHAR = "~";

	public static string Taint(object text)
	{
		return "~" + text;
	}

	public static bool IsTainted(string text)
	{
		return text?.StartsWith("~") ?? false;
	}

	public static string Untaint(string text)
	{
		if (!IsTainted(text))
		{
			return text;
		}
		return text.Substring("~".Length);
	}

	public static string Compose(string key, params object[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(key);
		stringBuilder.Append('|');
		for (int i = 0; i < args.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append('|');
			}
			string text = ((args[i] == null) ? "" : Convert.ToString(args[i]));
			int length = text.Length;
			for (int j = 0; j < length; j++)
			{
				char c = text[j];
				switch (c)
				{
				case '|':
					stringBuilder.Append("\\!");
					break;
				case '\\':
					stringBuilder.Append("\\\\");
					break;
				default:
					stringBuilder.Append(c);
					break;
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string Compose(string key, params string[] args)
	{
		return Compose(key, (object[])args);
	}

	public static string Unescape(string value)
	{
		if (value.IndexOf('\\') == -1)
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (c != '\\' || i == length - 1)
			{
				stringBuilder.Append(c);
				continue;
			}
			c = value[++i];
			stringBuilder.Append((c == '!') ? '|' : c);
		}
		return stringBuilder.ToString();
	}

	public static string Tcompose(string key, params object[] args)
	{
		int num = args.Length;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Taint(args[i]);
		}
		object[] args2 = array;
		return Compose(key, args2);
	}

	public static string Tcompose(string key, params string[] args)
	{
		int i = 0;
		for (int num = args.Length; i < num; i++)
		{
			args[i] = Taint(args[i]);
		}
		return Compose(key, args);
	}

	public static string[] decompose(string compoundKey)
	{
		string[] array = Regex.Split(compoundKey, "\\|");
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Unescape(array[i]);
		}
		return array;
	}

	public static string Qualify(string bundle, string key)
	{
		if (bundle.IndexOf("%") != -1 || bundle.IndexOf(":") != -1)
		{
			throw new ArgumentException("Message bundle may not contain '%' or ':' [bundle=" + bundle + ", key=" + key + "]");
		}
		return "%" + bundle + ":" + key;
	}

	public static string GetBundle(string qualifiedKey)
	{
		if (!qualifiedKey.StartsWith("%"))
		{
			throw new ArgumentException(qualifiedKey + " is not a fully qualified message key.");
		}
		int num = qualifiedKey.IndexOf(":");
		if (num == -1)
		{
			throw new ArgumentException(qualifiedKey + " is not a valid fully qualified key.");
		}
		return qualifiedKey.Substring("%".Length, num - "%".Length);
	}

	public static string GetUnqualifiedKey(string qualifiedKey)
	{
		if (!qualifiedKey.StartsWith("%"))
		{
			throw new ArgumentException(qualifiedKey + " is not a fully qualified message key.");
		}
		int num = qualifiedKey.IndexOf(":");
		if (num == -1)
		{
			throw new ArgumentException(qualifiedKey + " is not a valid fully qualified key.");
		}
		return qualifiedKey.Substring(num + 1);
	}
}
