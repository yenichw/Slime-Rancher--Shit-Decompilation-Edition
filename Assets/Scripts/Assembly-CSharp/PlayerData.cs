using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData : DataModule<PlayerData>
{
	public const int CURR_FORMAT_ID = 3;

	public Vector3 playerPos;

	public Vector3 playerRotEuler;

	public int health;

	public int energy;

	public int rad;

	public int currency;

	public Ammo.AmmoData[] ammo;

	public List<PlayerState.Upgrade> upgrades;

	public Dictionary<PlayerState.Upgrade, float> upgradeLocks;

	public List<MailDirector.Mail> mail;

	public int keys;

	public Dictionary<ProgressDirector.ProgressType, int> progress;

	public Dictionary<ProgressDirector.ProgressType, List<float>> delayedProgress;

	public int currencyEverCollected;

	public PlayerState.GameMode gameMode;

	public Identifiable.Id gameIconId = Identifiable.Id.CARROT_VEGGIE;

	public string version = "0.3.0";

	public static void AssertEquals(PlayerData dataA, PlayerData dataB)
	{
		TestUtil.AreApproximatelyEqual(dataA.playerPos, dataB.playerPos, 0.01f, string.Concat("Player position: ", dataA.playerPos, " vs ", dataB.playerPos));
		TestUtil.AreApproximatelyEqual(dataA.playerRotEuler, dataB.playerRotEuler, 0.01f, string.Concat("Player rotation: ", dataA.playerRotEuler, " vs ", dataB.playerRotEuler));
	}

	private static string PrintAmmo(Ammo.AmmoData[] allAmmo)
	{
		string text = "Ammo: ";
		foreach (Ammo.AmmoData ammoData in allAmmo)
		{
			text += ((ammoData == null) ? "null," : string.Concat(ammoData.id, ":", ammoData.count, ","));
		}
		return text;
	}

	private static string PrintDelayedProgress(Dictionary<ProgressDirector.ProgressType, List<float>> delayedProg)
	{
		string text = "DelayedProg: ";
		foreach (KeyValuePair<ProgressDirector.ProgressType, List<float>> item in delayedProg)
		{
			text = string.Concat(text, item.Key, ":");
			foreach (float item2 in item.Value)
			{
				text = text + item2 + ",";
			}
			text += ";";
		}
		return text;
	}
}
