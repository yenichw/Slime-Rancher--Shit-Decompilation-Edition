using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class ResourceBundle
{
	private Dictionary<string, string> dict;

	private static readonly Dictionary<string, string> UNESCAPE_DICTIONARY = new Dictionary<string, string>
	{
		{ "\\=", "=" },
		{ "\\n", "\n" },
		{ "\\u00AD", "\u00ad" },
		{ "&bsol;", "\\" }
	};

	public ResourceBundle(Dictionary<string, string> dict)
	{
		this.dict = dict;
	}

	public ICollection<string> GetKeys()
	{
		return dict.Keys;
	}

	public string GetString(string key)
	{
		dict.TryGetValue(key, out var value);
		return value;
	}

	public static ResourceBundle GetBundle(string prefix, string path, CultureInfo culture, string defaultLang)
	{
		string culturePath = prefix + "/" + culture.Name + "/" + path;
		string langPath = prefix + "/" + culture.TwoLetterISOLanguageName + "/" + path;
		string defaultPath = prefix + "/" + defaultLang + "/" + path;
		return new ResourceBundle(LoadFromResources(culturePath, langPath, defaultPath));
	}

	public static Dictionary<string, string> LoadFromResources(string culturePath, string langPath, string defaultPath)
	{
		TextAsset textAsset = Resources.Load(culturePath, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			textAsset = Resources.Load(langPath, typeof(TextAsset)) as TextAsset;
		}
		if (textAsset == null)
		{
			textAsset = Resources.Load(defaultPath, typeof(TextAsset)) as TextAsset;
		}
		if (textAsset == null)
		{
			Log.Warning("Failed to read file.", "culturePath", culturePath, "langPath", langPath, "defaultPath", defaultPath);
			return new Dictionary<string, string>();
		}
		return LoadFromTextAsset(textAsset);
	}

	public static Dictionary<string, string> LoadFromTextAsset(TextAsset file)
	{
		return LoadFromText(file.name, file.text);
	}

	public static Dictionary<string, string> LoadFromText(string path, string text)
	{
		List<string> list = new List<string>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = Regex.Replace(text, "\\\\(\\r\\n|\\n|\\r)[ \\t]*", "").Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		if (list == null)
		{
			Log.Warning("Resource is empty. '" + path + "'");
			return new Dictionary<string, string>();
		}
		foreach (string item2 in list)
		{
			if (item2.Length > 1 && item2[0] != '#')
			{
				string[] array2 = Regex.Split(item2, "(?<!(?<!\\\\)*\\\\)\\=");
				if (array2.Length != 2)
				{
					Log.Warning("Illegal resource bundle line", "path", path, "line", item2);
				}
				else
				{
					dictionary[Unescape(array2[0]).Trim()] = Unescape(array2[1]).Trim();
				}
			}
		}
		return dictionary;
	}

	private static string Unescape(string s)
	{
		foreach (KeyValuePair<string, string> item in UNESCAPE_DICTIONARY)
		{
			s = s.Replace(item.Key, item.Value);
		}
		return s;
	}
}
