using System;
using System.Collections.Generic;
using UnityEngine;

public class LangReplaceParticleMat : MonoBehaviour
{
	[Serializable]
	public class Entry
	{
		public string lang;

		public Material mat;
	}

	public Entry[] replacements;

	private ParticleSystemRenderer partSys;

	private MessageDirector msgDir;

	private Dictionary<string, Material> replacementDict = new Dictionary<string, Material>();

	public void Awake()
	{
		partSys = GetComponent<ParticleSystemRenderer>();
		msgDir = SRSingleton<GameContext>.Instance.MessageDirector;
		Entry[] array = replacements;
		foreach (Entry entry in array)
		{
			replacementDict[entry.lang] = entry.mat;
		}
	}

	public void OnEnable()
	{
		string key = msgDir.GetCultureLang().ToString();
		if (replacementDict.ContainsKey(key))
		{
			partSys.sharedMaterial = replacementDict[key];
		}
	}
}
