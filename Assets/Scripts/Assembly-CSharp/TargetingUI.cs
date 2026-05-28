using System.Linq;
using TMPro;
using UnityEngine;

public class TargetingUI : SRSingleton<TargetingUI>
{
	public TMP_Text nameText;

	public TMP_Text infoText;

	private PediaDirector pediaDir;

	private MessageBundle uiBundle;

	private MessageBundle pediaBundle;

	private PlayerState player;

	private float holdInfoUntil;

	private const float HOLD_DURATION = 1f;

	private GameObject currentTarget;

	public void Start()
	{
		SRSingleton<GameContext>.Instance.MessageDirector.RegisterBundlesListener(OnBundlesAvailable);
		player = SRSingleton<SceneContext>.Instance.PlayerState;
		pediaDir = SRSingleton<SceneContext>.Instance.PediaDirector;
	}

	public void OnBundlesAvailable(MessageDirector msgDir)
	{
		uiBundle = msgDir.GetBundle("ui");
		pediaBundle = msgDir.GetBundle("pedia");
		currentTarget = null;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SRSingleton<GameContext>.Instance != null)
		{
			SRSingleton<GameContext>.Instance.MessageDirector.UnregisterBundlesListener(OnBundlesAvailable);
		}
	}

	public void Update()
	{
		GameObject targeting = player.Targeting;
		if (targeting != null && currentTarget == targeting)
		{
			holdInfoUntil = Time.time + 1f;
			return;
		}
		currentTarget = null;
		if (targeting != null && (GetIdentifiableTarget(targeting) || GetGordoIdentifiableTarget(targeting) || GetDroneTarget(targeting)))
		{
			holdInfoUntil = Time.time + 1f;
			currentTarget = targeting;
		}
		bool flag = Time.time <= holdInfoUntil;
		nameText.enabled = flag;
		infoText.enabled = flag;
	}

	private bool GetIdentifiableTarget(GameObject gameObject)
	{
		Identifiable.Id id = Identifiable.GetId(gameObject);
		if (id != 0)
		{
			if (Identifiable.IsPlort(id))
			{
				nameText.text = Identifiable.GetName(id);
				infoText.text = uiBundle.Get("m.hudinfo_plort");
				return true;
			}
			if (Identifiable.IsEcho(id))
			{
				nameText.text = Identifiable.GetName(id);
				infoText.text = uiBundle.Get("m.hudinfo_echo");
				return true;
			}
			if (Identifiable.IsEchoNote(id))
			{
				nameText.text = Identifiable.GetName(id);
				infoText.text = uiBundle.Get("m.hudinfo_echo_note");
				return true;
			}
			if (Identifiable.IsOrnament(id))
			{
				nameText.text = Identifiable.GetName(id);
				infoText.text = uiBundle.Get("m.hudinfo_ornament");
				return true;
			}
			if (Identifiable.IsToy(id))
			{
				nameText.text = Identifiable.GetName(id);
				if (id == Identifiable.Id.KOOKADOBA_BALL)
				{
					infoText.text = uiBundle.Get("m.hudinfo_fruitball");
				}
				else
				{
					infoText.text = uiBundle.Get("m.hudinfo_toy");
				}
				return true;
			}
			if (pediaDir.GetPediaId(id).HasValue)
			{
				nameText.text = Identifiable.GetName(id);
				infoText.text = GetIdentifiableInfoText(id);
				return true;
			}
			if (Identifiable.IsTarr(id))
			{
				nameText.text = Identifiable.GetName(Identifiable.Id.TARR_SLIME);
				infoText.text = GetIdentifiableInfoText(Identifiable.Id.TARR_SLIME);
				return true;
			}
			if (Identifiable.IsSlime(id))
			{
				nameText.text = Identifiable.GetName(id);
				infoText.text = GetIdentifiableInfoText(id);
				return true;
			}
		}
		return false;
	}

	private string GetIdentifiableInfoText(Identifiable.Id identId)
	{
		SlimeEat component = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(identId).GetComponent<SlimeEat>();
		if (Identifiable.IsTarr(identId))
		{
			return uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_diet", "m.foodgroup.tarr"));
		}
		switch (identId)
		{
		case Identifiable.Id.PUDDLE_SLIME:
			return uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_diet", "m.foodgroup.water"));
		case Identifiable.Id.FIRE_SLIME:
			return uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_diet", "m.foodgroup.ash"));
		default:
			if (component != null)
			{
				return uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_diet", component.slimeDefinition.Diet.GetModulesFoodGroupsMsg()));
			}
			return uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_type", SlimeDiet.GetFoodCategoryMsg(identId)));
		}
	}

	private bool GetGordoIdentifiableTarget(GameObject gameObject)
	{
		GordoIdentifiable component = gameObject.GetComponent<GordoIdentifiable>();
		GordoEat component2 = gameObject.GetComponent<GordoEat>();
		if (component != null && component2 != null && Identifiable.IsGordo(component.id))
		{
			nameText.text = Identifiable.GetName(component.id);
			infoText.text = uiBundle.Xlate(MessageUtil.Compose("m.hudinfo_diet", component2.GetDirectFoodGroupsMsg()));
			return true;
		}
		return false;
	}

	private bool GetDroneTarget(GameObject gameObject)
	{
		Drone component = gameObject.GetComponent<Drone>();
		if (component != null)
		{
			nameText.text = pediaBundle.Get("m.gadget.name.drone");
			infoText.text = string.Join(", ", (from p in component.gadget.programs
				where p.IsComplete()
				select p.target.GetName()).ToArray());
			return true;
		}
		return false;
	}
}
