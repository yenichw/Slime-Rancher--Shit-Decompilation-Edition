using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

[Serializable]
public class OptionsData : DataModule<OptionsData>
{
	[Serializable]
	public class ButtonData
	{
		public string action;

		public KeyCode primary;

		public KeyCode secondary;

		public KeyCode gamepad;

		public ButtonData(string action, KeyCode primary, KeyCode secondary, KeyCode gamepad)
		{
			this.action = action;
			this.primary = primary;
			this.secondary = secondary;
			this.gamepad = gamepad;
		}
	}

	[Serializable]
	public class AxisData
	{
		public string action;

		public KeyCode primaryPos;

		public KeyCode primaryNeg;

		public KeyCode secondaryPos;

		public KeyCode secondaryNeg;

		public AxisData(string action, KeyCode primaryPos, KeyCode primaryNeg, KeyCode secondaryPos, KeyCode secondaryNeg)
		{
			this.action = action;
			this.primaryPos = primaryPos;
			this.primaryNeg = primaryNeg;
			this.secondaryPos = secondaryPos;
			this.secondaryNeg = secondaryNeg;
		}
	}

	[Serializable]
	public class BindingData
	{
		public string action;

		public Key primKey;

		public Mouse primMouse;

		public Key secondary;

		public InputControlType gamepad;

		public BindingData()
		{
		}

		public BindingData(string action, Key primKey, Mouse primMouse, Key secondary, InputControlType gamepad)
		{
			this.action = action;
			this.primKey = primKey;
			this.primMouse = primMouse;
			this.secondary = secondary;
			this.gamepad = gamepad;
		}
	}

	public const int CURR_FORMAT_ID = 2;

	public List<ButtonData> buttons = new List<ButtonData>();

	public List<AxisData> axes = new List<AxisData>();

	public bool disableCameraBob = true;

	public float masterVolume = 1f;

	public float musicVolume = 0.5f;

	public float sfxVolume = 1f;

	public string bugReportEmail;

	public bool bufferForGif;

	public float mouseSensitivity = 1f;

	public bool disableTutorials;

	public SRQualitySettings.Settings qualitySettings;

	public SRQualitySettings.Level qualityLevel = SRQualitySettings.Level.DEFAULT;

	public int screenWidth = 800;

	public int screenHeight = 600;

	public bool fullScreen = true;

	public bool swapSticks;

	public bool invertGamepadLookY;

	public bool invertMouseLookY;

	public bool disableGamepad;

	public bool vacLockOnHold;

	public float lookSensitivityX;

	public float lookSensitivityY = -0.2f;

	public List<BindingData> bindings = new List<BindingData>();
}
