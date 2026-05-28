using System;
using System.Collections.Generic;
using UnityEngine;

public static class AnalyticsUtil
{
	private interface IListener
	{
		void CustomEvent(string eventName, IDictionary<string, object> eventData);
	}

	public const string EVENT_SESSION_ENDED = "SessionEnded";

	public const string NULL = "null";

	private static List<IListener> Listeners = new List<IListener>();

	public static void CustomEvent(string eventName, IDictionary<string, object> customEventData = null, bool includeDefaultEventData = true)
	{
		Dictionary<string, object> eventData = ((customEventData != null) ? new Dictionary<string, object>(customEventData) : new Dictionary<string, object>());
		if (includeDefaultEventData)
		{
			foreach (KeyValuePair<string, object> defaultEventDatum in GetDefaultEventData())
			{
				if (!eventData.ContainsKey(defaultEventDatum.Key))
				{
					eventData[defaultEventDatum.Key] = defaultEventDatum.Value;
				}
			}
		}
		Listeners.ForEach(delegate(IListener instance)
		{
			instance.CustomEvent(eventName, eventData);
		});
	}

	private static Dictionary<string, object> GetDefaultEventData()
	{
		try
		{
			return new Dictionary<string, object>
			{
				{
					"Game.Id",
					SRSingleton<GameContext>.Instance.AutoSaveDirector.SavedGame.GetName()
				},
				{
					"Game.Mode",
					SRSingleton<SceneContext>.Instance.GameModel.currGameMode
				},
				{
					"Player.Position",
					GetEventData(SRSingleton<SceneContext>.Instance.Player.transform.position)
				},
				{
					"Player.Region",
					SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId()
				},
				{
					"Player.Zone",
					SRSingleton<SceneContext>.Instance.Player.GetComponent<PlayerZoneTracker>().GetCurrentZone()
				},
				{
					"Time.WorldTime",
					GetEventData(SRSingleton<SceneContext>.Instance.TimeDirector.WorldTime(), 0)
				}
			};
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to get default analytics event metadata.", "exception", ex);
		}
		return new Dictionary<string, object>();
	}

	public static string GetEventData(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return "null";
		}
		Identifiable componentInParent = gameObject.GetComponentInParent<Identifiable>();
		if (componentInParent != null)
		{
			return componentInParent.id.ToString();
		}
		return gameObject.name;
	}

	public static string GetEventData(Vector3 vector3)
	{
		return $"{{\"x\":{GetEventData(vector3.x)},\"y\":{GetEventData(vector3.y)},\"z\":{GetEventData(vector3.z)}}}";
	}

	public static string GetEventData(double value, int decimals = 2)
	{
		return value.ToString($"F{decimals}");
	}

	public static string GetEventData(float value, int decimals = 2)
	{
		return value.ToString($"F{decimals}");
	}
}
