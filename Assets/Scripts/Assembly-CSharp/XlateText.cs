using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XlateText : MonoBehaviour
{
	private const string XBOX_SUFFIX = "_xbox";

	private const string XBOX_GAME_PREVIEW_SUFFIX = "_xboxgp";

	public string bundlePath = "ui";

	public string key;

	public string[] args;

	public bool addPlatformSuffix;

	private MessageBundle bundle;

	private Text text;

	private TMP_Text meshText;

	public void Awake()
	{
		text = GetComponent<Text>();
		meshText = GetComponent<TMP_Text>();
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(InitBundles);
	}

	public void OnDestroy()
	{
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(InitBundles);
		}
	}

	public void SetKey(string key)
	{
		this.key = key;
		UpdateText();
	}

	private void UpdateText()
	{
		string[] array = new string[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = bundle.Xlate(args[i]);
		}
		string text = key;
		if (this.text != null)
		{
			this.text.text = bundle.Get(text, array);
		}
		if (meshText != null)
		{
			meshText.text = bundle.Get(text, array);
		}
	}

	public void InitBundles(MessageDirector msgDir)
	{
		bundle = msgDir.GetBundle(bundlePath);
		UpdateText();
	}
}
