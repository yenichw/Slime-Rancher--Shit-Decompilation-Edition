using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPInput : vp_Component
{
	public Vector2 MouseLookSensitivity = new Vector2(5f, 5f);

	public int MouseLookSmoothSteps = 10;

	public float MouseLookSmoothWeight = 0.5f;

	public bool MouseLookAcceleration;

	public float MouseLookAccelerationThreshold = 0.4f;

	public bool MouseLookInvert;

	protected Vector2 m_MouseLookSmoothMove = Vector2.zero;

	protected Vector2 m_MouseLookRawMove = Vector2.zero;

	protected List<Vector2> m_MouseLookSmoothBuffer = new List<Vector2>();

	protected int m_LastMouseLookFrame = -1;

	protected Vector2 m_CurrentMouseLook = Vector2.zero;

	public Rect[] MouseCursorZones;

	public bool MouseCursorForced;

	public bool MouseCursorBlocksMouseLook = true;

	protected Vector2 m_MousePos = Vector2.zero;

	protected Vector2 m_MoveVector = Vector2.zero;

	protected bool m_AllowGameplayInput = true;

	protected vp_FPPlayerEventHandler m_FPPlayer;

	private OptionsDirector optionsDir;

	private InputDirector inputDir;

	public Vector2 MousePos => m_MousePos;

	public bool AllowGameplayInput
	{
		get
		{
			return m_AllowGameplayInput;
		}
		set
		{
			m_AllowGameplayInput = value;
		}
	}

	public vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
			}
			return m_FPPlayer;
		}
	}

	protected virtual Vector2 OnValue_InputMoveVector
	{
		get
		{
			return m_MoveVector;
		}
		set
		{
			m_MoveVector = ((value.sqrMagnitude > 1f) ? value.normalized : value);
		}
	}

	protected virtual float OnValue_InputClimbVector => SRInput.Actions.vertical.RawValue;

	protected virtual bool OnValue_InputAllowGameplay
	{
		get
		{
			return m_AllowGameplayInput;
		}
		set
		{
			m_AllowGameplayInput = value;
		}
	}

	protected virtual bool OnValue_Pause
	{
		get
		{
			return vp_TimeUtility.Paused;
		}
		set
		{
			vp_TimeUtility.Paused = !vp_Gameplay.isMultiplayer && value;
		}
	}

	protected virtual Vector2 OnValue_InputSmoothLook => GetMouseLook();

	protected virtual Vector2 OnValue_InputRawLook => GetMouseLookRaw();

	protected override void Awake()
	{
		base.Awake();
		optionsDir = SRSingleton<GameContext>.Instance.OptionsDirector;
		inputDir = SRSingleton<GameContext>.Instance.InputDirector;
	}

	protected override void OnEnable()
	{
		if (FPPlayer != null)
		{
			Register(FPPlayer);
		}
	}

	protected override void OnDisable()
	{
		if (FPPlayer != null)
		{
			Unregister(FPPlayer);
		}
	}

	protected override void Update()
	{
		UpdateCursorLock();
		UpdatePause();
		if (!FPPlayer.Pause.Get() && m_AllowGameplayInput)
		{
			InputInteract();
			InputMove();
			InputRun();
			InputJump();
			InputCrouch();
			InputAttack();
			InputReload();
			InputSetWeapon();
			InputCamera();
		}
	}

	protected virtual void InputInteract()
	{
		if (SRInput.Actions.interact.WasReleased)
		{
			FPPlayer.Interact.TryStart();
		}
		else
		{
			FPPlayer.Interact.TryStop();
		}
	}

	protected virtual void InputMove()
	{
		Vector2 vector = new Vector2(SRInput.Actions.horizontal, SRInput.Actions.vertical);
		Vector2 o = (InputDirector.UsingGamepad() ? ApplyRadialDeadZone(vector, inputDir.ControllerStickDeadZone) : vector);
		FPPlayer.InputMoveVector.Set(o);
	}

	private Vector2 ApplyRadialDeadZone(Vector2 v, float deadZone)
	{
		float magnitude = v.magnitude;
		if (magnitude < deadZone)
		{
			return Vector2.zero;
		}
		return v.normalized * ((magnitude - deadZone) / (1f - deadZone));
	}

	protected virtual void InputRun()
	{
		if (optionsDir.sprintHold ? SRInput.Actions.run.IsPressed : (FPPlayer.Run.Active ^ SRInput.Actions.run.WasPressed))
		{
			FPPlayer.Run.TryStart();
		}
		else
		{
			FPPlayer.Run.TryStop();
		}
	}

	protected virtual void InputJump()
	{
		if (SRInput.Actions.jump.IsPressed)
		{
			if (!FPPlayer.Jump.TryStart() && !FPPlayer.Jump.Active)
			{
				FPPlayer.Jetpack.TryStart();
			}
		}
		else
		{
			FPPlayer.Jump.Stop();
			FPPlayer.Jetpack.Stop();
		}
	}

	protected virtual void InputCrouch()
	{
	}

	protected virtual void InputCamera()
	{
	}

	protected virtual void InputAttack()
	{
		if (vp_Utility.LockCursor)
		{
			if (SRInput.Actions.attack.IsPressed)
			{
				FPPlayer.Attack.TryStart();
			}
			else
			{
				FPPlayer.Attack.TryStop();
			}
		}
	}

	protected virtual void InputReload()
	{
	}

	protected virtual void InputSetWeapon()
	{
	}

	protected virtual void UpdatePause()
	{
	}

	protected virtual void UpdateCursorLock()
	{
		m_MousePos.x = Input.mousePosition.x;
		m_MousePos.y = (float)Screen.height - Input.mousePosition.y;
		if (MouseCursorForced)
		{
			if (vp_Utility.LockCursor)
			{
				vp_Utility.LockCursor = false;
			}
		}
		else
		{
			if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
			{
				return;
			}
			if (MouseCursorZones.Length != 0)
			{
				Rect[] mouseCursorZones = MouseCursorZones;
				foreach (Rect rect in mouseCursorZones)
				{
					if (rect.Contains(m_MousePos))
					{
						if (vp_Utility.LockCursor)
						{
							vp_Utility.LockCursor = false;
						}
						return;
					}
				}
			}
			if (!vp_Utility.LockCursor)
			{
				vp_Utility.LockCursor = true;
			}
		}
	}

	protected virtual Vector2 GetMouseLook()
	{
		if (MouseCursorBlocksMouseLook && !vp_Utility.LockCursor)
		{
			return Vector2.zero;
		}
		if (m_LastMouseLookFrame == Time.frameCount)
		{
			return m_CurrentMouseLook;
		}
		m_LastMouseLookFrame = Time.frameCount;
		Vector2 mouseLook = SRInput.Instance.GetMouseLook();
		Vector2 vector = mouseLook;
		if (InputDirector.UsingGamepad())
		{
			vector = ApplyRadialDeadZone(mouseLook, inputDir.ControllerStickDeadZone);
			vector.x *= inputDir.GetGamepadLookSensitivityXFactor();
			vector.y *= inputDir.GetGamepadLookSensitivityYFactor();
		}
		m_MouseLookSmoothMove.x = vector.x * Time.timeScale;
		m_MouseLookSmoothMove.y = vector.y * Time.timeScale;
		MouseLookSmoothSteps = Mathf.Clamp(MouseLookSmoothSteps, 1, 20);
		float num = (inputDir.GetDisableMouseLookSmooth() ? 0f : (Mathf.Clamp01(MouseLookSmoothWeight) / base.Delta));
		while (m_MouseLookSmoothBuffer.Count > MouseLookSmoothSteps)
		{
			m_MouseLookSmoothBuffer.RemoveAt(0);
		}
		m_MouseLookSmoothBuffer.Add(m_MouseLookSmoothMove);
		float num2 = 1f;
		Vector2 zero = Vector2.zero;
		float num3 = 0f;
		for (int num4 = m_MouseLookSmoothBuffer.Count - 1; num4 > 0; num4--)
		{
			zero += m_MouseLookSmoothBuffer[num4] * num2;
			num3 += 1f * num2;
			num2 *= num;
		}
		num3 = Mathf.Max(1f, num3);
		m_CurrentMouseLook = vp_MathUtility.NaNSafeVector2(zero / num3);
		float num5 = 0f;
		float num6 = Mathf.Abs(m_CurrentMouseLook.x);
		float num7 = Mathf.Abs(m_CurrentMouseLook.y);
		if (MouseLookAcceleration)
		{
			num5 = Mathf.Sqrt(num6 * num6 + num7 * num7) / base.Delta;
			num5 = ((num5 <= MouseLookAccelerationThreshold) ? 0f : num5);
		}
		m_CurrentMouseLook.x *= MouseLookSensitivity.x + num5;
		m_CurrentMouseLook.y *= MouseLookSensitivity.y + num5;
		m_CurrentMouseLook.y = (MouseLookInvert ? m_CurrentMouseLook.y : (0f - m_CurrentMouseLook.y));
		return m_CurrentMouseLook;
	}

	protected virtual Vector2 GetMouseLookRaw()
	{
		if (MouseCursorBlocksMouseLook && !vp_Utility.LockCursor)
		{
			return Vector2.zero;
		}
		Vector2 mouseLookRaw = SRInput.Instance.GetMouseLookRaw();
		Vector2 vector = ApplyRadialDeadZone(mouseLookRaw, inputDir.ControllerStickDeadZone);
		m_MouseLookRawMove.x = vector.x;
		m_MouseLookRawMove.y = vector.y;
		return m_MouseLookRawMove;
	}

	protected virtual Vector2 Get_InputMoveVector()
	{
		return m_MoveVector;
	}

	protected virtual void Set_InputMoveVector(Vector2 value)
	{
		m_MoveVector = ((value.sqrMagnitude > 1f) ? value.normalized : value);
	}

	protected virtual float Get_InputClimbVector()
	{
		return SRInput.Actions.vertical.RawValue;
	}

	protected virtual bool Get_InputAllowGameplay()
	{
		return m_AllowGameplayInput;
	}

	protected virtual void Set_InputAllowGameplay(bool value)
	{
		m_AllowGameplayInput = value;
	}

	protected virtual bool Get_Pause()
	{
		return vp_TimeUtility.Paused;
	}

	protected virtual void Set_Pause(bool value)
	{
		vp_TimeUtility.Paused = !vp_Gameplay.isMultiplayer && value;
	}

	protected virtual bool OnMessage_InputGetButton(string button)
	{
		throw new NotImplementedException();
	}

	protected virtual bool OnMessage_InputGetButtonUp(string button)
	{
		throw new NotImplementedException();
	}

	protected virtual bool OnMessage_InputGetButtonDown(string button)
	{
		throw new NotImplementedException();
	}

	public override void Register(vp_EventHandler eventHandler)
	{
		base.Register(eventHandler);
		eventHandler.RegisterMessage<string, bool>("InputGetButton", OnMessage_InputGetButton);
		eventHandler.RegisterMessage<string, bool>("InputGetButtonDown", OnMessage_InputGetButtonDown);
		eventHandler.RegisterMessage<string, bool>("InputGetButtonUp", OnMessage_InputGetButtonUp);
		eventHandler.RegisterValue("InputSmoothLook", GetMouseLook, null);
		eventHandler.RegisterValue("InputRawLook", GetMouseLookRaw, null);
		eventHandler.RegisterValue("InputAllowGameplay", Get_InputAllowGameplay, Set_InputAllowGameplay);
		eventHandler.RegisterValue("InputClimbVector", Get_InputClimbVector, null);
		eventHandler.RegisterValue("InputMoveVector", Get_InputMoveVector, Set_InputMoveVector);
		eventHandler.RegisterValue("Pause", Get_Pause, Set_Pause);
	}

	public override void Unregister(vp_EventHandler eventHandler)
	{
		base.Unregister(eventHandler);
		eventHandler.UnregisterMessage<string, bool>("InputGetButton", OnMessage_InputGetButton);
		eventHandler.UnregisterMessage<string, bool>("InputGetButtonDown", OnMessage_InputGetButtonDown);
		eventHandler.UnregisterMessage<string, bool>("InputGetButtonUp", OnMessage_InputGetButtonUp);
		eventHandler.UnregisterValue("InputSmoothLook", GetMouseLook, null);
		eventHandler.UnregisterValue("InputRawLook", GetMouseLookRaw, null);
		eventHandler.UnregisterValue("InputAllowGameplay", Get_InputAllowGameplay, Set_InputAllowGameplay);
		eventHandler.UnregisterValue("InputClimbVector", Get_InputClimbVector, null);
		eventHandler.UnregisterValue("InputMoveVector", Get_InputMoveVector, Set_InputMoveVector);
		eventHandler.UnregisterValue("Pause", Get_Pause, Set_Pause);
	}
}
