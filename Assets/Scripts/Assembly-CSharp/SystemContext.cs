using System.Globalization;
using MonomiPark.Controller;
using UnityEngine;

public class SystemContext : SRSingleton<SystemContext>
{
	public static bool IsModded;

	public IDateProvider DateProvider = new StandardDateProvider();

	public GameCoreXboxContext GameCoreXboxContext { get; private set; }

	public UWPContext UWPContext { get; private set; }

	public PS4Context PS4Context { get; private set; }

	public RumbleHandler RumbleHandler { get; private set; }

	public override void Awake()
	{
		if (SRSingleton<SystemContext>.Instance == null)
		{
			base.Awake();
			if (Application.unityVersion == "_never_POSSIBLE_")
			{
				new ChineseLunisolarCalendar();
				new GregorianCalendar();
				new HebrewCalendar();
				new HijriCalendar();
				new JapaneseCalendar();
				new JapaneseLunisolarCalendar();
				new JulianCalendar();
				new KoreanCalendar();
				new KoreanLunisolarCalendar();
				new PersianCalendar();
				new TaiwanCalendar();
				new TaiwanLunisolarCalendar();
				new ThaiBuddhistCalendar();
				new UmAlQuraCalendar();
			}
			GameCoreXboxContext = GetComponent<GameCoreXboxContext>();
			UWPContext = GetComponent<UWPContext>();
			PS4Context = GetComponent<PS4Context>();
			Object.DontDestroyOnLoad(base.gameObject);
			RumbleHandler = new EmptyRumbleHandler();
		}
		else if (SRSingleton<SystemContext>.Instance != this)
		{
			Destroyer.Destroy(base.gameObject, "SystemContext.Awake");
		}
	}
}
