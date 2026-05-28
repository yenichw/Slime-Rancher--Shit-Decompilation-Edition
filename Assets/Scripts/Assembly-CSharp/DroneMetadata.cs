using System;
using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

public class DroneMetadata : ScriptableObject
{
	public class Program
	{
		public abstract class BaseComponent
		{
			public string id;

			public Sprite image;

			public virtual string GetName()
			{
				return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Get(id);
			}

			public virtual Sprite GetImage()
			{
				return image;
			}
		}

		public class Target : BaseComponent
		{
			public class Basic : Target
			{
				public Basic(Identifiable.Id ident)
				{
					id = $"m.drone.target.identifiable.{Enum.GetName(typeof(Identifiable.Id), ident).ToLowerInvariant()}";
					base.ident = ident;
					predicate = (Identifiable.Id rhs) => rhs == ident;
				}

				public override string GetName()
				{
					return Identifiable.GetName(ident);
				}

				public override Sprite GetImage()
				{
					return SRSingleton<GameContext>.Instance.LookupDirector.GetIcon(ident);
				}
			}

			public class Collection : Target
			{
				public HashSet<Identifiable.Id> collection;

				public Collection(string id, HashSet<Identifiable.Id> collection, Sprite image)
				{
					base.id = $"m.drone.target.name.category_{id}";
					ident = collection.First();
					predicate = (Identifiable.Id rhs) => collection.Contains(rhs);
					this.collection = collection;
					base.image = image;
				}
			}

			public Identifiable.Id ident;

			public Predicate<Identifiable.Id> predicate;
		}

		public class Behaviour : BaseComponent
		{
			public Type[] types;

			public Predicate<Program> isCompatible = (Program p) => true;
		}

		public class GadgetBehaviour : Behaviour
		{
			public Func<bool> gadgetPredicate;

			public Gadget.Id gadget;

			public override string GetName()
			{
				if (gadgetPredicate())
				{
					return SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("pedia").Get($"m.gadget.name.{gadget.ToString().ToLowerInvariant()}");
				}
				return base.GetName();
			}

			public override Sprite GetImage()
			{
				if (gadgetPredicate())
				{
					return SRSingleton<GameContext>.Instance.LookupDirector.GetGadgetDefinition(gadget).icon;
				}
				return base.GetImage();
			}
		}

		public Target target;

		public Behaviour source;

		public Behaviour destination;

		public Program()
		{
		}

		public Program(Target target, Behaviour source, Behaviour destination)
		{
			this.target = target;
			this.source = source;
			this.destination = destination;
		}

		public Program Clone()
		{
			return new Program(target, source, destination);
		}

		public bool IsComplete()
		{
			if (target.id != "drone.target.none" && source.id != "drone.behaviour.none")
			{
				return destination.id != "drone.behaviour.none";
			}
			return false;
		}

		public bool IsReset()
		{
			if (target.id == "drone.target.none" && source.id == "drone.behaviour.none")
			{
				return destination.id == "drone.behaviour.none";
			}
			return false;
		}
	}

	[Header("GameObject Prefabs")]
	public DroneUI droneUI;

	public DroneUIProgram droneUIProgram;

	public DroneUIProgramPicker droneUIProgramPicker;

	public DroneUIProgramButton droneUIProgramButton;

	[Header("Program Component Images")]
	public Sprite imageTargetCollectionPlorts;

	public Sprite imageTargetCollectionFruits;

	public Sprite imageTargetCollectionVeggies;

	public Sprite imageTargetCollectionMeats;

	public Sprite imageTargetCollectionElders;

	public Sprite imageSourceCorral;

	public Sprite imageSourcePond;

	public Sprite imageSourceIncinerator;

	public Sprite imageSourceGarden;

	public Sprite imageSourceCoop;

	public Sprite imageSourceSilo;

	public Sprite imageSourcePlortCollector;

	public Sprite imageSourceOutsidePlots;

	public Sprite imageSourceFreeRange;

	public Sprite imageDestinationCorral;

	public Sprite imageDestinationSilo;

	public Sprite imageDestinationFeeder;

	public Sprite imageDestinationIncinerator;

	public Sprite imageDestinationPlortMarket;

	public Sprite imageDestinationRefinery;

	public Sprite imageNone;

	public Sprite pickTargetIcon;

	public Sprite pickSourceIcon;

	public Sprite pickDestinationIcon;

	[Header("SFX")]
	public SECTR_AudioCue onActiveCue;

	public SECTR_AudioCue onGatherBeginCue;

	public SECTR_AudioCue onGatherLoopCue;

