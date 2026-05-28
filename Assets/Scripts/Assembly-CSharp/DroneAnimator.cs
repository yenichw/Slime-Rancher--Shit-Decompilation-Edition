using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneAnimator : SRAnimator<Drone>
{
	public enum Id
	{
		IDLE = 0,
		MOVE = 1,
		GATHER = 2,
		DEPOSIT = 3,
		REST = 4,
		IDLE_CELEBRATE = 5,
		IDLE_GRUMP = 6
	}

	public class IdComparer : IEqualityComparer<Id>
	{
		public static IdComparer Instance = new IdComparer();

		public bool Equals(Id a, Id b)
		{
			return a == b;
		}

		public int GetHashCode(Id a)
		{
			return (int)a;
		}
	}

	private static Dictionary<Id, int> ANIMATION_DICT;

	private static readonly int HAS_BATTERY;

	private Dictionary<DroneAnimatorState.Id, Action> onStateExit = new Dictionary<DroneAnimatorState.Id, Action>(DroneAnimatorState.IdComparer.Instance);

	private DroneStationBattery battery => base.parent.station.battery;

	public override void Awake()
	{
		base.Awake();
		SetAnimation(Id.IDLE);
	}

	public void Start()
	{
		battery.onHasAnyChanged -= OnBatteryHasAnyChanged;
		battery.onHasAnyChanged += OnBatteryHasAnyChanged;
		OnBatteryHasAnyChanged();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (battery != null)
		{
			battery.onHasAnyChanged -= OnBatteryHasAnyChanged;
			battery.onHasAnyChanged += OnBatteryHasAnyChanged;
			OnBatteryHasAnyChanged();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		battery.onHasAnyChanged -= OnBatteryHasAnyChanged;
	}

	static DroneAnimator()
	{
		HAS_BATTERY = Animator.StringToHash("HAS_BATTERY");
		ANIMATION_DICT = new Dictionary<Id, int>(IdComparer.Instance);
		foreach (Id item in Enum.GetValues(typeof(Id)).Cast<Id>())
		{
			ANIMATION_DICT.Add(item, Animator.StringToHash(Enum.GetName(typeof(Id), item)));
		}
	}

	public void SetAnimation(Id id)
	{
		onStateExit.Clear();
		if (!base.animator.isInitialized)
		{
			return;
		}
		foreach (KeyValuePair<Id, int> item in ANIMATION_DICT)
		{
			base.animator.SetBool(item.Value, item.Key == id);
		}
	}

	private void OnBatteryHasAnyChanged()
	{
		base.animator.SetBool(HAS_BATTERY, battery.HasAny());
	}

	public void OnStateExit(DroneAnimatorState.Id id, Action callback)
	{
		if (onStateExit.ContainsKey(id))
		{
			Dictionary<DroneAnimatorState.Id, Action> dictionary = onStateExit;
			dictionary[id] = (Action)Delegate.Combine(dictionary[id], callback);
		}
		else
		{
			onStateExit[id] = callback;
		}
	}

	public void OnStateExit(DroneAnimatorState.Id id)
	{
		if (onStateExit.TryGetValue(id, out var value) && value != null)
		{
			value();
			onStateExit[id] = null;
		}
	}
}
