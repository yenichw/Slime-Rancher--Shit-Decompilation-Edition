using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.Xbox
{
	public class Gdk : MonoBehaviour
	{
		public delegate void OnGameSaveLoadedHandler(object sender, GameSaveLoadedArgs e);

		public delegate void OnErrorHandler(object sender, ErrorEventArgs e);

		[Header("You can find the value of the scid in your MicrosoftGame.config")]
		public string scid;

		public Text gamertagLabel;

		public bool signInOnStart = true;

		private static Gdk _xboxHelpers;

		private static bool _initialized;

		private static Dictionary<int, string> _hresultToFriendlyErrorLookup;

		private const int _100PercentAchievementProgress = 100;

		private const string _GameSaveContainerName = "x_game_save_default_container";

		private const string _GameSaveBlobName = "x_game_save_default_blob";

		private const int _MaxAssociatedProductsToRetrieve = 25;

		public static Gdk Helpers
		{
			get
			{
				if (_xboxHelpers == null)
				{
					Gdk[] array = Object.FindObjectsOfType<Gdk>();
					if (array.Length != 0)
					{
						_xboxHelpers = array[0];
						_xboxHelpers._Initialize();
					}
					else
					{
						_LogError("Error: Could not find Xbox prefab. Make sure you have added the Xbox prefab to your scene.");
					}
				}
				return _xboxHelpers;
			}
		}

		public event OnGameSaveLoadedHandler OnGameSaveLoaded;

		public event OnErrorHandler OnError;

		private void Start()
		{
			_Initialize();
		}

		private void _Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void InitializeHresultToFriendlyErrorLookup()
		{
			_hresultToFriendlyErrorLookup.Add(-2143330041, "IAP_UNEXPECTED: Does the player you are signed in as have a license for the game? You can get one by downloading your game from the store and purchasing it first. If you can't find your game in the store, have you published it in Partner Center?");
		}

		public void SignIn()
		{
		}

		public void Save(byte[] data)
		{
		}

		public void LoadSaveData()
		{
		}

		public void UnlockAchievement(string achievementId)
		{
		}

		private void Update()
		{
		}

		protected static bool Succeeded(int hresult, string operationFriendlyName)
		{
			bool result = false;
			if (HR.SUCCEEDED(hresult))
			{
				result = true;
			}
			else
			{
				string text = hresult.ToString("X8");
				string empty = string.Empty;
				empty = ((!_hresultToFriendlyErrorLookup.ContainsKey(hresult)) ? (operationFriendlyName + " failed.") : _hresultToFriendlyErrorLookup[hresult]);
				_LogError($"{empty} Error code: hr=0x{text}");
				if (Helpers.OnError != null)
				{
					Helpers.OnError(Helpers, new ErrorEventArgs(text, empty));
				}
			}
			return result;
		}

		private static void _LogError(string message)
		{
			Debug.Log(message);
		}
	}
}
