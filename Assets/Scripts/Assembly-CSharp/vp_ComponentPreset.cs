using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;

public sealed class vp_ComponentPreset
{
	private enum ReadMode
	{
		Normal = 0,
		LineComment = 1,
		BlockComment = 2
	}

	private class Field
	{
		public RuntimeFieldHandle FieldHandle;

		public object Args;

		public Field(RuntimeFieldHandle fieldHandle, object args)
		{
			FieldHandle = fieldHandle;
			Args = args;
		}
	}

	private static string m_FullPath = null;

	private static int m_LineNumber = 0;

	public static bool LogErrors = true;

	private static ReadMode m_ReadMode = ReadMode.Normal;

	private Type m_ComponentType;

	private List<Field> m_Fields = new List<Field>();

	private Dictionary<string, string[]> MovedParameters = new Dictionary<string, string[]>
	{
		{
			"vp_FPCamera.MouseAcceleration",
			new string[2] { "vp_FPInput", "MouseLookAcceleration" }
		},
		{
			"vp_FPCamera.MouseSensitivity",
			new string[2] { "vp_FPInput", "MouseLookSensitivity" }
		},
		{
			"vp_FPCamera.MouseSmoothSteps",
			new string[2] { "vp_FPInput", "MouseLookSmoothSteps" }
		},
		{
			"vp_FPCamera.MouseSmoothWeight",
			new string[2] { "vp_FPInput", "MouseLookSmoothWeight" }
		},
		{
			"vp_FPCamera.MouseAccelerationThreshold",
			new string[2] { "vp_FPInput", "MouseLookAccelerationThreshold" }
		},
		{
			"vp_FPInput.ForceCursor",
			new string[2] { "vp_FPInput", "MouseCursorForced" }
		}
	};

	public Type ComponentType
	{
		get
		{
			return m_ComponentType;
		}
		set
		{
			m_ComponentType = value;
		}
	}

	public static string Save(Component component, string fullPath)
	{
		vp_ComponentPreset obj = new vp_ComponentPreset();
		obj.InitFromComponent(component);
		return Save(obj, fullPath);
	}

	public static string Save(vp_ComponentPreset savePreset, string fullPath, bool isDifference = false)
	{
		m_FullPath = fullPath;
		bool logErrors = LogErrors;
		LogErrors = false;
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadTextStream(m_FullPath);
		LogErrors = logErrors;
		if (vp_ComponentPreset2 != null)
		{
			if (vp_ComponentPreset2.m_ComponentType != null)
			{
				if (vp_ComponentPreset2.ComponentType != savePreset.ComponentType)
				{
					return "'" + ExtractFilenameFromPath(m_FullPath) + "' has the WRONG component type: " + vp_ComponentPreset2.ComponentType.ToString() + ".\n\nDo you want to replace it with a " + savePreset.ComponentType.ToString() + "?";
				}
				if (File.Exists(m_FullPath))
				{
					if (isDifference)
					{
						return "This will update '" + ExtractFilenameFromPath(m_FullPath) + "' with only the values modified since pressing Play or setting a state.\n\nContinue?";
					}
					return "'" + ExtractFilenameFromPath(m_FullPath) + "' already exists.\n\nDo you want to replace it?";
				}
			}
			if (File.Exists(m_FullPath))
			{
				return "'" + ExtractFilenameFromPath(m_FullPath) + "' has an UNKNOWN component type.\n\nDo you want to replace it?";
			}
		}
		ClearTextFile();
		Append("///////////////////////////////////////////////////////////");
		Append("// Component Preset Script");
		Append("///////////////////////////////////////////////////////////\n");
		Append("ComponentType " + savePreset.ComponentType.Name);
		foreach (Field field in savePreset.m_Fields)
		{
			string text = "";
			string text2 = "";
			FieldInfo fieldFromHandle = FieldInfo.GetFieldFromHandle(field.FieldHandle);
			if (fieldFromHandle.FieldType == typeof(float))
			{
				text2 = $"{(float)field.Args:0.#######}";
			}
			else if (fieldFromHandle.FieldType == typeof(Vector4))
			{
				Vector4 vector = (Vector4)field.Args;
				text2 = $"{vector.x:0.#######}" + " " + $"{vector.y:0.#######}" + " " + $"{vector.z:0.#######}" + " " + $"{vector.w:0.#######}";
			}
			else if (fieldFromHandle.FieldType == typeof(Vector3))
			{
				Vector3 vector2 = (Vector3)field.Args;
				text2 = $"{vector2.x:0.#######}" + " " + $"{vector2.y:0.#######}" + " " + $"{vector2.z:0.#######}";
			}
			else if (fieldFromHandle.FieldType == typeof(Vector2))
			{
				Vector2 vector3 = (Vector2)field.Args;
				text2 = $"{vector3.x:0.#######}" + " " + $"{vector3.y:0.#######}";
			}
			else if (fieldFromHandle.FieldType == typeof(int))
			{
				text2 = ((int)field.Args).ToString();
			}
			else if (fieldFromHandle.FieldType == typeof(bool))
			{
				text2 = ((bool)field.Args).ToString();
			}
			else if (fieldFromHandle.FieldType == typeof(string))
			{
				text2 = (string)field.Args;
			}
			else
			{
				text = "//";
				text2 = "<NOTE: Type '" + fieldFromHandle.FieldType.Name.ToString() + "' can't be saved to preset.>";
			}
			if (!string.IsNullOrEmpty(text2) && fieldFromHandle.Name != "Persist")
			{
				Append(text + fieldFromHandle.Name + " " + text2);
			}
		}
		return null;
	}

