using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sentry;
using UnityEngine;
using UnityEngine.Networking;

public class SentrySdk : MonoBehaviour
{
	public delegate void OnCaptureCompleteDelegate(Exception exception);

	public static readonly string SESSION_USER_ID = Guid.NewGuid().ToString();

	private float _timeLastError;

	private const float MinTime = 10f;

	private Breadcrumb[] _breadcrumbs;

	private int _lastBreadcrumbPos;

	private int _noBreadcrumbs;

	[Header("Sentry Authentication")]
	public string Dsn;

	[Header("Debug Settings")]
	public bool showDebugMessagesInLog = true;

	[Tooltip("Enable to log exceptions from the editor.")]
	public bool postExceptionsFromEditor;

	private Dsn _dsn;

	private bool _initialized;

	private readonly HashSet<string> previouslySentErrors = new HashSet<string>();

	private static SentrySdk _instance;

	public void Start()
	{
		if (Dsn == string.Empty)
		{
			Debug.LogWarning("No DSN defined. The Sentry SDK will be disabled.");
		}
		else if (_instance == null)
		{
			try
			{
				_dsn = new Dsn(Dsn);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error parsing DSN: {ex.Message}");
				return;
			}
			_breadcrumbs = new Breadcrumb[100];
			UnityEngine.Object.DontDestroyOnLoad(this);
			_instance = this;
			_initialized = true;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public static void AddBreadcrumb(string message)
	{
		if (!(_instance == null))
		{
			_instance.DoAddBreadcrumb(message);
		}
	}

	public static void CaptureMessage(string message)
	{
		if (!(_instance == null))
		{
			_instance.DoCaptureMessage(message);
		}
	}

	public static void CaptureEvent(SentryEvent @event, OnCaptureCompleteDelegate onCaptureComplete = null)
	{
		if (!(_instance == null))
		{
			_instance.DoCaptureEvent(@event, onCaptureComplete);
		}
	}

	public static void CaptureFeedback(string summary, string description, OnCaptureCompleteDelegate onCaptureComplete = null)
	{
		if (!(_instance == null))
		{
			_instance.DoCaptureFeedback(summary, description, onCaptureComplete);
		}
	}

	private void DoCaptureFeedback(string summary, string description, OnCaptureCompleteDelegate onCaptureComplete = null)
	{
		SentryEvent @event = new SentryEvent(summary + "\n\n" + description, GetBreadcrumbs())
		{
			tags = 
			{
				isUserFeedback = true
			},
			level = "info"
		};
		StartCoroutine(ContinueSendingEvent(@event, onCaptureComplete));
	}

	private void DoCaptureMessage(string message, OnCaptureCompleteDelegate onCaptureComplete = null)
	{
		if (showDebugMessagesInLog)
		{
			Debug.Log("sending message to sentry.");
		}
		SentryEvent @event = new SentryEvent(message, GetBreadcrumbs())
		{
			level = "info"
		};
		DoCaptureEvent(@event, onCaptureComplete);
	}

	private void DoCaptureEvent(SentryEvent @event, OnCaptureCompleteDelegate onCaptureComplete)
	{
		if (showDebugMessagesInLog)
		{
			Debug.Log("sending event to sentry.");
		}
		StartCoroutine(ContinueSendingEvent(@event, onCaptureComplete));
	}

	private void DoAddBreadcrumb(string message)
	{
		if (!_initialized)
		{
			Debug.LogError("Cannot AddBreadcrumb if we are not initialized");
			return;
		}
		string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
		_breadcrumbs[_lastBreadcrumbPos] = new Breadcrumb(timestamp, message);
		_lastBreadcrumbPos++;
		_lastBreadcrumbPos %= 100;
		if (_noBreadcrumbs < 100)
		{
			_noBreadcrumbs++;
		}
	}

	private List<Breadcrumb> GetBreadcrumbs()
	{
		return Breadcrumb.CombineBreadcrumbs(_breadcrumbs, _lastBreadcrumbPos, _noBreadcrumbs);
	}

	public void OnEnable()
	{
		Application.logMessageReceived += OnLogMessageReceived;
	}

	public void OnDisable()
	{
		Application.logMessageReceived -= OnLogMessageReceived;
	}

	public void ScheduleException(string condition, string stackTrace)
	{
		if (showDebugMessagesInLog)
		{
			Debug.Log("sending exception to sentry.");
		}
		List<StackTraceSpec> list = new List<StackTraceSpec>();
		string[] array = condition.Split(new char[1] { ':' }, 2);
		string exceptionType = array[0];
		string exceptionValue = ((array.Length > 1) ? array[1].Trim() : array[0]);
		foreach (StackTraceSpec stackTrace2 in GetStackTraces(stackTrace))
		{
			list.Add(stackTrace2);
		}
		SentryExceptionEvent @event = new SentryExceptionEvent(exceptionType, exceptionValue, GetBreadcrumbs(), list);
		StartCoroutine(ContinueSendingEvent(@event));
	}

	private static IEnumerable<StackTraceSpec> GetStackTraces(string stackTrace)
	{
		string[] stackList = stackTrace.Split('\n');
		for (int i = stackList.Length - 1; i >= 0; i--)
		{
			string text = stackList[i];
			if (text == string.Empty)
			{
				continue;
			}
			int num = text.IndexOf(')');
			if (num == -1)
			{
				continue;
			}
			string text2;
			string text3;
			int lineNo;
			try
			{
				text2 = text.Substring(0, num + 1);
				if (text.Length < num + 6)
				{
					text3 = string.Empty;
					lineNo = -1;
				}
				else if (text.Substring(num + 1, 5) != " (at ")
				{
					Debug.Log("failed parsing " + text);
					text2 = text;
					lineNo = -1;
					text3 = string.Empty;
				}
				else
				{
					int num2 = text.LastIndexOf(':', text.Length - 1, text.Length - num);
					if (num == text.Length - 1)
					{
						text3 = string.Empty;
						lineNo = -1;
					}
					else if (num2 == -1)
					{
						text3 = text.Substring(num + 6, text.Length - num - 7);
						lineNo = -1;
					}
					else
					{
						text3 = text.Substring(num + 6, num2 - num - 6);
						lineNo = Convert.ToInt32(text.Substring(num2 + 1, text.Length - 2 - num2));
					}
				}
			}
			catch
			{
				continue;
			}
			bool inApp;
			if (text3 == string.Empty || (text3[0] == '<' && text3[text3.Length - 1] == '>'))
			{
				text3 = string.Empty;
				inApp = true;
				if (text2.Contains("UnityEngine."))
				{
					inApp = false;
				}
			}
			else
			{
				inApp = text3.Contains("Assets/");
			}
			yield return new StackTraceSpec(text3, text2, lineNo, inApp);
		}
	}

	public void OnLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (_initialized && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && !previouslySentErrors.Contains(condition) && !(Time.time - _timeLastError <= 10f))
		{
			_timeLastError = Time.time;
			previouslySentErrors.Add(condition);
			ScheduleException(condition, stackTrace);
		}
	}

	private IEnumerator ContinueSendingEvent<T>(T @event, OnCaptureCompleteDelegate onCaptureComplete = null) where T : SentryEvent
	{
		yield return new WaitForSecondsRealtime(5f);
		string s = JsonUtility.ToJson(@event);
		string publicKey = _dsn.publicKey;
		string secretKey = _dsn.secretKey;
		string text = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
		string value = "Sentry sentry_version=5,sentry_client=Unity0.1,sentry_timestamp=" + text + ",sentry_key=" + publicKey + ",sentry_secret=" + secretKey;
		UnityWebRequest www = new UnityWebRequest(_dsn.callUri.ToString());
		www.method = "POST";
		www.SetRequestHeader("X-Sentry-Auth", value);
		www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
		www.downloadHandler = new DownloadHandlerBuffer();
		yield return www.SendWebRequest();
		while (!www.isDone)
		{
			yield return null;
		}
		if (www.isNetworkError || www.isHttpError || www.responseCode != 200)
		{
			Debug.LogWarning("error sending request to sentry: " + www.error);
			onCaptureComplete?.Invoke(new Exception(www.error));
		}
		else if (showDebugMessagesInLog)
		{
			Debug.Log("Sentry sent back: " + www.downloadHandler.text);
			onCaptureComplete?.Invoke(null);
		}
	}
}
