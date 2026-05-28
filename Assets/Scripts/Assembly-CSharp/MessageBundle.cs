using System;
using System.Collections.Generic;
using UnityEngine;

public class MessageBundle
{
	private MessageDirector msgDir;

	private string path;

	private ResourceBundle bundle;

	private MessageBundle parent;

	public void Init(MessageDirector msgDir, string path, ResourceBundle bundle, MessageBundle parent)
	{
		this.msgDir = msgDir;
		this.path = path;
		this.bundle = bundle;
		this.parent = parent;
	}

	public string Get(string key)
	{
		if (MessageUtil.IsTainted(key))
		{
			return MessageUtil.Untaint(key);
		}
		string resourceString = GetResourceString(key);
		if (resourceString == null)
		{
			return key;
		}
		return resourceString;
	}

	public void GetAll(string prefix, ICollection<string> messages, bool includeParent)
	{
		foreach (string key in bundle.GetKeys())
		{
			if (key.StartsWith(prefix))
			{
				messages.Add(Get(key));
			}
		}
		if (includeParent && parent != null)
		{
			parent.GetAll(prefix, messages, includeParent);
		}
	}

	public void GetAllKeys(string prefix, ICollection<string> keys, bool includeParent)
	{
		foreach (string key in bundle.GetKeys())
		{
			if (key.StartsWith(prefix))
			{
				keys.Add(key);
			}
		}
		if (includeParent && parent != null)
		{
			parent.GetAllKeys(prefix, keys, includeParent);
		}
	}

	public bool Exists(string key)
	{
		return GetResourceString(key, reportMissing: false) != null;
	}

	public string GetResourceString(string key)
	{
		return GetResourceString(key, reportMissing: true);
	}

	public string GetResourceString(string key, bool reportMissing)
	{
		string text = null;
		if (bundle != null)
		{
			text = bundle.GetString(key);
		}
		if (text != null)
		{
			return text;
		}
		if (parent != null)
		{
			string resourceString = parent.GetResourceString(key, reportMissing: false);
			if (resourceString != null)
			{
				return resourceString;
			}
		}
		if (reportMissing)
		{
			Log.Warning("Missing translation message", "bundle", path, "key", key);
		}
		return null;
	}

	public string Get(string key, bool reportMissing, params object[] args)
	{
		if (key.StartsWith("%"))
		{
			return msgDir.GetBundle(MessageUtil.GetBundle(key)).Get(MessageUtil.GetUnqualifiedKey(key), args);
		}
		string suffix = GetSuffix(args);
		string resourceString = GetResourceString(key + suffix, reportMissing: false);
		if (resourceString == null)
		{
			if (suffix != "")
			{
				resourceString = GetResourceString(key, reportMissing: false);
			}
			if (resourceString == null)
			{
				if (reportMissing)
				{
					Log.Warning("Missing translation message", "bundle", path, "key", key);
				}
				return key + StringUtil.ToString(args);
			}
		}
		try
		{
			return string.Format(resourceString, args);
		}
		catch (ArgumentException ex)
		{
			Log.Warning("Translation error: '" + ex.Message + "'", "bundle", path, "key", key, "msg", resourceString, "args", args, ex);
			return resourceString + StringUtil.ToString(args);
		}
	}

	public string Get(string key, params object[] args)
	{
		return Get(key, reportMissing: true, args);
	}

	public string Get(string key, params string[] args)
	{
		return Get(key, (object[])args);
	}

	public string GetSuffix(object[] args)
	{
		if (args.Length != 0 && args[0] != null)
		{
			try
			{
				int result = 0;
				if (args[0] is int)
				{
					result = (int)args[0];
				}
				else if (!int.TryParse(Convert.ToString(args[0]), out result))
				{
					return "";
				}
				switch (result)
				{
				case 0:
					return ".0";
				case 1:
					return ".1";
				default:
					return ".n";
				}
			}
			catch (FormatException)
			{
				Debug.LogWarning("Format Exception in GetSuffix");
			}
		}
		return "";
	}

	public string Xlate(string compoundKey)
	{
		if (compoundKey.StartsWith("%"))
		{
			return msgDir.GetBundle(MessageUtil.GetBundle(compoundKey)).Xlate(MessageUtil.GetUnqualifiedKey(compoundKey));
		}
		int num = compoundKey.IndexOf('|');
		if (num == -1)
		{
			return Get(compoundKey);
		}
		string key = compoundKey.Substring(0, num);
		string[] array = compoundKey.Substring(num + 1).Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			if (MessageUtil.IsTainted(array[i]))
			{
				array[i] = MessageUtil.Unescape(MessageUtil.Untaint(array[i]));
			}
			else
			{
				array[i] = Xlate(MessageUtil.Unescape(array[i]));
			}
		}
		object[] args = array;
		return Get(key, args);
	}
}
