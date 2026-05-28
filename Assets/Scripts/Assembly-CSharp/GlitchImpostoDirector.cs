using System.Collections.Generic;
using System.Linq;
using Assets.Script.Util.Extensions;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

[RequireComponent(typeof(Region))]
public class GlitchImpostoDirector : SRBehaviour, GlitchImpostoDirectorModel.Participant
{
	[Tooltip("Random range of number of impostos to enable this cell is unhibernated.")]
	public Vector2 availableCount;

	private GlitchImpostoDirectorModel model;

	private GlitchMetadata metadata;

	private TimeDirector timeDirector;

	private Region region;

	private List<GlitchImposto> registered = new List<GlitchImposto>();

	public string id => base.name;

	public void Awake()
	{
		metadata = SRSingleton<SceneContext>.Instance.MetadataDirector.Glitch;
		timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
		SRSingleton<SceneContext>.Instance.GameModel.Glitch.Register(this);
	}

	public void Start()
	{
		region = GetComponent<Region>();
		region.onHibernationStateChanged += OnHibernationStateChanged;
	}

	public void InitModel(GlitchImpostoDirectorModel model)
	{
		model.hibernationTime = null;
	}

	public void SetModel(GlitchImpostoDirectorModel model)
	{
		this.model = model;
	}

	public void OnDestroy()
	{
		if (SRSingleton<SceneContext>.Instance != null)
		{
			SRSingleton<SceneContext>.Instance.GameModel.Glitch.Unregister(this);
		}
		if (region != null)
		{
			region.onHibernationStateChanged -= OnHibernationStateChanged;
			region = null;
		}
	}

	public void Register(GlitchImposto imposto)
	{
		registered.Add(imposto);
	}

	public bool Deregister(GlitchImposto imposto)
	{
		return registered.RemoveAll((GlitchImposto d) => d == imposto) >= 1;
	}

	public void ResetImpostos()
	{
		registered.ForEach(delegate(GlitchImposto imposto)
		{
			imposto.Deactivate();
		});
		foreach (GlitchImposto item in Randoms.SHARED.Pick(registered.Where((GlitchImposto imposto) => imposto.IsReady()).ToList(), Mathf.FloorToInt(availableCount.GetRandom()), (GlitchImposto imposto) => imposto.weight))
		{
			item.Activate();
		}
	}

	private void OnHibernationStateChanged(bool hibernating)
	{
		if (hibernating)
		{
			if (!model.hibernationTime.HasValue)
			{
				model.hibernationTime = timeDirector.WorldTime();
			}
		}
		else if (!model.hibernationTime.HasValue || timeDirector.TimeSince(model.hibernationTime.Value) >= (double)(metadata.impostoMinHibernationTime * 3600f))
		{
			ResetImpostos();
		}
	}
}
