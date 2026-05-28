using System.Collections;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class GlitchImposto : IdHandler<GlitchImpostoModel>, LiquidConsumer
{
	private enum Visibility
	{
		NONE = 0,
		OUT_OF_RANGE = 1,
		IN_RANGE = 2
	}

	[Tooltip("Custom transform node used as the spawn position of the glitch slimes when exposed. (optional)")]
	public Transform spawnNode;

	[Tooltip("Weight used when picking which imposto to be enabled by the GlitchImpostoDirector.")]
	public float weight = 1f;

	private GlitchImpostoModel model;

	private GlitchMetadata metadata;

	private TimeDirector timeDirector;

	private GameObject player;

	private GlitchImpostoDirector impostoDirector;

	private Renderer[] renderers;

	private Visibility visibility;

	protected override string IdPrefix()
	{
		return "imposto";
	}

	public override void Awake()
	{
		base.Awake();
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		player = SRSingleton<SceneContext>.Instance.Player;
		impostoDirector = GetComponentInParent<GlitchImpostoDirector>();
		impostoDirector.Register(this);
		renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
	}

	protected override GameModel.Unregistrant Register(GameModel game)
	{
		return game.Glitch.impostos.Register(this);
	}

	protected override void InitModel(GlitchImpostoModel model)
	{
		model.deactivateTime = null;
		model.cooldownTime = 0.0;
	}

	protected override void SetModel(GlitchImpostoModel model)
	{
		this.model = model;
		if (this.model.deactivateTime.HasValue && timeDirector.HasReached(this.model.deactivateTime.Value))
		{
			Deactivate();
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (impostoDirector != null)
		{
			impostoDirector.Deregister(this);
			impostoDirector = null;
		}
	}

	public void Update()
	{
		Visibility maxVisibility = GetMaxVisibility();
		if (maxVisibility != visibility)
		{
			OnVisibilityChanged(visibility, maxVisibility);
			visibility = maxVisibility;
		}
	}

	public void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = ((visibility == Visibility.IN_RANGE) ? Color.green : ((visibility == Visibility.OUT_OF_RANGE) ? Color.yellow : Color.red));
			Gizmos.DrawLine(base.transform.position, base.transform.position + Vector3.up * 10f);
		}
		if (GizmosUtil.IsThisOrChildSelected(base.gameObject) && spawnNode != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(spawnNode.position, 0.5f);
		}
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
		model.deactivateTime = 0.0;
	}

	public void Activate()
	{
		visibility = Visibility.NONE;
		base.gameObject.SetActive(value: true);
		model.deactivateTime = null;
	}

	public bool IsReady()
	{
		return timeDirector.HasReached(model.cooldownTime);
	}

	public void AddLiquid(Identifiable.Id id, float units)
	{
		if (id == Identifiable.Id.GLITCH_DEBUG_SPRAY_LIQUID)
		{
			metadata.impostoExposure.OnExposed(base.gameObject, (spawnNode != null) ? spawnNode.position : base.transform.position);
			model.cooldownTime = timeDirector.HoursFromNow(metadata.impostoCooldownTime);
			Deactivate();
		}
	}

	private void OnVisibilityChanged(Visibility previous, Visibility current)
	{
		switch (previous)
		{
		case Visibility.IN_RANGE:
			model.deactivateTime = timeDirector.HoursFromNow(metadata.impostoDeactivateTime * (1f / 60f));
			return;
		case Visibility.NONE:
			if (model.deactivateTime.HasValue && timeDirector.HasReached(model.deactivateTime.Value))
			{
				StartCoroutine(OnFailedExposedCoroutine());
				return;
			}
			break;
		}
		if (current == Visibility.IN_RANGE)
		{
			model.deactivateTime = null;
		}
	}

	private IEnumerator OnFailedExposedCoroutine()
	{
		yield return new WaitForSeconds(metadata.impostoFailedExposedDelayTime);
		metadata.impostoExposure.OnFailedExposed(base.gameObject);
		Deactivate();
	}

	private Visibility GetMaxVisibility()
	{
		Visibility visibility = Visibility.NONE;
		for (int i = 0; i < renderers.Length; i++)
		{
			if (visibility >= Visibility.IN_RANGE)
			{
				break;
			}
			visibility = Max(visibility, renderers[i].isVisible ? ((!((player.transform.position - renderers[i].transform.position).sqrMagnitude <= metadata.impostoDetectionRange)) ? Visibility.OUT_OF_RANGE : Visibility.IN_RANGE) : Visibility.NONE);
		}
		return visibility;
	}

	private static Visibility Max(Visibility a, Visibility b)
	{
		if (a > b)
		{
			return a;
		}
		return b;
	}
}
