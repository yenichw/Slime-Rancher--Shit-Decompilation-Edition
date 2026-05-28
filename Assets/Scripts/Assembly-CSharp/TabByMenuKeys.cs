using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class TabByMenuKeys : MonoBehaviour
{
	public static bool disabledForBinding;

	private Toggle[] tabs;

	private int currIdx;

	public void Awake()
	{
		tabs = GetComponentsInChildren<Toggle>(includeInactive: false);
	}

	public void Start()
	{
		tabs = GetComponentsInChildren<Toggle>(includeInactive: false);
	}

	public void Update()
	{
		if (!disabledForBinding)
		{
			if (SRInput.PauseActions.menuTabRight.WasPressed)
			{
				SelectNextTab();
			}
			else if (SRInput.PauseActions.menuTabLeft.WasPressed)
			{
				SelectPrevTab();
			}
		}
	}

	public void SelectNextTab()
	{
		currIdx = Math.Min(currIdx + 1, tabs.Length - 1);
		tabs[currIdx].isOn = true;
	}

	public void SelectPrevTab()
	{
		currIdx = Math.Max(currIdx - 1, 0);
		tabs[currIdx].isOn = true;
	}

	public void RecalcSelected()
	{
		if (tabs == null)
		{
			return;
		}
		for (int i = 0; i < tabs.Length; i++)
		{
			if (tabs[i].isOn)
			{
				currIdx = i;
				break;
			}
		}
	}
}
