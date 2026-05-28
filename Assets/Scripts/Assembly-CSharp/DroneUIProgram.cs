using UnityEngine;
using UnityEngine.UI;

public class DroneUIProgram : SRBehaviour
{
	[Tooltip("Drone program button: target")]
	public DroneUIProgramButton buttonTarget;

	[Tooltip("Drone program button: source")]
	public DroneUIProgramButton buttonSource;

	[Tooltip("Drone program button: destination")]
	public DroneUIProgramButton buttonDestination;

	public DroneUIProgram Init(DroneMetadata.Program program, int? index)
	{
		buttonTarget.Init(program.target, new DroneUIProgramButton.Title
		{
			type = DroneUIProgramButton.Title.Type.TARGET,
			index = index
		});
		buttonSource.Init(program.source, new DroneUIProgramButton.Title
		{
			type = DroneUIProgramButton.Title.Type.SOURCE,
			index = index
		});
		buttonDestination.Init(program.destination, new DroneUIProgramButton.Title
		{
			type = DroneUIProgramButton.Title.Type.DESTINATION,
			index = index
		});
		return this;
	}

	public void LinkGamepadNav(DroneUIProgram down)
	{
		LinkDefaultNavigation();
		SRBehaviour.LinkNavigation(buttonTarget.button, down.GetButtonOrRightmost(down.buttonTarget.button), NavigationDirection.DOWN);
		SRBehaviour.LinkNavigation(buttonSource.button, down.GetButtonOrRightmost(down.buttonSource.button), NavigationDirection.DOWN);
		SRBehaviour.LinkNavigation(buttonDestination.button, down.GetButtonOrRightmost(down.buttonDestination.button), NavigationDirection.DOWN);
		SRBehaviour.LinkNavigation(down.buttonTarget.button, GetButtonOrRightmost(buttonTarget.button), NavigationDirection.UP);
		SRBehaviour.LinkNavigation(down.buttonSource.button, GetButtonOrRightmost(buttonSource.button), NavigationDirection.UP);
		SRBehaviour.LinkNavigation(down.buttonDestination.button, GetButtonOrRightmost(buttonDestination.button), NavigationDirection.UP);
	}

	public void LinkGamepadNav(Selectable down)
	{
		LinkDefaultNavigation();
		SRBehaviour.LinkNavigation(buttonTarget.button, down, NavigationDirection.DOWN_UP);
		SRBehaviour.LinkNavigation(buttonSource.button, down, NavigationDirection.DOWN);
		SRBehaviour.LinkNavigation(buttonDestination.button, down, NavigationDirection.DOWN);
	}

	private void LinkDefaultNavigation()
	{
		SRBehaviour.LinkNavigation(buttonTarget.button, buttonSource.button, NavigationDirection.RIGHT_LEFT);
		SRBehaviour.LinkNavigation(buttonSource.button, buttonDestination.button, NavigationDirection.RIGHT_LEFT);
	}

	private Selectable GetButtonOrRightmost(Selectable selectable)
	{
		if (selectable.interactable)
		{
			return selectable;
		}
		if (selectable == buttonDestination.button)
		{
			return GetButtonOrRightmost(buttonSource.button);
		}
		return buttonTarget.button;
	}
}
