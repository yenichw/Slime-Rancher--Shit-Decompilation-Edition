using UnityEngine;
using UnityEngine.UI;

public class GardenCountdownUI : MonoBehaviour
{
	public GameObject mainPanel;

	public Image cropImg;

	public LandPlot plot;

	public void Update()
	{
		Identifiable.Id attachedCropId = plot.GetAttachedCropId();
		mainPanel.SetActive(attachedCropId != Identifiable.Id.NONE);
		if (attachedCropId != 0)
		{
			cropImg.sprite = SRSingleton<GameContext>.Instance.LookupDirector.GetIcon(attachedCropId);
		}
	}
}
