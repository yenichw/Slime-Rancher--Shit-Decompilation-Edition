using System;
using UnityEngine.UI;

public class BugReportUI : BaseUI
{
	public InputField summaryField;

	public InputField descField;

	public Button submitButton;

	private const string ERR_REQUIRE_SUMMARY = "e.require_summary";

	private const string MSG_SENDING_REPORT = "m.sending_report";

	private bool ignoreCallback;

	public override void OnDestroy()
	{
		base.OnDestroy();
		ignoreCallback = true;
	}

	public void Submit()
	{
		string text = summaryField.text;
		if (text.Length <= 0)
		{
			Error("e.require_summary");
			return;
		}
		submitButton.interactable = false;
		Status("m.sending_report");
		SentrySdk.CaptureFeedback(text, descField.text, OnBugReportComplete);
	}

	private void OnBugReportComplete(Exception exception)
	{
		if (!ignoreCallback)
		{
			if (exception != null)
			{
				submitButton.interactable = true;
				Error(exception.Message);
			}
			else
			{
				Close();
			}
		}
	}
}
