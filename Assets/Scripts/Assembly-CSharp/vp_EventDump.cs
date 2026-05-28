using System;
using System.Collections.Generic;
using System.Reflection;

public class vp_EventDump
{
	public static string Dump(vp_EventHandler handler, string[] eventTypes)
	{
		string text = "";
		for (int i = 0; i < eventTypes.Length; i++)
		{
			switch (eventTypes[i])
			{
			case "vp_Message":
				text += DumpEventsOfType("vp_Message", (eventTypes.Length > 1) ? "MESSAGES:\n\n" : "", handler);
				break;
			case "vp_Attempt":
				text += DumpEventsOfType("vp_Attempt", (eventTypes.Length > 1) ? "ATTEMPTS:\n\n" : "", handler);
				break;
			case "vp_Value":
				text += DumpEventsOfType("vp_Value", (eventTypes.Length > 1) ? "VALUES:\n\n" : "", handler);
				break;
			case "vp_Activity":
				text += DumpEventsOfType("vp_Activity", (eventTypes.Length > 1) ? "ACTIVITIES:\n\n" : "", handler);
				break;
			}
		}
		return text;
	}

	private static string DumpEventsOfType(string type, string caption, vp_EventHandler handler)
	{
		return "Dumping Disabled";
	}

	private static string DumpEventListeners(object e, string[] invokers)
	{
		Type type = e.GetType();
		string text = "";
		foreach (string text2 in invokers)
		{
			FieldInfo field = type.GetField(text2);
			if (field == null)
			{
				return "";
			}
			Delegate @delegate = (Delegate)field.GetValue(e);
			string[] array = null;
			if ((object)@delegate != null)
			{
				array = GetMethodNames(@delegate.GetInvocationList());
			}
			text += "\t\t\t\t";
			if (type.ToString().Contains("vp_Value"))
			{
				text = ((text2 == "Get") ? (text + "Get") : ((!(text2 == "Set")) ? (text + "Unsupported listener: ") : (text + "Set")));
			}
			else if (type.ToString().Contains("vp_Attempt"))
			{
				text += "Try";
			}
			else if (type.ToString().Contains("vp_Message"))
			{
				text += "Send";
			}
			else if (type.ToString().Contains("vp_Activity"))
			{
				switch (text2)
				{
				case "StartConditions":
					text += "TryStart";
					break;
				case "StopConditions":
					text += "TryStop";
					break;
				case "StartCallbacks":
					text += "Start";
					break;
				case "StopCallbacks":
					text += "Stop";
					break;
				case "FailStartCallbacks":
					text += "FailStart";
					break;
				case "FailStopCallbacks":
					text += "FailStop";
					break;
				default:
					text += "Unsupported listener: ";
					break;
				}
			}
			else
			{
				text += "Unsupported listener";
			}
			if (array != null)
			{
				text = ((array.Length <= 2) ? (text + ": ") : (text + ":\n"));
				text += DumpDelegateNames(array);
			}
		}
		return text;
	}

	private static string[] GetMethodNames(Delegate[] list)
	{
		list = RemoveDelegatesFromList(list);
		string[] array = new string[list.Length];
		if (list.Length == 1)
		{
			array[0] = ((list[0].Target == null) ? "" : string.Concat("(", list[0].Target, ") ")) + list[0].Method.Name;
		}
		else
		{
			for (int i = 1; i < list.Length; i++)
			{
				array[i] = ((list[i].Target == null) ? "" : string.Concat("(", list[i].Target, ") ")) + list[i].Method.Name;
			}
		}
		return array;
	}

	private static Delegate[] RemoveDelegatesFromList(Delegate[] list)
	{
		List<Delegate> list2 = new List<Delegate>(list);
		for (int num = list2.Count - 1; num > -1; num--)
		{
			if ((object)list2[num] != null && list2[num].Method.Name.Contains("m_"))
			{
				list2.RemoveAt(num);
			}
		}
		return list2.ToArray();
	}

	private static string DumpDelegateNames(string[] array)
	{
		string text = "";
		foreach (string text2 in array)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + ((array.Length > 2) ? "\t\t\t\t\t\t\t" : "") + text2 + "\n";
			}
		}
		return text;
	}
}