	public SECTR_AudioCue onGatherEndCue;

	public SECTR_AudioCue onDepositBeginCue;

	public SECTR_AudioCue onDepositLoopCue;

	public SECTR_AudioCue onDepositEndCue;

	public SECTR_AudioCue onRestBeginCue;

	public SECTR_AudioCue onRestLoopCue;

	public SECTR_AudioCue onRestEndCue;

	public SECTR_AudioCue onHappyCue;

	public SECTR_AudioCue onGrumpyCue;

	public SECTR_AudioCue onBoppedCue;

	public SECTR_AudioCue onBatteryFilledCue;

	public SECTR_AudioCue onGuiEnableCue;

	public SECTR_AudioCue onGuiDisableCue;

	public SECTR_AudioCue onGuiButtonTargetCue;

	public SECTR_AudioCue onGuiButtonSourceCue;

	public SECTR_AudioCue onGuiButtonDestinationCue;

	public SECTR_AudioCue onGuiButtonActivateCue;

	public SECTR_AudioCue onGuiButtonResetCue;

	[Header("FX")]
	public GameObject onBatteryFilledFX;

	public GameObject onTeleportFX;

	[Header("Coins Override")]
	public Sprite coinsIcon;

	public Color coinsColor;

	public SECTR_AudioCue coinsCue;

	public const string TARGET_NONE_ID = "drone.target.none";

	public const string BEHAVIOUR_NONE_ID = "drone.behaviour.none";

	public Program.Target[] targets { get; private set; }

	public Program.Behaviour[] sources { get; private set; }

	public Program.Behaviour[] destinations { get; private set; }

