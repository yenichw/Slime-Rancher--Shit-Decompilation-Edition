using System.Collections.Generic;
using System.Linq;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

public class EchoNoteGordoRanchTeleporter : SRBehaviour
{
	[Tooltip("Parent GameObject containing the portal ring.")]
	public GameObject ring;

	public void OnEnable()
	{
		HolidayModel holiday = SRSingleton<SceneContext>.Instance.GameModel.GetHolidayModel();
		ring.SetActive(SRSingleton<SceneContext>.Instance.GameModel.AllEchoNoteGordos().Any((KeyValuePair<string, EchoNoteGordoModel> pair) => pair.Value.state == EchoNoteGordoModel.State.POPPED && holiday.eventEchoNoteGordos.Any((HolidayModel.EventEchoNoteGordo e) => e.objectId == pair.Key)));
	}
}
