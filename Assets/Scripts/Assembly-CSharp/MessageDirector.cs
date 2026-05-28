using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MessageDirector : MonoBehaviour
{
	public enum Lang
	{
		EN = 0,
		DE = 1,
		ES = 2,
		FR = 3,
		RU = 4,
		SV = 5,
		ZH = 6,
		JA = 7,
		PT = 8,
		KO = 9
	}

	public delegate void BundlesListener(MessageDirector msgDir);

	private BundlesListener bundlesListeners;

	public const string GLOBAL_BUNDLE = "global";

	public string msgPath = "I18n";

	public Lang defaultLang;

	public string fallbackLang = "en";

	private CultureInfo culture;

	private Dictionary<string, MessageBundle> cache = new Dictionary<string, MessageBundle>();

	private MessageBundle global;

	private const string MBUNDLE_CLASS_KEY = "msgbundle_class";

	public static Lang GetLang(string code)
	{
		string value = code.ToUpperInvariant();
		if (Enum.IsDefined(typeof(Lang), value))
		{
			return (Lang)Enum.Parse(typeof(Lang), value);
		}
		return Lang.EN;
	}

	public static Lang GetLang(CultureInfo culture)
	{
		return GetLang(culture.TwoLetterISOLanguageName);
	}

	public void Awake()
	{
		SetCulture(GetLang(CultureInfo.CurrentCulture));
	}

	public CultureInfo GetCulture()
	{
		return culture;
	}

	private void SetCulture(SystemLanguage systemLanguage)
	{
		Debug.Log("Setting Culture given SystemLanguage: " + systemLanguage);
		switch (Application.systemLanguage)
		{
		case SystemLanguage.German:
			SetCulture(Lang.DE);
			break;
		case SystemLanguage.French:
			SetCulture(Lang.FR);
			break;
		case SystemLanguage.Russian:
			SetCulture(Lang.RU);
			break;
		case SystemLanguage.Spanish:
			SetCulture(Lang.ES);
			break;
		case SystemLanguage.Swedish:
			SetCulture(Lang.SV);
			break;
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
		case SystemLanguage.ChineseTraditional:
			SetCulture(Lang.ZH);
			break;
		default:
			SetCulture(Lang.EN);
			break;
		}
	}

	private void SetCulture(CultureInfo culture)
	{
		SetCulture(culture, updateGlobal: true);
	}

	public string GetCurrentLanguageCode()
	{
		return GetCulture().TwoLetterISOLanguageName;
	}

	public void SetCulture(Lang lang)
	{
		SetCulture(GetCultureInfo(lang));
	}

	public static CultureInfo GetCultureInfo(Lang lang)
	{
		string text = lang.ToString();
		if (text == "ZH")
		{
			text = "ZH-HANS";
		}
		return CultureInfo.GetCultureInfo(text);
	}

	private void SetCulture(CultureInfo culture, bool updateGlobal)
	{
		if (this.culture != culture)
		{
			this.culture = culture;
			cache.Clear();
			if (updateGlobal)
			{
				global = GetBundle("global");
			}
			Log.Info("", "Culture", culture);
			if (bundlesListeners != null)
			{
				bundlesListeners(this);
			}
		}
	}

	public Lang GetCultureLang()
	{
		return GetLang(culture);
	}

	public void RegisterBundlesListener(BundlesListener avail)
	{
		bundlesListeners = (BundlesListener)Delegate.Combine(bundlesListeners, avail);
		avail(this);
	}

	public void UnregisterBundlesListener(BundlesListener avail)
	{
		bundlesListeners = (BundlesListener)Delegate.Remove(bundlesListeners, avail);
	}

	public MessageBundle GetBundle(string path)
	{
		if (cache.ContainsKey(path))
		{
			return cache[path];
		}
		ResourceBundle resourceBundle = LoadBundle(path);
		MessageBundle customBundle = null;
		if (resourceBundle != null)
		{
			string text = null;
			try
			{
				text = resourceBundle.GetString("msgbundle_class");
				if (text != null)
				{
					text = text.Trim();
					if (text != "")
					{
						customBundle = Type.GetType(text).GetConstructor(new Type[0]).Invoke(new object[0]) as MessageBundle;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("Failure instantiating custom message bundle", "mbclass", text, "error", ex);
			}
		}
		MessageBundle messageBundle = CreateBundle(path, resourceBundle, customBundle);
		cache[path] = messageBundle;
		return messageBundle;
	}

	public string Get(string path, string key)
	{
		return GetBundle(path).Get(key);
	}

	public string Get(string path, string key, params object[] args)
	{
		return GetBundle(path).Get(key, args);
	}

	protected MessageBundle CreateBundle(string path, ResourceBundle rbundle, MessageBundle customBundle)
	{
		if (customBundle == null)
		{
			customBundle = new MessageBundle();
		}
		InitBundle(customBundle, path, rbundle);
		return customBundle;
	}

	protected void InitBundle(MessageBundle bundle, string path, ResourceBundle rbundle)
	{
		MessageBundle bundle2 = global;
		if (rbundle != null)
		{
			string @string = rbundle.GetString("__parent");
			if (@string != null)
			{
				bundle2 = GetBundle(@string);
			}
		}
		bundle.Init(this, path, rbundle, bundle2);
	}

	protected ResourceBundle LoadBundle(string path)
	{
		try
		{
			return ResourceBundle.GetBundle(msgPath, path, culture, fallbackLang);
		}
		catch (MissingResourceException ex)
		{
			Log.Warning("Unable to resolve resource bundle", "path", path, "culture", culture, ex);
			return null;
		}
	}
}
