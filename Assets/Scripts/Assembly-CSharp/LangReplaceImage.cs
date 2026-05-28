using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LangReplaceImage : MonoBehaviour
{
	[Serializable]
	public class Entry
	{
		public string lang;

		public Sprite sprite;
	}

	public Entry[] replacements;

	private Image img;

	private MessageDirector msgDir;

	private Dictionary<string, Sprite> replacementDict = new Dictionary<string, Sprite>();

	private Sprite orig;

	public void Awake()
	{
		img = GetComponent<Image>();
		orig = img.sprite;
		msgDir = SRSingleton<GameContext>.Instance.MessageDirector;
		Entry[] array = replacements;
		foreach (Entry entry in array)
		{
			replacementDict[entry.lang] = entry.sprite;
		}
		msgDir.RegisterBundlesListener(OnBundlesAvailable);
	}

	public void OnBundlesAvailable(MessageDirector messageDir)
	{
		UpdateImage();
	}

	private void UpdateImage()
	{
		string key = msgDir.GetCultureLang().ToString();
		if (replacementDict.ContainsKey(key))
		{
			img.sprite = replacementDict[key];
		}
		else
		{
			img.sprite = orig;
		}
	}
}
