using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SlimeLineupUI : MonoBehaviour
{
	public Dropdown dropdown;

	public SlimeLineup slimeLineup;

	public Button showSlimeButton;

	public Button showSlimesAndLargosButton;

	private SlimeDefinition[] baseSlimeTypes;

	private int selectedIndex;

	private void Start()
	{
		baseSlimeTypes = slimeLineup.slimeDefinitions.Slimes.Where((SlimeDefinition slime) => !slime.IsLargo).ToArray();
		dropdown.AddOptions(baseSlimeTypes.Select((SlimeDefinition slime) => slime.Name).ToList());
		dropdown.onValueChanged.AddListener(OnSlimeTypeSelected);
		showSlimeButton.onClick.AddListener(ShowSelectedSlime);
		showSlimesAndLargosButton.onClick.AddListener(ShowSelectedSlimeAndLargos);
	}

	public void OnSlimeTypeSelected(int index)
	{
		selectedIndex = index;
	}

	public void ShowSelectedSlime()
	{
		slimeLineup.ShowSlime(baseSlimeTypes[selectedIndex]);
	}

	public void ShowSelectedSlimeAndLargos()
	{
		slimeLineup.ShowSlimeAndLargos(baseSlimeTypes[selectedIndex]);
	}
}
