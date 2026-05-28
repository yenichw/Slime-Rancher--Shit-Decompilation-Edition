using System;
using System.Text;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	public delegate void DestroyCallback();

	protected static bool s_EverInitialized;

	protected static SteamManager s_instance;

	protected bool m_bInitialized;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public DestroyCallback destroyCallbacks;

	public static Callback<GameOverlayActivated_t> s_GameOverlayActivated;

	public static Callback<GamepadTextInputDismissed_t> s_GamepadTextInputDismissed;

	public static Callback<UserStatsReceived_t> s_UserStatsReceived;

	public static bool s_UserStatsReceived_Initialized;

	protected static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	public static event Action s_UserStatsReceived_OnInitialized;

	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	protected virtual void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
		}
		else
		{
			s_EverInitialized = true;
		}
	}

	protected virtual void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	public static void AddDestroyCallback(DestroyCallback callback)
	{
		SteamManager instance = Instance;
		instance.destroyCallbacks = (DestroyCallback)Delegate.Combine(instance.destroyCallbacks, callback);
	}

	protected virtual void OnDestroy()
	{
		if (s_instance != this)
		{
			return;
		}
		if (destroyCallbacks != null)
		{
			destroyCallbacks();
		}
		s_instance = null;
		if (m_bInitialized)
		{
			SteamAPI.Shutdown();
			if (s_UserStatsReceived != null)
			{
				Debug.Log("Disposing of UserStatsReceived callback");
				s_UserStatsReceived.Dispose();
			}
			if (s_GameOverlayActivated != null)
			{
				Debug.Log("Disposing of GameOverlayActivated callback");
				s_GameOverlayActivated.Dispose();
			}
			if (s_GamepadTextInputDismissed != null)
			{
				Debug.Log("Disposing of GamepadTextInputDismissed callback");
				s_GamepadTextInputDismissed.Dispose();
			}
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public static void AddAchievement(AchievementsDirector.Achievement achievement)
	{
		if (s_UserStatsReceived_Initialized)
		{
			SteamUserStats.SetAchievement(Enum.GetName(typeof(AchievementsDirector.Achievement), achievement));
			SteamUserStats.StoreStats();
		}
		else
		{
			s_UserStatsReceived_OnInitialized += delegate
			{
				AddAchievement(achievement);
			};
		}
	}

	public static void c_UserStatsReceived(UserStatsReceived_t response)
	{
		s_UserStatsReceived_Initialized = true;
		s_UserStatsReceived.Unregister();
		s_UserStatsReceived = null;
		if (SteamManager.s_UserStatsReceived_OnInitialized != null)
		{
			SteamManager.s_UserStatsReceived_OnInitialized();
			SteamManager.s_UserStatsReceived_OnInitialized = null;
		}
	}
}
