using DG.Tweening;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GlitchTarrNode : IdHandler<GlitchTarrNodeModel>
{
	public enum State
	{
		INACTIVE = 0,
		ACTIVATING = 1,
		ACTIVE = 2
	}

	public enum Group
	{
		A = 0,
		B = 1
	}

	[Tooltip("Tarr node activation group minor index.")]
	[Range(0f, 10f)]
	public int activationIndex;

	private TimeDirector timeDirector;

	private GlitchCellDirector cellDirector;

	private GlitchTarrNodeModel model;

	private GlitchMetadata metadata;

	private Tween tween;

	public Group activationGroup => cellDirector.tarrActivationGroup;

	public Vector3 scale { get; private set; }

	public override void Awake()
	{
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		cellDirector = GetRequiredComponentInParent<GlitchCellDirector>();
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		scale = base.transform.localScale;
		base.Awake();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (timeDirector != null)
		{
			timeDirector.RemovePassedTimeDelegate(OnActivationStateChanged);
			timeDirector = null;
		}
	}

	protected override string IdPrefix()
	{
		return "glitchTN";
	}

	protected override GameModel.Unregistrant Register(GameModel game)
	{
		return game.Glitch.nodes.Register(this);
	}

	protected override void InitModel(GlitchTarrNodeModel model)
	{
		model.activationTime = 0.0;
	}

	protected override void SetModel(GlitchTarrNodeModel model)
	{
		this.model = model;
		ResetNode(this.model.activationTime);
	}

	public void ResetNode(double activationTime)
	{
		model.activationTime = activationTime;
		OnActivationStateChanged();
		if (!timeDirector.HasReached(activationTime))
		{
			timeDirector.RemovePassedTimeDelegate(OnActivationStateChanged);
			timeDirector.AddPassedTimeDelegate(activationTime, OnActivationStateChanged);
		}
	}

	public State GetState()
	{
		if (timeDirector.HasReached(model.activationTime))
		{
			if (tween != null && tween.IsActive() && tween.IsPlaying())
			{
				return State.ACTIVATING;
			}
			return State.ACTIVE;
		}
		return State.INACTIVE;
	}

	private void OnActivationStateChanged()
	{
		tween?.Kill();
		tween = null;
		bool flag = timeDirector.HasReached(model.activationTime);
		base.gameObject.SetActive(flag);
		if (flag)
		{
			tween = base.transform.DOScale(scale, metadata.tarrNodeScaleInSpeed).From(scale * 0.2f).SetEase(Ease.Linear)
				.SetSpeedBased(isSpeedBased: true);
		}
	}
}
