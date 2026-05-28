using System.Collections.Generic;

namespace RichPresence
{
	public class Director
	{
		private List<Handler> handlers = new List<Handler>();

		private static Dictionary<ZoneDirector.Zone, string> RICH_PRESENCE_ZONE_LOOKUP = new Dictionary<ZoneDirector.Zone, string>(ZoneDirector.zoneComparer)
		{
			{
				ZoneDirector.Zone.DESERT,
				"desert"
			},
			{
				ZoneDirector.Zone.MOCHI_RANCH,
				"mochis_ranch"
			},
			{
				ZoneDirector.Zone.MOSS,
				"moss_blanket"
			},
			{
				ZoneDirector.Zone.OGDEN_RANCH,
				"ogdens_ranch"
			},
			{
				ZoneDirector.Zone.QUARRY,
				"quarry"
			},
			{
				ZoneDirector.Zone.RANCH,
				"ranch"
			},
			{
				ZoneDirector.Zone.REEF,
				"reef"
			},
			{
				ZoneDirector.Zone.RUINS,
				"ruins"
			},
			{
				ZoneDirector.Zone.SLIMULATIONS,
				"slimulations"
			},
			{
				ZoneDirector.Zone.VALLEY,
				"nimble_valley"
			},
			{
				ZoneDirector.Zone.VIKTOR_LAB,
				"viktors_lab"
			},
			{
				ZoneDirector.Zone.WILDS,
				"wilds"
			}
		};

		public void Register(Handler handler)
		{
			handlers.Add(handler);
		}

		public void Deregister(Handler handler)
		{
			handlers.Remove(handler);
		}

		public void SetRichPresence(MainMenuData data)
		{
			foreach (Handler handler in handlers)
			{
				handler.SetRichPresence(data);
			}
		}

		public void SetRichPresence(InZoneData data)
		{
			foreach (Handler handler in handlers)
			{
				handler.SetRichPresence(data);
			}
		}

		public static bool TryGetZoneId(ZoneDirector.Zone zone, out string id)
		{
			return RICH_PRESENCE_ZONE_LOOKUP.TryGetValue(zone, out id);
		}
	}
}