	public static string SaveDifference(vp_ComponentPreset initialStatePreset, Component modifiedComponent, string fullPath, vp_ComponentPreset diskPreset)
	{
		if (initialStatePreset.ComponentType != modifiedComponent.GetType())
		{
			Error("Tried to save difference between different type components in 'SaveDifference'");
			return null;
		}
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.InitFromComponent(modifiedComponent);
		vp_ComponentPreset vp_ComponentPreset3 = new vp_ComponentPreset();
		vp_ComponentPreset3.m_ComponentType = vp_ComponentPreset2.ComponentType;
		for (int i = 0; i < vp_ComponentPreset2.m_Fields.Count; i++)
		{
			if (!initialStatePreset.m_Fields[i].Args.Equals(vp_ComponentPreset2.m_Fields[i].Args))
			{
				vp_ComponentPreset3.m_Fields.Add(vp_ComponentPreset2.m_Fields[i]);
			}
		}
		foreach (Field field in diskPreset.m_Fields)
		{
			bool flag = true;
			foreach (Field field2 in vp_ComponentPreset3.m_Fields)
			{
				if (field.FieldHandle == field2.FieldHandle)
				{
					flag = false;
				}
			}
			bool flag2 = false;
			foreach (Field field3 in vp_ComponentPreset2.m_Fields)
			{
				if (field.FieldHandle == field3.FieldHandle)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				flag = false;
			}
			if (flag)
			{
				vp_ComponentPreset3.m_Fields.Add(field);
			}
		}
		return Save(vp_ComponentPreset3, fullPath, isDifference: true);
	}

	public void InitFromComponent(Component component)
	{
	}

	public static vp_ComponentPreset CreateFromComponent(Component component)
	{
		return null;
	}

	public int TryMakeCompatibleWithComponent(vp_Component component)
	{
		return 0;
	}

	public bool LoadTextStream(string fullPath)
	{
		return true;
	}

