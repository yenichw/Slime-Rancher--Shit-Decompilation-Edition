using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayWithToys : FindConsumable
{
	private class ToyDriveCalculator : DriveCalculator
	{
		private Identifiable.Id[] favoriteToys;

		public ToyDriveCalculator(Identifiable.Id[] favoriteToys)
			: base(SlimeEmotions.Emotion.NONE, 0f, 0f)
		{
			this.favoriteToys = favoriteToys;
		}

		public override float Drive(SlimeEmotions emotions, Identifiable.Id id)
		{
			return base.Drive(emotions, id) * (favoriteToys.Contains(id) ? 1f : 0.5f);
		}
	}

	[Tooltip("We use the SlimeDefinition to find the slime's favorite toys.")]
	public SlimeDefinition slimeDefinition;

	[Tooltip("Should we only play with toys that are floating")]
	public bool onlyFloatingToys;

	private GameObject target;

	private float currDrive;

	private float nextPlayTime;

	private Dictionary<Identifiable.Id, DriveCalculator> toysDict;

	private const float POUNCE_DIST = 8f;

	private const float POUNCE_DIST_SQR = 64f;

	private const float PLAY_RESET_TIME = 5f;

	private bool neverPlayWithToys;

	private static List<GameObjectActorModelIdentifiableIndex.Entry> localStaticToyEntries = new List<GameObjectActorModelIdentifiableIndex.Entry>();

	public override void Awake()
	{
		base.Awake();
		toysDict = new Dictionary<Identifiable.Id, DriveCalculator>(Identifiable.idComparer);
		DriveCalculator value = new ToyDriveCalculator(slimeDefinition.FavoriteToys);
		foreach (Identifiable.Id item in Identifiable.TOY_CLASS)
		{
			toysDict[item] = value;
		}
		DestroyOnTouching component = GetComponent<DestroyOnTouching>();
		if (component != null && !component.touchingWaterOkay && !component.touchingAshOkay)
		{
			neverPlayWithToys = true;
		}
	}

	protected override Dictionary<Identifiable.Id, DriveCalculator> GetSearchIds()
	{
		return toysDict;
	}

	public override float Relevancy(bool isGrounded)
	{
		if (Time.time < nextPlayTime)
		{
			return 0f;
		}
		if (neverPlayWithToys)
		{
			return 0f;
		}
		localStaticToyEntries.Clear();
		CellDirector.GetToysNearMember(member, localStaticToyEntries);
		target = FindNearestConsumable(localStaticToyEntries, out currDrive);
		if (target == null)
		{
			return 0f;
		}
		currDrive = Randoms.SHARED.GetFloat(0.8f);
		if (!(target == null) && (!onlyFloatingToys || DragFloatReactor.IsFloating(target)))
		{
			return currDrive * currDrive;
		}
		return 0f;
	}

	public override void Action()
	{
		if (!(target == null) && (!onlyFloatingToys || DragFloatReactor.IsFloating(target)) && IsGrounded())
		{
			Vector3 vector = SlimeSubbehaviour.GetGotoPos(target) - base.transform.position;
			float sqrMagnitude = vector.sqrMagnitude;
			Vector3 normalized = vector.normalized;
			RotateTowards(normalized);
			float num = 1.2f;
			float num2 = Mathf.Sqrt(Mathf.Sqrt(sqrMagnitude) * Physics.gravity.magnitude) * num;
			GetComponent<Rigidbody>().AddForce((normalized + Vector3.up).normalized * num2, ForceMode.VelocityChange);
			slimeAudio.Play(slimeAudio.slimeSounds.jumpCue);
			slimeAudio.Play(slimeAudio.slimeSounds.voiceJumpCue);
			target = null;
			nextPlayTime = Time.fixedTime + 5f;
		}
	}

	public override void Selected()
	{
	}
}
