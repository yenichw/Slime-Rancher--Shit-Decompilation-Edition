using System;
using RichPresence;

public static class Discord
{
	private class RichPresenceHandlerImpl : Handler
	{
		public static RichPresenceHandlerImpl Instance = new RichPresenceHandlerImpl();

		public void SetRichPresence(MainMenuData data)
		{
			MessageDirector messageDirector = SRSingleton<GameContext>.Instance.MessageDirector;
			DiscordRpc.UpdatePresence(new DiscordRpc.RichPresence
			{
				details = messageDirector.Get("global", "l.presence.in_menu"),
				largeImageKey = "main-menu-large"
			});
		}

		public void SetRichPresence(InZoneData data)
		{
			if (Director.TryGetZoneId(data.zone, out var id))
			{
				MessageDirector messageDirector = SRSingleton<GameContext>.Instance.MessageDirector;
				DiscordRpc.UpdatePresence(new DiscordRpc.RichPresence
				{
					details = messageDirector.Get("global", $"l.presence.{id}"),
					largeImageKey = $"zone-{id}-large",
					state = string.Format("{0}, {1}", messageDirector.Get("ui", $"m.gamemode_{SRSingleton<SceneContext>.Instance.GameModel.currGameMode.ToString().ToLower()}"), SRSingleton<SceneContext>.Instance.TimeDirector.CurrDayString())
				});
			}
		}
	}

	private class UnityEventListener : SRSingleton<UnityEventListener>
	{
		public void OnApplicationQuit()
		{
			DiscordRpc.ClearPresence();
			DiscordRpc.Shutdown();
		}

		public void Update()
		{
			DiscordRpc.RunCallbacks();
		}
	}

	private static readonly DiscordRpc.EventHandlers staticEventHandlers;

	private const string DISCORD_ID = "443564201349218305";

	private const string STEAM_ID = null;

	public static Handler RichPresenceHandler => RichPresenceHandlerImpl.Instance;

	static Discord()
	{
		staticEventHandlers = new DiscordRpc.EventHandlers
		{
			readyCallback = OnReadyCallback,
			disconnectedCallback = OnDisconnectedCallback,
			errorCallback = OnErrorCallback,
			joinCallback = OnJoinCallback,
			spectateCallback = OnSpectateCallback,
			requestCallback = OnRequestCallback
		};
		try
		{
			DiscordRpc.Initialize("443564201349218305", ref staticEventHandlers, autoRegister: true, null);
			SRSingleton<GameContext>.Instance.gameObject.AddComponent<UnityEventListener>();
		}
		catch (Exception ex)
		{
			Log.Error("Failed to initialize Discord.", "exception", ex);
		}
	}

	private static void OnReadyCallback(ref DiscordRpc.DiscordUser user)
	{
	}

	private static void OnDisconnectedCallback(int errorCode, string message)
	{
	}

	private static void OnErrorCallback(int errorCode, string message)
	{
		Log.Error("Discord.errorCallback", "errorCode", errorCode, "message", message);
	}

	private static void OnJoinCallback(string secret)
	{
	}

	private static void OnSpectateCallback(string secret)
	{
	}

	private static void OnRequestCallback(ref DiscordRpc.DiscordUser user)
	{
	}
}
