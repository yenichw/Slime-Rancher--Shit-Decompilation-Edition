using System;
using UnityEngine;

public class MapControls : MonoBehaviour
{
	public GameObject upButton;

	public GameObject downButton;

	public GameObject leftButton;

	public GameObject rightButton;

	public GameObject zoomInButton;

	public GameObject zoomOutButton;

	public GameObject scrollView;

	private MapScrollRect mapScrollRect;

	public void Awake()
	{
		if (scrollView != null)
		{
			mapScrollRect = scrollView.GetComponent<MapScrollRect>();
			WireButton(upButton, mapScrollRect.ScrollUp);
			WireButton(downButton, mapScrollRect.ScrollDown);
			WireButton(leftButton, mapScrollRect.ScrollLeft);
			WireButton(rightButton, mapScrollRect.ScrollRight);
			WireButton(zoomInButton, mapScrollRect.ZoomIn);
			WireButton(zoomOutButton, mapScrollRect.ZoomOut);
		}
	}

	public void OnDestroy()
	{
		if (mapScrollRect != null)
		{
			UnwireButton(upButton, mapScrollRect.ScrollUp);
			UnwireButton(downButton, mapScrollRect.ScrollDown);
			UnwireButton(leftButton, mapScrollRect.ScrollLeft);
			UnwireButton(rightButton, mapScrollRect.ScrollRight);
			UnwireButton(zoomInButton, mapScrollRect.ZoomIn);
			UnwireButton(zoomOutButton, mapScrollRect.ZoomOut);
		}
	}

	private void WireButton(GameObject button, PerformWhileMouseDown.MouseIsDownEvent eventHandler)
	{
		if (button != null)
		{
			PerformWhileMouseDown component = button.GetComponent<PerformWhileMouseDown>();
			if (component != null)
			{
				component.WhileMouseIsDown = (PerformWhileMouseDown.MouseIsDownEvent)Delegate.Combine(component.WhileMouseIsDown, eventHandler);
			}
		}
	}

	private void UnwireButton(GameObject button, PerformWhileMouseDown.MouseIsDownEvent eventHandler)
	{
		if (button != null)
		{
			PerformWhileMouseDown component = button.GetComponent<PerformWhileMouseDown>();
			if (component != null)
			{
				component.WhileMouseIsDown = (PerformWhileMouseDown.MouseIsDownEvent)Delegate.Remove(component.WhileMouseIsDown, eventHandler);
			}
		}
	}
}
