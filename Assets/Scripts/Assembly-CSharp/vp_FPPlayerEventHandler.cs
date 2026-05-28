using UnityEngine;

public class vp_FPPlayerEventHandler : vp_PlayerEventHandler
{
	public vp_Message<string> HUDText = new vp_Message<string>("HUDText");

	public vp_Value<Texture> Crosshair = new vp_Value<Texture>("Crosshair");

	public vp_Value<Texture2D> CurrentAmmoIcon = new vp_Value<Texture2D>("CurrentAmmoIcon");

	public vp_Value<Vector2> InputSmoothLook = new vp_Value<Vector2>("InputSmootLook");

	public vp_Value<Vector2> InputRawLook = new vp_Value<Vector2>("InputRawLook");

	public vp_Message<string, bool> InputGetButton = new vp_Message<string, bool>("InputGetButton");

	public vp_Message<string, bool> InputGetButtonUp = new vp_Message<string, bool>("InputGetButtonUp");

	public vp_Message<string, bool> InputGetButtonDown = new vp_Message<string, bool>("InputGetButtonDown");

	public vp_Value<bool> InputAllowGameplay = new vp_Value<bool>("InputAllowGameplay");

	public vp_Value<bool> Pause = new vp_Value<bool>("Pause");

	public vp_Value<Vector3> CameraLookDirection = new vp_Value<Vector3>("CameraLookDirection");

	public vp_Message CameraToggle3rdPerson = new vp_Message("CameraToggle3rdPerson");

	public vp_Message<float> CameraGroundStomp = new vp_Message<float>("CameraGroundStomp");

	public vp_Message<float> CameraBombShake = new vp_Message<float>("CameraBombShake");

	public vp_Value<Vector3> CameraEarthQuakeForce = new vp_Value<Vector3>("CameraEarthQuakeForce");

	public vp_Activity<Vector3> CameraEarthQuake = new vp_Activity<Vector3>("CameraEarthQuake");

	public vp_Value<string> CurrentWeaponClipType = new vp_Value<string>("CurrentWeaponClipType");

	public vp_Attempt<object> AddAmmo = new vp_Attempt<object>("AddAmmo");

	public vp_Attempt RemoveClip = new vp_Attempt("RemoveClip");

	protected override void Awake()
	{
		base.Awake();
	}

	public vp_FPPlayerEventHandler()
	{
		AddEvents();
	}

	private void AddEvents()
	{
		m_HandlerEvents.Add("HUDText", HUDText);
		m_HandlerEvents.Add("Crosshair", Crosshair);
		m_HandlerEvents.Add("CurrentAmmoIcon", CurrentAmmoIcon);
		m_HandlerEvents.Add("InputSmoothLook", InputSmoothLook);
		m_HandlerEvents.Add("InputRawLook", InputRawLook);
		m_HandlerEvents.Add("InputGetButton", InputGetButton);
		m_HandlerEvents.Add("InputGetButtonUp", InputGetButtonUp);
		m_HandlerEvents.Add("InputGetButtonDown", InputGetButtonDown);
		m_HandlerEvents.Add("InputAllowGameplay", InputAllowGameplay);
		m_HandlerEvents.Add("Pause", Pause);
		m_HandlerEvents.Add("CameraLookDirection", CameraLookDirection);
		m_HandlerEvents.Add("CameraToggle3rdPerson", CameraToggle3rdPerson);
		m_HandlerEvents.Add("CameraGroundStomp", CameraGroundStomp);
		m_HandlerEvents.Add("CameraBombShake", CameraBombShake);
		m_HandlerEvents.Add("CameraEarthQuakeForce", CameraEarthQuakeForce);
		m_HandlerEvents.Add("CameraEarthQuake", CameraEarthQuake);
		m_HandlerEvents.Add("CurrentWeaponClipType", CurrentWeaponClipType);
		m_HandlerEvents.Add("AddAmmo", AddAmmo);
		m_HandlerEvents.Add("RemoveClip", RemoveClip);
	}
}
