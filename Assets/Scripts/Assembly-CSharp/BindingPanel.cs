using System.Collections.Generic;
using InControl;
using TMPro;
using UnityEngine;

public class BindingPanel
{
	public static GameObject CreateBindingLine(string label, PlayerAction action, GameObject bindingLineObj, MessageBundle uiBundle, Dictionary<BindingLineUI, string> labelKeyDict, BindingLineUI.DisableDelegate disableDelegate)
	{
		bindingLineObj.transform.Find("ActionText").gameObject.GetComponent<TMP_Text>().text = uiBundle.Xlate(label);
		BindingLineUI component = bindingLineObj.GetComponent<BindingLineUI>();
		component.action = action;
		if (disableDelegate != null)
		{
			component.disableDelegate = disableDelegate;
		}
		labelKeyDict[component] = label;
		return bindingLineObj;
	}
}
