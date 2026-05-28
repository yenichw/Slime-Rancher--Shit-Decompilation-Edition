using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Sentry
{
	[Serializable]
	public class SentryEvent
	{
		public string event_id;

		public string message;

		public string timestamp;

		public string logger;

		public string level;

		public string platform = "csharp";

		public string release;

		public Context contexts;

		public SdkVersion sdk = new SdkVersion();

		public List<Breadcrumb> breadcrumbs;

		public User user = new User();

		public Tags tags;

		public SentryEvent(string message, List<Breadcrumb> breadcrumbs = null)
		{
			event_id = Guid.NewGuid().ToString("N");
			this.message = GetDescription(message);
			timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
			level = "error";
			this.breadcrumbs = breadcrumbs;
			contexts = new Context();
			release = GetVersion();
			tags = new Tags();
			tags.cultureName = CultureInfo.CurrentCulture.Name;
			if (SRSingleton<GameContext>.Instance != null && SRSingleton<GameContext>.Instance.MessageDirector != null)
			{
				tags.gameLanguage = SRSingleton<GameContext>.Instance.MessageDirector.GetCurrentLanguageCode();
			}
			tags.isModded = SystemContext.IsModded;
		}

		private static string GetDescription(string description)
		{
			try
			{
				string text = "NONE";
				if (SRSingleton<SceneContext>.Instance != null && SRSingleton<SceneContext>.Instance.Player != null)
				{
					Transform transform = SRSingleton<SceneContext>.Instance.Player.transform;
					text = string.Concat(transform.position, " Facing: ", transform.eulerAngles);
				}
				if (SRSingleton<GameContext>.Instance != null)
				{
					return $"{description}\n\nVersion: {GetVersion()}\nPosition: {text}\n\nLog:\n{SRSingleton<GameContext>.Instance.LogText}";
				}
				return string.Format("{0}\n\nVersion: {1}\nPosition: {2}\n\nLog:\n{3}", description, GetVersion(), text, "Log text not available.");
			}
			catch (Exception ex)
			{
				return $"Caught exception while getting description for Sentry: {ex.Message}";
			}
		}

		private static string GetVersion()
		{
			if (SRSingleton<GameContext>.Instance != null)
			{
				return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("build").Xlate("m.version");
			}
			return Application.version;
		}
	}
}
