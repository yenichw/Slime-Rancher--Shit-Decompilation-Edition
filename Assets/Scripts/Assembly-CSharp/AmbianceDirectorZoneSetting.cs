using UnityEngine;

[CreateAssetMenu(menuName = "Ambiance Director/Zone Setting", fileName = "AmbianceDirectorZoneSetting")]
public class AmbianceDirectorZoneSetting : ScriptableObject
{
	public AmbianceDirector.Zone zone;

	public Color dayFogColor;

	public float dayFogDensity;

	public Color dayAmbientColor;

	public Color nightFogColor;

	public float nightFogDensity;

	public Color nightAmbientColor;

	public Color daySkyColor;

	public Color daySkyHorizon;

	public Color nightSkyColor;

	public Color nightSkyHorizon;

	public AmbianceDirectorZoneSetting Clone()
	{
		AmbianceDirectorZoneSetting ambianceDirectorZoneSetting = ScriptableObject.CreateInstance<AmbianceDirectorZoneSetting>();
		ambianceDirectorZoneSetting.zone = zone;
		ambianceDirectorZoneSetting.dayFogColor = dayFogColor;
		ambianceDirectorZoneSetting.dayFogDensity = dayFogDensity;
		ambianceDirectorZoneSetting.dayAmbientColor = dayAmbientColor;
		ambianceDirectorZoneSetting.nightFogColor = nightFogColor;
		ambianceDirectorZoneSetting.nightFogDensity = nightFogDensity;
		ambianceDirectorZoneSetting.nightAmbientColor = nightAmbientColor;
		ambianceDirectorZoneSetting.daySkyColor = daySkyColor;
		ambianceDirectorZoneSetting.daySkyHorizon = daySkyHorizon;
		ambianceDirectorZoneSetting.nightSkyColor = nightSkyColor;
		ambianceDirectorZoneSetting.nightSkyHorizon = nightSkyHorizon;
		return ambianceDirectorZoneSetting;
	}
}