	public static bool Load(vp_Component component, string fullPath)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadTextStream(fullPath);
		return Apply(component, vp_ComponentPreset2);
	}

	public bool LoadFromResources(string resourcePath)
	{
		m_FullPath = resourcePath;
		TextAsset textAsset = Resources.Load(m_FullPath) as TextAsset;
		if (textAsset == null)
		{
			Error("Failed to read file. '" + m_FullPath + "'");
			return false;
		}
		return LoadFromTextAsset(textAsset);
	}

	public static vp_ComponentPreset LoadFromResources(vp_Component component, string resourcePath)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadFromResources(resourcePath);
		Apply(component, vp_ComponentPreset2);
		return vp_ComponentPreset2;
	}

	public bool LoadFromTextAsset(TextAsset file)
	{
		m_FullPath = file.name;
		List<string> list = new List<string>();
		string[] array = file.text.Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		if (list == null)
		{
			Error("Preset is empty. '" + m_FullPath + "'");
			return false;
		}
		ParseLines(list);
		return true;
	}

	public static vp_ComponentPreset LoadFromTextAsset(vp_Component component, TextAsset file)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadFromTextAsset(file);
		Apply(component, vp_ComponentPreset2);
		return vp_ComponentPreset2;
	}

	private static void Append(string str)
	{
	}

	private static void ClearTextFile()
	{
	}

	private void ParseLines(List<string> lines)
	{
		m_LineNumber = 0;
		foreach (string line in lines)
		{
			m_LineNumber++;
			string text = RemoveComments(line);
			if (!string.IsNullOrEmpty(text) && !Parse(text))
			{
				return;
			}
		}
		m_LineNumber = 0;
	}

	private bool Parse(string line)
	{
		return true;
	}

	private string[] FindMovedParameter(string type, string field)
	{
		if (!MovedParameters.TryGetValue(type + "." + field, out var value))
		{
			return null;
		}
		return value;
	}

	public static bool Apply(vp_Component component, vp_ComponentPreset preset)
	{
		return true;
	}

	public static Type GetFileType(string fullPath)
	{
		bool logErrors = LogErrors;
		LogErrors = false;
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadTextStream(fullPath);
		LogErrors = logErrors;
		if (vp_ComponentPreset2 != null && vp_ComponentPreset2.m_ComponentType != null)
		{
			return vp_ComponentPreset2.m_ComponentType;
		}
		return null;
	}

	public static Type GetFileTypeFromAsset(TextAsset asset)
	{
		bool logErrors = LogErrors;
		LogErrors = false;
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadFromTextAsset(asset);
		LogErrors = logErrors;
		if (vp_ComponentPreset2 != null && vp_ComponentPreset2.m_ComponentType != null)
		{
			return vp_ComponentPreset2.m_ComponentType;
		}
		return null;
	}

	private static object TokensToObject(FieldInfo field, string[] tokens)
	{
		if (field.FieldType == typeof(float))
		{
			return ArgsToFloat(tokens);
		}
		if (field.FieldType == typeof(Vector4))
		{
			return ArgsToVector4(tokens);
		}
		if (field.FieldType == typeof(Vector3))
		{
			return ArgsToVector3(tokens);
		}
		if (field.FieldType == typeof(Vector2))
		{
			return ArgsToVector2(tokens);
		}
		if (field.FieldType == typeof(int))
		{
			return ArgsToInt(tokens);
		}
		if (field.FieldType == typeof(bool))
		{
			return ArgsToBool(tokens);
		}
		if (field.FieldType == typeof(string))
		{
			return ArgsToString(tokens);
		}
		return null;
	}

	private static string RemoveComments(string str)
	{
		string text = "";
		for (int i = 0; i < str.Length; i++)
		{
			switch (m_ReadMode)
			{
			case ReadMode.Normal:
				if (str[i] == '/' && str[i + 1] == '*')
				{
					m_ReadMode = ReadMode.BlockComment;
					i++;
				}
				else if (str[i] == '/' && str[i + 1] == '/')
				{
					m_ReadMode = ReadMode.LineComment;
					i++;
				}
				else
				{
					text += str[i];
				}
				break;
			case ReadMode.LineComment:
				if (i == str.Length - 1)
				{
					m_ReadMode = ReadMode.Normal;
				}
				break;
			case ReadMode.BlockComment:
				if (str[i] == '*' && str[i + 1] == '/')
				{
					m_ReadMode = ReadMode.Normal;
					i++;
				}
				break;
			}
		}
		return text;
	}

	private static Vector4 ArgsToVector4(string[] args)
	{
		if (args.Length - 1 != 4)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector4.zero;
		}
		try
		{
			return new Vector4(Convert.ToSingle(args[1], CultureInfo.InvariantCulture), Convert.ToSingle(args[2], CultureInfo.InvariantCulture), Convert.ToSingle(args[3], CultureInfo.InvariantCulture), Convert.ToSingle(args[4], CultureInfo.InvariantCulture));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + ", " + args[4] + "'");
			return Vector4.zero;
		}
	}

	private static Vector3 ArgsToVector3(string[] args)
	{
		if (args.Length - 1 != 3)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector3.zero;
		}
		try
		{
			return new Vector3(Convert.ToSingle(args[1], CultureInfo.InvariantCulture), Convert.ToSingle(args[2], CultureInfo.InvariantCulture), Convert.ToSingle(args[3], CultureInfo.InvariantCulture));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + "'");
			return Vector3.zero;
		}
	}

	private static Vector2 ArgsToVector2(string[] args)
	{
		if (args.Length - 1 != 2)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector2.zero;
		}
		try
		{
			return new Vector2(Convert.ToSingle(args[1], CultureInfo.InvariantCulture), Convert.ToSingle(args[2], CultureInfo.InvariantCulture));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + "'");
			return Vector2.zero;
		}
	}

	private static float ArgsToFloat(string[] args)
	{
		if (args.Length - 1 != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0f;
		}
		try
		{
			return Convert.ToSingle(args[1], CultureInfo.InvariantCulture);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0f;
		}
	}

	private static int ArgsToInt(string[] args)
	{
		if (args.Length - 1 != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0;
		}
		try
		{
			return Convert.ToInt32(args[1], CultureInfo.InvariantCulture);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0;
		}
	}

	private static bool ArgsToBool(string[] args)
	{
		if (args.Length - 1 != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return false;
		}
		if (args[1].ToLower() == "true")
		{
			return true;
		}
		if (args[1].ToLower() == "false")
		{
			return false;
		}
		PresetError("Illegal value: '" + args[1] + "'");
		return false;
	}

	private static string ArgsToString(string[] args)
	{
		string text = "";
		for (int i = 1; i < args.Length; i++)
		{
			text += args[i];
			if (i < args.Length - 1)
			{
				text += " ";
			}
		}
		return text;
	}

	public Type GetFieldType(string fieldName)
	{
		Type result = null;
		foreach (Field field in m_Fields)
		{
			FieldInfo fieldFromHandle = FieldInfo.GetFieldFromHandle(field.FieldHandle);
			if (fieldFromHandle.Name == fieldName)
			{
				result = fieldFromHandle.FieldType;
			}
		}
		return result;
	}

	public object GetFieldValue(string fieldName)
	{
		object result = null;
		foreach (Field field in m_Fields)
		{
			if (FieldInfo.GetFieldFromHandle(field.FieldHandle).Name == fieldName)
			{
				result = field.Args;
			}
		}
		return result;
	}

	public static string ExtractFilenameFromPath(string path)
	{
		int num = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		if (num == -1)
		{
			return path;
		}
		if (num == path.Length - 1)
		{
			return "";
		}
		return path.Substring(num + 1, path.Length - num - 1);
	}

	private static void PresetError(string message)
	{
		if (LogErrors)
		{
			Debug.LogError("Preset Error: " + m_FullPath + " (at " + m_LineNumber + ") " + message);
		}
	}

	private static void PresetWarning(string message)
	{
		if (LogErrors)
		{
			Debug.LogWarning("Preset Warning: " + m_FullPath + " (at " + m_LineNumber + ") " + message);
		}
	}

	private static void Error(string message)
	{
		if (LogErrors)
		{
			Debug.LogError("Error: " + message);
		}
	}
}