	public void OnEnable()
	{
		targets = new Program.Target[39]
		{
			new Program.Target.Collection("plorts", Identifiable.PLORT_CLASS, imageTargetCollectionPlorts),
			new Program.Target.Collection("veggies", Identifiable.VEGGIE_CLASS, imageTargetCollectionVeggies),
			new Program.Target.Collection("fruits", Identifiable.FRUIT_CLASS, imageTargetCollectionFruits),
			new Program.Target.Collection("meats", Identifiable.MEAT_CLASS, imageTargetCollectionMeats),
			new Program.Target.Collection("elders", Identifiable.ELDER_CLASS, imageTargetCollectionElders),
			new Program.Target.Basic(Identifiable.Id.PINK_PLORT),
			new Program.Target.Basic(Identifiable.Id.ROCK_PLORT),
			new Program.Target.Basic(Identifiable.Id.TABBY_PLORT),
			new Program.Target.Basic(Identifiable.Id.PHOSPHOR_PLORT),
			new Program.Target.Basic(Identifiable.Id.RAD_PLORT),
			new Program.Target.Basic(Identifiable.Id.BOOM_PLORT),
			new Program.Target.Basic(Identifiable.Id.HONEY_PLORT),
			new Program.Target.Basic(Identifiable.Id.PUDDLE_PLORT),
			new Program.Target.Basic(Identifiable.Id.CRYSTAL_PLORT),
			new Program.Target.Basic(Identifiable.Id.HUNTER_PLORT),
			new Program.Target.Basic(Identifiable.Id.QUANTUM_PLORT),
			new Program.Target.Basic(Identifiable.Id.MOSAIC_PLORT),
			new Program.Target.Basic(Identifiable.Id.DERVISH_PLORT),
			new Program.Target.Basic(Identifiable.Id.TANGLE_PLORT),
			new Program.Target.Basic(Identifiable.Id.FIRE_PLORT),
			new Program.Target.Basic(Identifiable.Id.SABER_PLORT),
			new Program.Target.Basic(Identifiable.Id.CARROT_VEGGIE),
			new Program.Target.Basic(Identifiable.Id.OCAOCA_VEGGIE),
			new Program.Target.Basic(Identifiable.Id.BEET_VEGGIE),
			new Program.Target.Basic(Identifiable.Id.PARSNIP_VEGGIE),
			new Program.Target.Basic(Identifiable.Id.ONION_VEGGIE),
			new Program.Target.Basic(Identifiable.Id.POGO_FRUIT),
			new Program.Target.Basic(Identifiable.Id.MANGO_FRUIT),
			new Program.Target.Basic(Identifiable.Id.CUBERRY_FRUIT),
			new Program.Target.Basic(Identifiable.Id.LEMON_FRUIT),
			new Program.Target.Basic(Identifiable.Id.PEAR_FRUIT),
			new Program.Target.Basic(Identifiable.Id.HEN),
			new Program.Target.Basic(Identifiable.Id.ROOSTER),
			new Program.Target.Basic(Identifiable.Id.STONY_HEN),
			new Program.Target.Basic(Identifiable.Id.BRIAR_HEN),
			new Program.Target.Basic(Identifiable.Id.PAINTED_HEN),
			new Program.Target.Basic(Identifiable.Id.ELDER_HEN),
			new Program.Target.Basic(Identifiable.Id.ELDER_ROOSTER),
			new Program.Target.Basic(Identifiable.Id.SPICY_TOFU)
		};
		sources = new Program.Behaviour[9]
		{
			new Program.Behaviour
			{
				id = "m.drone.source.name.corral",
				image = imageSourceCorral,
				types = new Type[1] { typeof(DroneProgramSourceCorral) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.pond",
				image = imageSourcePond,
				types = new Type[1] { typeof(DroneProgramSourcePond) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.incinerator",
				image = imageSourceIncinerator,
				types = new Type[1] { typeof(DroneProgramSourceIncinerator) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.garden",
				image = imageSourceGarden,
				types = new Type[1] { typeof(DroneProgramSourceGarden) },
				isCompatible = (Program p) => Identifiable.IsFruit(p.target.ident) || Identifiable.IsVeggie(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.coop",
				image = imageSourceCoop,
				isCompatible = (Program p) => Identifiable.IsAnimal(p.target.ident),
				types = new Type[2]
				{
					typeof(DroneProgramSourceElderCollector),
					typeof(DroneProgramSourceCoop)
				}
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.silo",
				image = imageSourceSilo,
				types = new Type[1] { typeof(DroneProgramSourceSilo) }
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.plort_collector",
				image = imageSourcePlortCollector,
				types = new Type[1] { typeof(DroneProgramSourcePlortCollector) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.dynamic",
				image = imageSourceOutsidePlots,
				types = new Type[1] { typeof(DroneProgramSourceOutsidePlots) },
				isCompatible = (Program p) => !Identifiable.IsPlort(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.source.name.free_range",
				image = imageSourceFreeRange,
				types = new Type[1] { typeof(DroneProgramSourceFreeRange) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident)
			}
		};
		destinations = new Program.Behaviour[6]
		{
			new Program.Behaviour
			{
				id = "m.drone.destination.name.corral",
				image = imageDestinationCorral,
				types = new Type[1] { typeof(DroneProgramDestinationCorral) },
				isCompatible = (Program p) => Identifiable.IsFood(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.destination.name.silo",
				image = imageDestinationSilo,
				types = new Type[1] { typeof(DroneProgramDestinationSilo) }
			},
			new Program.Behaviour
			{
				id = "m.drone.destination.name.auto_feeder",
				image = imageDestinationFeeder,
				types = new Type[1] { typeof(DroneProgramDestinationFeeder) },
				isCompatible = (Program p) => Identifiable.IsFood(p.target.ident)
			},
			new Program.Behaviour
			{
				id = "m.drone.destination.name.incinerator",
				image = imageDestinationIncinerator,
				types = new Type[1] { typeof(DroneProgramDestinationIncinerator) },
				isCompatible = (Program p) => Identifiable.IsFood(p.target.ident)
			},
			new Program.GadgetBehaviour
			{
				id = "m.drone.destination.name.plort_market",
				image = imageDestinationPlortMarket,
				types = new Type[1] { typeof(DroneProgramDestinationPlortMarket) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident),
				gadget = Gadget.Id.MARKET_LINK,
				gadgetPredicate = () => SRSingleton<SceneContext>.Instance.Player.GetComponent<RegionMember>().regions.All((Region r) => r.gameObject.name != "cellRanch_Home")
			},
			new Program.GadgetBehaviour
			{
				id = "m.drone.destination.name.refinery",
				image = imageDestinationRefinery,
				types = new Type[1] { typeof(DroneProgramDestinationRefinery) },
				isCompatible = (Program p) => Identifiable.IsPlort(p.target.ident),
				gadget = Gadget.Id.REFINERY_LINK,
				gadgetPredicate = () => SRSingleton<SceneContext>.Instance.Player.GetComponent<RegionMember>().regions.All((Region r) => r.gameObject.name != "cellRanch_Lab")
			}
		};
	}

	public Program.Target GetDefaultTarget()
	{
		return new Program.Target
		{
			id = "drone.target.none",
			image = imageNone,
			predicate = (Identifiable.Id id) => false
		};
	}

	public Program.Behaviour GetDefaultBehaviour()
	{
		return new Program.Behaviour
		{
			id = "drone.behaviour.none",
			image = imageNone,
			types = new Type[0]
		};
	}
}
