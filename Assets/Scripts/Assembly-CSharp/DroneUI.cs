using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DroneUI : BaseUI
{
	private delegate DroneMetadata.Program DeriveProgram(DroneMetadata.Program program);

	public Transform programsParent;

	public TMP_Text warningText;

	public Button activateButton;

	public Button resetButton;

	private List<DroneUIProgram> programUIs = new List<DroneUIProgram>();

	private bool programsChanged;

	private const int GRID_COLUMNS = 6;

	private DroneGadget gadget;

	private DroneMetadata.Program[] programs;

	private DroneUIProgramPicker pickerUI;

	private DroneMetadata metadata => gadget.metadata;

	public DroneUI Init(DroneGadget gadget)
	{
		this.gadget = gadget;
		programs = this.gadget.programs.Select((DroneMetadata.Program p) => p.Clone()).ToArray();
		ResetUI();
		string programWarning = GetProgramWarning();
		warningText.gameObject.SetActive(programWarning != null);
		if (programWarning != null)
		{
			warningText.text = SRSingleton<GameContext>.Instance.MessageDirector.GetBundle("ui").Xlate(programWarning);
		}
		return this;
	}

	public void Start()
	{
		SECTR_AudioSystem.Play(metadata.onGuiEnableCue, Vector3.zero, loop: false);
	}

	public void OnEnable()
	{
		if (gadget != null)
		{
			SECTR_AudioSystem.Play(metadata.onGuiEnableCue, Vector3.zero, loop: false);
		}
	}

	public void OnDisable()
	{
		SECTR_AudioSystem.Play(metadata.onGuiDisableCue, Vector3.zero, loop: false);
	}

	private string GetProgramWarning()
	{
		if (!gadget.drone.ammo.IsEmpty())
		{
			return "w.drone_reprogram_drops_ammo";
		}
		return null;
	}

	private void ResetUI()
	{
		foreach (DroneUIProgram programUI in programUIs)
		{
			Destroyer.Destroy(programUI.gameObject, "DroneUI.ResetUI");
		}
		programUIs.Clear();
		for (int i = 0; i < programs.Length; i++)
		{
			DroneMetadata.Program program = programs[i];
			int? index = ((programs.Length >= 2) ? new int?(i + 1) : null);
			DroneUIProgram droneUIProgram = UnityEngine.Object.Instantiate(metadata.droneUIProgram.gameObject, programsParent).GetComponent<DroneUIProgram>().Init(program, index);
			programUIs.Add(droneUIProgram);
			int idx = i;
			SetProgramPicker(droneUIProgram.buttonTarget, program, (DroneMetadata.Program p) => new DroneMetadata.Program(metadata.GetDefaultTarget(), metadata.GetDefaultBehaviour(), metadata.GetDefaultBehaviour()), interactable: true, delegate(DroneMetadata.Program wp)
			{
				GatherTarget(wp, delegate(DroneMetadata.Program.Target tgt)
				{
					wp.target = tgt;
					programs[idx] = (program = wp);
					programsChanged = true;
				});
			});
			SetProgramPicker(droneUIProgram.buttonSource, program, (DroneMetadata.Program p) => new DroneMetadata.Program(p.target, metadata.GetDefaultBehaviour(), metadata.GetDefaultBehaviour()), program.target.id != "drone.target.none", delegate(DroneMetadata.Program wp)
			{
				GatherSource(wp, delegate(DroneMetadata.Program.Behaviour src)
				{
					wp.source = src;
					programs[idx] = (program = wp);
					programsChanged = true;
				});
			});
			SetProgramPicker(droneUIProgram.buttonDestination, program, (DroneMetadata.Program p) => new DroneMetadata.Program(p.target, p.source, metadata.GetDefaultBehaviour()), program.source.id != "drone.behaviour.none", delegate(DroneMetadata.Program wp)
			{
				GatherDestination(wp, delegate(DroneMetadata.Program.Behaviour dest)
				{
					wp.destination = dest;
					programs[idx] = (program = wp);
					programsChanged = true;
				});
			});
		}
		UpdateButtonState();
		SelectFirstButton();
		for (int j = 1; j < programUIs.Count; j++)
		{
			DroneUIProgram droneUIProgram2 = programUIs[j - 1];
			DroneUIProgram down = programUIs[j];
			droneUIProgram2.LinkGamepadNav(down);
		}
		programUIs.Last().LinkGamepadNav(activateButton.interactable ? activateButton : resetButton);
	}

	private void UpdateButtonState()
	{
		activateButton.interactable = programsChanged && programs.Any((DroneMetadata.Program p) => p.IsComplete());
		resetButton.interactable = programs.Any((DroneMetadata.Program p) => !p.IsReset());
		SRBehaviour.LinkNavigation(activateButton, resetButton, NavigationDirection.DOWN_UP);
	}

	private void SelectFirstButton()
	{
		for (int i = 0; i < programs.Length; i++)
		{
			if (programs[i].target.id == "drone.target.none")
			{
				programUIs[i].buttonTarget.button.Select();
				return;
			}
			if (programs[i].source.id == "drone.behaviour.none")
			{
				programUIs[i].buttonSource.button.Select();
				return;
			}
			if (programs[i].destination.id == "drone.behaviour.none")
			{
				programUIs[i].buttonDestination.button.Select();
				return;
			}
		}
		if (activateButton.interactable)
		{
			activateButton.Select();
		}
		else
		{
			programUIs[0].buttonTarget.button.Select();
		}
	}

	protected override bool Closeable()
	{
		if (base.Closeable())
		{
			return pickerUI == null;
		}
		return false;
	}

	public void OnClickConfirmation()
	{
		SECTR_AudioSystem.Play(metadata.onGuiButtonActivateCue, Vector3.zero, loop: false);
		gadget.SetPrograms(programs);
		Close();
	}

	public void OnClickReset()
	{
		SECTR_AudioSystem.Play(metadata.onGuiButtonResetCue, Vector3.zero, loop: false);
		DroneMetadata.Program[] array = programs;
		foreach (DroneMetadata.Program obj in array)
		{
			obj.target = metadata.GetDefaultTarget();
			obj.source = metadata.GetDefaultBehaviour();
			obj.destination = metadata.GetDefaultBehaviour();
		}
		gadget.SetPrograms(programs);
		programsChanged = false;
		ResetUI();
	}

	private void SetProgramPicker(DroneUIProgramButton button, DroneMetadata.Program program, DeriveProgram deriver, bool interactable, Action<DroneMetadata.Program> onClicked)
	{
		button.button.interactable = interactable;
		button.button.onClick.AddListener(delegate
		{
			DroneMetadata.Program obj = deriver(program);
			onClicked(obj);
		});
	}

	private void GatherTarget(DroneMetadata.Program workingProgram, Action<DroneMetadata.Program.Target> onComplete)
	{
		if (workingProgram.target.id == "drone.target.none")
		{
			CreatePicker("t.drone.pick_target", metadata.pickTargetIcon, metadata.targets, metadata.onGuiButtonTargetCue, onComplete);
		}
		else
		{
			onComplete(workingProgram.target);
		}
	}

	private void GatherSource(DroneMetadata.Program workingProgram, Action<DroneMetadata.Program.Behaviour> onComplete)
	{
		if (workingProgram.source.id == "drone.behaviour.none")
		{
			CreatePicker("t.drone.pick_source", metadata.pickSourceIcon, FilterSources(workingProgram, metadata.sources), metadata.onGuiButtonSourceCue, onComplete);
		}
		else
		{
			onComplete(workingProgram.source);
		}
	}

	private void GatherDestination(DroneMetadata.Program workingProgram, Action<DroneMetadata.Program.Behaviour> onComplete)
	{
		if (workingProgram.destination.id == "drone.behaviour.none")
		{
			CreatePicker("t.drone.pick_destination", metadata.pickDestinationIcon, FilterDestinations(workingProgram, metadata.destinations), metadata.onGuiButtonDestinationCue, onComplete);
		}
		else
		{
			onComplete(workingProgram.destination);
		}
	}

	private DroneMetadata.Program.Behaviour[] FilterSources(DroneMetadata.Program workingProgram, DroneMetadata.Program.Behaviour[] allSrcs)
	{
		return allSrcs.Where((DroneMetadata.Program.Behaviour s) => s.isCompatible(workingProgram)).ToArray();
	}

	private DroneMetadata.Program.Behaviour[] FilterDestinations(DroneMetadata.Program workingProgram, DroneMetadata.Program.Behaviour[] allDests)
	{
		return allDests.Where((DroneMetadata.Program.Behaviour d) => d.isCompatible(workingProgram) && d.id != workingProgram.source.id.Replace("source", "destination")).ToArray();
	}

	private void CreatePicker<T>(string title, Sprite titleIcon, T[] options, SECTR_AudioCue buttonCue, Action<T> onPicked) where T : DroneMetadata.Program.BaseComponent
	{
		if (pickerUI != null)
		{
			Destroyer.Destroy(pickerUI.gameObject, "DroneUI.SetProgramPicker");
		}
		pickerUI = UnityEngine.Object.Instantiate(metadata.droneUIProgramPicker.gameObject).GetComponent<DroneUIProgramPicker>();
		pickerUI.title.text = uiBundle.Get(title);
		pickerUI.icon.sprite = titleIcon;
		Button[] array = new Button[options.Length];
		for (int i = 0; i < options.Length; i++)
		{
			T option = options[i];
			DroneUIProgramButton droneUIProgramButton = UnityEngine.Object.Instantiate(metadata.droneUIProgramButton.gameObject, pickerUI.contentGrid).GetComponent<DroneUIProgramButton>().Init(option);
			droneUIProgramButton.button.onClick.AddListener(delegate
			{
				SECTR_AudioSystem.Play(buttonCue, Vector3.zero, loop: false);
				pickerUI.Close();
				onPicked(option);
			});
			array[i] = droneUIProgramButton.button;
			if (i == 0)
			{
				droneUIProgramButton.button.gameObject.AddComponent<InitSelected>();
			}
		}
		int num = Mathf.CeilToInt((float)array.Length / 6f);
		for (int j = 0; j < array.Length; j++)
		{
			int num2 = j / 6;
			int num3 = j % 6;
			Navigation navigation = array[j].navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (num2 > 0)
			{
				navigation.selectOnUp = array[(num2 - 1) * 6 + num3];
			}
			if (num2 < num - 1)
			{
				navigation.selectOnDown = array[Math.Min((num2 + 1) * 6 + num3, array.Length - 1)];
			}
			if (num3 > 0)
			{
				navigation.selectOnLeft = array[num2 * 6 + (num3 - 1)];
			}
			if (num3 < 5 && j < array.Length - 1)
			{
				navigation.selectOnRight = array[num2 * 6 + (num3 + 1)];
			}
			array[j].navigation = navigation;
		}
		DroneUIProgramPicker droneUIProgramPicker = pickerUI;
		droneUIProgramPicker.onDestroy = (OnDestroyDelegate)Delegate.Combine(droneUIProgramPicker.onDestroy, (OnDestroyDelegate)delegate
		{
			if (SRSingleton<SceneContext>.Instance != null && this != null && base.gameObject != null)
			{
				ResetUI();
			}
		});
	}
}
