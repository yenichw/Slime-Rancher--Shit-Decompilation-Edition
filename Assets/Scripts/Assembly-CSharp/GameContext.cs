using System;
using System.Collections;
using System.IO;
using System.Text;
using RichPresence;
using UnityEngine;

public class GameContext : SRSingleton<GameContext>
{
	public struct TakeScreenshot_Params
	{
		public string directory;

		public string name;
	}

	public MessageOfTheDayDirector MessageOfTheDayDirector;

	public SlimeDefinitions SlimeDefinitions;

	private const int LOG_MAX_CHARACTERS = 51200;

	private const int LOG_TRUNCATE_AMOUNT = 25600;

	private StringBuilder partialLogText = new StringBuilder();

	private const int MAX_FRAME_RATE = 120;

	public LookupDirector LookupDirector { get; private set; }

	public AutoSaveDirector AutoSaveDirector { get; private set; }

	public SlimeShaders SlimeShaders { get; private set; }

	public MessageDirector MessageDirector { get; private set; }

	public UITemplates UITemplates { get; private set; }

	public InputDirector InputDirector { get; private set; }

	public MusicDirector MusicDirector { get; private set; }

	public OptionsDirector OptionsDirector { get; private set; }

	public GifRecorder GifRecorder { get; private set; }

	public PerformanceTracker PerformanceTracker { get; private set; }

	public GalaxyDirector GalaxyDirector { get; private set; }

	public RailDirector RailDirector { get; private set; }

	public Director RichPresenceDirector { get; private set; }

	public DLCDirector DLCDirector { get; private set; }

	public RaycastBatcher RaycastBatcher { get; private set; }

	public ToyDirector ToyDirector { get; private set; }

	public string LogText => partialLogText.ToString();

	public override void Awake()
	{
		if (SRSingleton<GameContext>.Instance == null)
		{
			base.Awake();
			LookupDirector = GetComponent<LookupDirector>();
			AutoSaveDirector = GetComponent<AutoSaveDirector>();
			SlimeShaders = GetComponent<SlimeShaders>();
			UITemplates = GetComponent<UITemplates>();
			InputDirector = GetComponent<InputDirector>();
			MessageDirector = GetComponent<MessageDirector>();
			MusicDirector = GetComponent<MusicDirector>();
			OptionsDirector = GetComponent<OptionsDirector>();
			GifRecorder = GetComponent<GifRecorder>();
			PerformanceTracker = GetComponent<PerformanceTracker>();
			GalaxyDirector = GetComponent<GalaxyDirector>();
			RailDirector = GetComponent<RailDirector>();
			RaycastBatcher = GetComponent<RaycastBatcher>();
			RichPresenceDirector = new Director();
			DLCDirector = new DLCDirector();
			ToyDirector = new ToyDirector();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			string[] joystickNames = Input.GetJoystickNames();
			Debug.Log(string.Format("Joystick Names: {0}", string.Join(",", joystickNames)));
			Application.logMessageReceived += LogReceived;
			Application.targetFrameRate = 120;
		}
		else if (SRSingleton<GameContext>.Instance != this)
		{
			Destroyer.Destroy(base.gameObject, "GameContext.Awake");
		}
	}

	public void Start()
	{
		RichPresenceDirector.Register(SRSingleton<SystemContext>.Instance.GameCoreXboxContext);
		RichPresenceDirector.Register(SRSingleton<SystemContext>.Instance.UWPContext);
		RichPresenceDirector.Register(SRSingleton<SystemContext>.Instance.PS4Context);
		RichPresenceDirector.Register(Discord.RichPresenceHandler);
		MessageOfTheDayProvider provider = MessageOfTheDayDirector.GetProvider();
		if (provider is MessageOfTheDayLocalProvider)
		{
			((MessageOfTheDayLocalProvider)provider).SetDLCDirector(DLCDirector);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Application.logMessageReceived -= LogReceived;
	}

	public void TakeGifScreenshot()
	{
		GifRecorder.MaybeSaveGif();
	}

	public void TakeScreenshot()
	{
		TakeScreenshot(default(TakeScreenshot_Params));
	}

	public void TakeScreenshot(TakeScreenshot_Params args)
	{
		StartCoroutine(TakeScreenshotAsync(args));
	}

	private IEnumerator TakeScreenshotAsync(TakeScreenshot_Params args)
	{
		string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), args.directory ?? string.Empty), args.name ?? $"SlimeRancher-{DateTime.Now:yyyy-MM-dd-hh-mm-ss-ff}.png");
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		File.Delete(path);
		yield return null;
		GameObject hudRoot = null;
		if (SRSingleton<HudUI>.Instance != null)
		{
			hudRoot = SRSingleton<HudUI>.Instance.transform.parent.gameObject;
		}
		if (hudRoot != null)
		{
			hudRoot.SetActive(value: false);
		}
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(path);
		if (hudRoot != null)
		{
			hudRoot.SetActive(value: true);
		}
	}

	private void LogReceived(string message, string stacktrace, LogType type)
	{
		partialLogText.AppendLine($"{DateTime.UtcNow.ToString()} +0000;[{type.ToString().ToUpperInvariant()}];{message}");
		if (!string.IsNullOrEmpty(stacktrace))
		{
			stacktrace = stacktrace.Replace("\n", "\n\t");
			stacktrace = stacktrace.TrimEnd('\t');
			if (stacktrace != "" && type != LogType.Log)
			{
				partialLogText.Append($"\t{stacktrace}");
			}
		}
		if (Truncate(partialLogText, 51200, 25600))
		{
			partialLogText.Insert(0, $"{DateTime.UtcNow} +0000;[LOG];Truncated logs due to string size limit of approximately {51200}KB\n");
		}
	}

	private bool Truncate(StringBuilder sb, int maxLength, int amount)
	{
		if (sb.Length > maxLength)
		{
			sb.Remove(0, amount);
			return true;
		}
		return false;
	}
}
