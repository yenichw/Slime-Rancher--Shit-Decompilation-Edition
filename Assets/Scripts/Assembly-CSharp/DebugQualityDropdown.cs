using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugQualityDropdown : MonoBehaviour
{
	public Dropdown dropdown;

	public void Awake()
	{
		List<SRQualitySettings.Level> levels = new List<SRQualitySettings.Level>
		{
			SRQualitySettings.Level.LOWEST,
			SRQualitySettings.Level.LOW,
			SRQualitySettings.Level.DEFAULT,
			SRQualitySettings.Level.HIGH,
			SRQualitySettings.Level.VERY_HIGH
		};
		dropdown.ClearOptions();
		dropdown.AddOptions(levels.Select((SRQualitySettings.Level level) => Enum.GetName(typeof(SRQualitySettings.Level), level)).ToList());
		dropdown.onValueChanged.AddListener(delegate(int index)
		{
			SRQualitySettings.CurrentLevel = levels[index];
		});
		dropdown.value = 2;
	}
}
