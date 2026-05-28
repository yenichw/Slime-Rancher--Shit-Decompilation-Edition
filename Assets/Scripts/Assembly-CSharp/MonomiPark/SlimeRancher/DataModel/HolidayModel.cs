using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonomiPark.SlimeRancher.Persist;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class HolidayModel
	{
		public interface Participant
		{
			void InitModel(HolidayModel model);

			void SetModel(HolidayModel model);
		}

		public interface ISchedule
		{
			bool IsLiveAsOf(DateTime date);

			string GetScheduleId();
		}

		public class DateRangeSchedule : ISchedule
		{
			public readonly DateTime startDate;

			public readonly DateTime finalDate;

			public DateRangeSchedule(DateTime startDate, DateTime finalDate)
			{
				this.startDate = startDate;
				this.finalDate = finalDate;
			}

			public bool IsLiveAsOf(DateTime date)
			{
				if (startDate <= date)
				{
					return date <= finalDate;
				}
				return false;
			}

			public string GetScheduleId()
			{
				return string.Format("{0}:{1}", startDate.ToString("yyyyMMdd"), finalDate.ToString("yyyyMMdd"));
			}
		}

		public class DateSchedule : ISchedule
		{
			public readonly DateTime scheduledDate;

			public DateSchedule(DateTime scheduledDate)
			{
				this.scheduledDate = scheduledDate;
			}

			public bool IsLiveAsOf(DateTime date)
			{
				return scheduledDate == date;
			}

			public string GetScheduleId()
			{
				return string.Format("{0}:{0}", scheduledDate.ToString("yyyyMMdd"));
			}
		}

		public class AnnualDateSchedule : ISchedule
		{
			public readonly int day;

			public readonly int month;

			public AnnualDateSchedule(int day, int month)
			{
				if (month < 1 || month > 12)
				{
					throw new ArgumentOutOfRangeException($"month is invalid. Received a value of {month}");
				}
				if (day < 1 || day > 31)
				{
					throw new ArgumentOutOfRangeException($"day is invalid. Received a value of {month}");
				}
				this.day = day;
				this.month = month;
			}

			public bool IsLiveAsOf(DateTime date)
			{
				if (month == date.Month)
				{
					return day == date.Day;
				}
				return false;
			}

			public string GetScheduleId()
			{
				return string.Format("XXXX{0}{1}:XXXX{0}{1}", day.ToString("D2"), month.ToString("D2"));
			}
		}

		public class WeeklySchedule : ISchedule
		{
			public readonly int weekNumber;

			public readonly int dayStart;

			public readonly int dayFinal;

			public WeeklySchedule(int weekNumber, int dayStart, int dayFinal)
			{
				if (weekNumber < 1 || weekNumber > 53)
				{
					throw new ArgumentOutOfRangeException($"Invalid weekNumber. Received {weekNumber}. Must be between 1 and 53 inclusive.");
				}
				if (dayStart < 1 || dayStart > 7)
				{
					throw new ArgumentOutOfRangeException($"Invalid dayStart. Received {dayStart}. Must be between 1 and 7 inclusive.");
				}
				if (dayFinal < 1 || dayFinal > 7)
				{
					throw new ArgumentOutOfRangeException($"Invalid dayFinal. Received {dayFinal}. Must be between 1 and 7 inclusive.");
				}
				if (dayStart > dayFinal)
				{
					throw new ArgumentException("dayStart must be less than or equal to dayFinal.");
				}
				this.weekNumber = weekNumber;
				this.dayStart = dayStart;
				this.dayFinal = dayFinal;
			}

			public bool IsLiveAsOf(DateTime date)
			{
				int weekOfYear = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
				int num = ToDayNumberOfWeek(CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date));
				if (weekOfYear == weekNumber)
				{
					if (dayStart <= num)
					{
						return num <= dayFinal;
					}
					return false;
				}
				return false;
			}

			public string GetScheduleId()
			{
				return string.Format("WEEK{0}{1}:WEEK{0}{2}", weekNumber.ToString("D2"), dayStart.ToString("D2"), dayFinal.ToString("D2"));
			}

			private int ToDayNumberOfWeek(DayOfWeek dayOfWeek)
			{
				switch (dayOfWeek)
				{
				case DayOfWeek.Monday:
					return 1;
				case DayOfWeek.Tuesday:
					return 2;
				case DayOfWeek.Wednesday:
					return 3;
				case DayOfWeek.Thursday:
					return 4;
				case DayOfWeek.Friday:
					return 5;
				case DayOfWeek.Saturday:
					return 6;
				case DayOfWeek.Sunday:
					return 7;
				default:
					throw new ArgumentException("Invalid DayOfWeek value provided.");
				}
			}
		}

		public abstract class ScheduledEvent
		{
			public readonly string objectId;

			public readonly ISchedule schedule;

			public string id => $"{objectId}:{schedule.GetScheduleId()}";

			private ScheduledEvent(string objectId)
			{
				this.objectId = objectId;
			}

			protected ScheduledEvent(string objectId, DateTime start, DateTime end)
				: this(objectId)
			{
				schedule = new DateRangeSchedule(start, end);
			}

			protected ScheduledEvent(string objectId, int weekNumber, int startDay, int finalDay)
				: this(objectId)
			{
				schedule = new WeeklySchedule(weekNumber, startDay, finalDay);
			}

			protected ScheduledEvent(string objectId, ISchedule schedule)
				: this(objectId)
			{
				this.schedule = schedule;
			}

			public bool IsLiveAsOf(DateTime date)
			{
				return schedule.IsLiveAsOf(date);
			}

			public override string ToString()
			{
				return $"{GetType().Name} [id={id}]";
			}

			public override bool Equals(object other_raw)
			{
				if (other_raw is ScheduledEvent scheduledEvent)
				{
					return id == scheduledEvent.id;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return id.GetHashCode();
			}
		}

		public abstract class EventGordo : ScheduledEvent
		{
			public class Default : EventGordo
			{
				public readonly Identifiable.Id[] commons;

				public Default(string gordo, DateTime start, DateTime end, Identifiable.Id[] commons)
					: base(gordo, start, end)
				{
					this.commons = commons;
				}

				public Default(string gordo, int weekNumber, int startDay, int finalDay, Identifiable.Id[] commons)
					: base(gordo, weekNumber, startDay, finalDay)
				{
					this.commons = commons;
				}
			}

			public class Fixed : EventGordo
			{
				public readonly Identifiable.Id ornament;

				public Fixed(string gordo, DateTime start, DateTime end, Identifiable.Id ornament)
					: base(gordo, start, end)
				{
					this.ornament = ornament;
				}
			}

			public static readonly Identifiable.Id CRATE = Identifiable.Id.CRATE_PARTY_01;

			public static readonly float CRATE_CHANCE = 0.5f;

			public static readonly float RARE_ORNAMENT_CHANCE = 0.05f;

			public static readonly Identifiable.Id[] RARE_ORNAMENTS = new Identifiable.Id[5]
			{
				Identifiable.Id.GOLD_ORNAMENT,
				Identifiable.Id.LUCKY_ORNAMENT,
				Identifiable.Id.PINK_PARTY_ORNAMENT,
				Identifiable.Id.RAINBOW_ORNAMENT,
				Identifiable.Id.TARR_ORNAMENT
			};

			public static readonly EventGordo[] INSTANCES = new EventGordo[53]
			{
				new Default("gordoPinkParty19", 1, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.GOLD_ORNAMENT,
					Identifiable.Id.LUCKY_ORNAMENT
				}),
				new Default("gordoPinkParty24", 2, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.SABER_ORNAMENT,
					Identifiable.Id.STRIPES_PURPLE_ORNAMENT
				}),
				new Default("gordoPinkParty16", 3, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.BOOM_ORNAMENT,
					Identifiable.Id.TABBY_ORNAMENT
				}),
				new Default("gordoPinkParty11", 4, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_TEAL_ORNAMENT,
					Identifiable.Id.HUNTER_ORNAMENT
				}),
				new Default("gordoPinkParty20", 5, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.TANGLE_ORNAMENT,
					Identifiable.Id.PAINTED_HEN_ORNAMENT
				}),
				new Default("gordoPinkParty27", 6, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.GLITCH_ORNAMENT,
					Identifiable.Id.PHOSPHOR_ORNAMENT
				}),
				new Default("gordoPinkParty6", 7, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.HEART_ORNAMENT,
					Identifiable.Id.CLOUD_ORNAMENT
				}),
				new Default("gordoPinkParty28", 8, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.IMPOSTER_ORNAMENT,
					Identifiable.Id.PINK_ORNAMENT
				}),
				new Default("gordoPinkParty2", 9, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STAR_ORNAMENT,
					Identifiable.Id.BOOM_ORNAMENT
				}),
				new Default("gordoPinkParty18", 10, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.DRONE_ORNAMENT,
					Identifiable.Id.ELDER_HEN_ORNAMENT
				}),
				new Default("gordoPinkParty12", 11, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.CLOVER_ORNAMENT,
					Identifiable.Id.STRIPES_GREEN_ORNAMENT
				}),
				new Default("gordoPinkParty8", 12, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.WILDFLOWER_ORNAMENT,
					Identifiable.Id.DERVISH_ORNAMENT
				}),
				new Default("gordoPinkParty9", 13, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.RAD_ORNAMENT,
					Identifiable.Id.QUANTUM_ORNAMENT
				}),
				new Default("gordoPinkParty17", 14, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STEGO_ORNAMENT,
					Identifiable.Id.HENHEN_ORNAMENT
				}),
				new Default("gordoPinkParty22", 15, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.CRYSTAL_ORNAMENT,
					Identifiable.Id.MOSAIC_ORNAMENT
				}),
				new Default("gordoPinkParty1", 16, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_PINK_ORNAMENT,
					Identifiable.Id.PUDDLE_ORNAMENT
				}),
				new Default("gordoPinkParty10", 17, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.SEVENZ_ORNAMENT,
					Identifiable.Id.CHEEVO_ORNAMENT
				}),
				new Default("gordoPinkParty30", 18, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.DUCKY_ORNAMENT,
					Identifiable.Id.PUDDLE_ORNAMENT
				}),
				new Default("gordoPinkParty2", 19, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STACHE_ORNAMENT,
					Identifiable.Id.NEWBUCK_ORNAMENT
				}),
				new Default("gordoPinkParty26", 20, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_RED_ORNAMENT,
					Identifiable.Id.STAR_ORNAMENT
				}),
				new Default("gordoPinkParty4", 21, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.CRYSTAL_ORNAMENT,
					Identifiable.Id.TANGLE_ORNAMENT
				}),
				new Default("gordoPinkParty7", 22, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.DRONE_SLEEPY_ORNAMENT,
					Identifiable.Id.BRIAR_HEN_ORNAMENT
				}),
				new Default("gordoPinkParty5", 23, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.CLOUD_ORNAMENT,
					Identifiable.Id.ROCK_ORNAMENT
				}),
				new Default("gordoPinkParty19", 24, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.IMPOSTER_TABBY_ORNAMENT,
					Identifiable.Id.STONY_HEN_ORNAMENT
				}),
				new Default("gordoPinkParty13", 25, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.SUNNY_ORNAMENT,
					Identifiable.Id.FIRE_ORNAMENT
				}),
				new Default("gordoPinkParty21", 26, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.FIREFLOWER_ORNAMENT,
					Identifiable.Id.RAD_ORNAMENT
				}),
				new Default("gordoPinkParty15", 27, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.HUNTER_ORNAMENT,
					Identifiable.Id.STRIPES_PURPLE_ORNAMENT
				}),
				new Default("gordoPinkParty25", 28, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.QUICKSILVER_ORNAMENT,
					Identifiable.Id.HEART_ORNAMENT
				}),
				new Default("gordoPinkParty14", 29, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.PINK_ORNAMENT,
					Identifiable.Id.HONEY_ORNAMENT
				}),
				new Default("gordoPinkParty23", 30, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.BUZZY_ORNAMENT,
					Identifiable.Id.HEART_ORNAMENT
				}),
				new Default("gordoPinkParty29", 31, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.ELDER_HEN_ORNAMENT,
					Identifiable.Id.PINK_ORNAMENT
				}),
				new Default("gordoPinkParty7", 32, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_BLUE_ORNAMENT,
					Identifiable.Id.STRIPES_PURPLE_ORNAMENT
				}),
				new Default("gordoPinkParty26", 33, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.QUANTUM_ORNAMENT,
					Identifiable.Id.HENHEN_ORNAMENT
				}),
				new Default("gordoPinkParty18", 34, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STEGO_ORNAMENT,
					Identifiable.Id.BUZZY_ORNAMENT
				}),
				new Default("gordoPinkParty22", 35, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.DRONE_ORNAMENT,
					Identifiable.Id.HEART_ORNAMENT
				}),
				new Default("gordoPinkParty4", 36, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STAR_ORNAMENT,
					Identifiable.Id.CLOUD_ORNAMENT
				}),
				new Default("gordoPinkParty30", 37, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.ROCK_ORNAMENT,
					Identifiable.Id.MOSAIC_ORNAMENT
				}),
				new Default("gordoPinkParty17", 38, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.HENHEN_ORNAMENT,
					Identifiable.Id.DERVISH_ORNAMENT
				}),
				new Default("gordoPinkParty16", 39, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.FIREFLOWER_ORNAMENT,
					Identifiable.Id.SUNNY_ORNAMENT
				}),
				new Default("gordoPinkParty5", 40, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_BLUE_ORNAMENT,
					Identifiable.Id.SABER_ORNAMENT
				}),
				new Default("gordoPinkParty1", 41, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.FIRE_ORNAMENT,
					Identifiable.Id.CRYSTAL_ORNAMENT
				}),
				new Default("gordoPinkParty6", 42, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.IMPOSTER_TABBY_ORNAMENT,
					Identifiable.Id.GLITCH_ORNAMENT
				}),
				new Default("gordoPinkParty21", 43, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.PHOSPHOR_ORNAMENT,
					Identifiable.Id.STRIPES_TEAL_ORNAMENT
				}),
				new Default("gordoPinkParty14", 44, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.TARR_LANTERN_ORNAMENT,
					Identifiable.Id.JACK_ORNAMENT
				}),
				new Default("gordoPinkParty9", 45, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.WILDFLOWER_ORNAMENT,
					Identifiable.Id.HONEY_ORNAMENT
				}),
				new Default("gordoPinkParty10", 46, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_PURPLE_ORNAMENT,
					Identifiable.Id.MOSAIC_ORNAMENT
				}),
				new Default("gordoPinkParty12", 47, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.BRIAR_HEN_ORNAMENT,
					Identifiable.Id.BOOM_ORNAMENT
				}),
				new Default("gordoPinkParty8", 48, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.ELDER_HEN_ORNAMENT,
					Identifiable.Id.HENHEN_ORNAMENT
				}),
				new Default("gordoPinkParty27", 49, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STACHE_ORNAMENT,
					Identifiable.Id.HUNTER_ORNAMENT
				}),
				new Default("gordoPinkParty15", 50, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.STRIPES_RED_ORNAMENT,
					Identifiable.Id.STRIPES_GREEN_ORNAMENT
				}),
				new Default("gordoPinkParty13", 51, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.SNOWFLAKE_ORNAMENT,
					Identifiable.Id.STAR_ORNAMENT
				}),
				new Default("gordoPinkParty29", 52, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.QUICKSILVER_ORNAMENT,
					Identifiable.Id.CLOUD_ORNAMENT
				}),
				new Default("gordoPinkParty2", 53, 5, 7, new Identifiable.Id[2]
				{
					Identifiable.Id.TWINKLE_ORNAMENT,
					Identifiable.Id.TWINKLE_ORNAMENT
				})
			};

			protected EventGordo(string gordo, DateTime start, DateTime end)
				: base(gordo, start, end)
			{
			}

			protected EventGordo(string gordo, int weekNumber, int startDay, int finalDay)
				: base(gordo, weekNumber, startDay, finalDay)
			{
			}
		}

		public class EventEchoNoteGordo : ScheduledEvent
		{
			public static readonly EventEchoNoteGordo[] INSTANCES = new EventEchoNoteGordo[13]
			{
				new EventEchoNoteGordo("gordoEchoNote9", 12, 18),
				new EventEchoNoteGordo("gordoEchoNote1", 12, 19),
				new EventEchoNoteGordo("gordoEchoNote6", 12, 20),
				new EventEchoNoteGordo("gordoEchoNote8", 12, 21),
				new EventEchoNoteGordo("gordoEchoNote10", 12, 22),
				new EventEchoNoteGordo("gordoEchoNote4", 12, 23),
				new EventEchoNoteGordo("gordoEchoNote13", 12, 24),
				new EventEchoNoteGordo("gordoEchoNote11", 12, 25),
				new EventEchoNoteGordo("gordoEchoNote7", 12, 26),
				new EventEchoNoteGordo("gordoEchoNote2", 12, 27),
				new EventEchoNoteGordo("gordoEchoNote12", 12, 28),
				new EventEchoNoteGordo("gordoEchoNote5", 12, 29),
				new EventEchoNoteGordo("gordoEchoNote3", 12, 30)
			};

			public EventEchoNoteGordo(string objectId, int month, int day)
				: base(objectId, new AnnualDateSchedule(day, month))
			{
			}
		}

		public HashSet<EventGordo> eventGordos = new HashSet<EventGordo>();

		public HashSet<EventEchoNoteGordo> eventEchoNoteGordos = new HashSet<EventEchoNoteGordo>();

		private Participant participant;

		public void SetParticipant(Participant participant)
		{
			this.participant = participant;
		}

		public void Init()
		{
			if (participant != null)
			{
				participant.InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			if (participant != null)
			{
				participant.SetModel(this);
			}
		}

		public void Push(HolidayDirectorV02 persistence)
		{
			eventGordos = new HashSet<EventGordo>(from id in persistence.eventGordos
				select EventGordo.INSTANCES.FirstOrDefault((EventGordo e) => e.id == id) into e
				where e != null
				select e);
			eventEchoNoteGordos = new HashSet<EventEchoNoteGordo>(from id in persistence.eventEchoNoteGordos
				select EventEchoNoteGordo.INSTANCES.FirstOrDefault((EventEchoNoteGordo e) => e.id == id) into e
				where e != null
				select e);
		}

		public HolidayDirectorV02 Pull()
		{
			return new HolidayDirectorV02
			{
				eventGordos = eventGordos.Select((EventGordo e) => e.id).ToList(),
				eventEchoNoteGordos = eventEchoNoteGordos.Select((EventEchoNoteGordo e) => e.id).ToList()
			};
		}
	}
}
