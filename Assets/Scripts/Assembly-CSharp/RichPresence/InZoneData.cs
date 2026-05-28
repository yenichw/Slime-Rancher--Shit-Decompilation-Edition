namespace RichPresence
{
	public struct InZoneData
	{
		public ZoneDirector.Zone zone;

		public InZoneData(ZoneDirector.Zone zone)
		{
			this.zone = zone;
		}

		public override string ToString()
		{
			return $"{GetType().Name} [zone={zone}]";
		}
	}
}
