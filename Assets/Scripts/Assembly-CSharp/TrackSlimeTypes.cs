using System;
using System.Collections.Generic;

public class TrackSlimeTypes : SRBehaviour
{
	public enum Mode
	{
		SLIMES = 0,
		LARGOS = 1
	}

	public Mode mode;

	public AchievementsDirector.IntStat stat;

	public bool trackPlayerEnteredSlimesStat;

	private HashSet<Identifiable.Id> workingSet = new HashSet<Identifiable.Id>();

	private List<Identifiable> workingList = new List<Identifiable>();

	private TrackContainedIdentifiables trackingContainer;

	public void Awake()
	{
		trackingContainer = GetComponent<TrackContainedIdentifiables>();
		TrackContainedIdentifiables trackContainedIdentifiables = trackingContainer;
		trackContainedIdentifiables.OnIdentifiableEntered = (TrackContainedIdentifiables.IdentifiableEntered)Delegate.Combine(trackContainedIdentifiables.OnIdentifiableEntered, new TrackContainedIdentifiables.IdentifiableEntered(OnIdentifiableEntered));
		TrackContainedIdentifiables trackContainedIdentifiables2 = trackingContainer;
		trackContainedIdentifiables2.OnNewIdentifiableTypeEntered = (TrackContainedIdentifiables.NewIdentifiableTypeEntered)Delegate.Combine(trackContainedIdentifiables2.OnNewIdentifiableTypeEntered, new TrackContainedIdentifiables.NewIdentifiableTypeEntered(OnNewIdentifiableTypeEntered));
	}

	public void OnDestroy()
	{
		TrackContainedIdentifiables trackContainedIdentifiables = trackingContainer;
		trackContainedIdentifiables.OnIdentifiableEntered = (TrackContainedIdentifiables.IdentifiableEntered)Delegate.Remove(trackContainedIdentifiables.OnIdentifiableEntered, new TrackContainedIdentifiables.IdentifiableEntered(OnIdentifiableEntered));
		TrackContainedIdentifiables trackContainedIdentifiables2 = trackingContainer;
		trackContainedIdentifiables2.OnNewIdentifiableTypeEntered = (TrackContainedIdentifiables.NewIdentifiableTypeEntered)Delegate.Remove(trackContainedIdentifiables2.OnNewIdentifiableTypeEntered, new TrackContainedIdentifiables.NewIdentifiableTypeEntered(OnNewIdentifiableTypeEntered));
	}

	public void OnIdentifiableEntered(TrackContainedIdentifiables container, Identifiable ident)
	{
		HashSet<Identifiable.Id> identifiablesForMode = GetIdentifiablesForMode();
		if (trackPlayerEnteredSlimesStat && ident.id == Identifiable.Id.PLAYER)
		{
			workingList.Clear();
			container.GetTrackedItemsOfClass(identifiablesForMode, workingList);
			SRSingleton<SceneContext>.Instance.AchievementsDirector.MaybeUpdateMaxStat(AchievementsDirector.IntStat.ENTERED_CORRAL_SLIMES, workingList.Count);
			workingList.Clear();
		}
	}

	public void OnNewIdentifiableTypeEntered(TrackContainedIdentifiables container, Identifiable ident)
	{
		workingSet.Clear();
		workingSet.UnionWith(GetIdentifiablesForMode());
		container.GetTrackedIdentifiableTypes(workingSet);
		SRSingleton<SceneContext>.Instance.AchievementsDirector.MaybeUpdateMaxStat(stat, workingSet.Count);
		workingSet.Clear();
	}

	private HashSet<Identifiable.Id> GetIdentifiablesForMode()
	{
		switch (mode)
		{
		case Mode.LARGOS:
			return Identifiable.LARGO_CLASS;
		case Mode.SLIMES:
			return Identifiable.EATERS_CLASS;
		default:
			return new HashSet<Identifiable.Id>();
		}
	}
}
