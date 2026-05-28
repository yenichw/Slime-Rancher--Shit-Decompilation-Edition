using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Metadata/RancherChatMetadata", fileName = "RancherChatMetadata")]
public class RancherChatMetadata : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public enum RancherName
		{
			UNKNOWN = 0,
			THORA = 1,
			VIKTOR = 2,
			OGDEN = 3,
			MOCHI = 4,
			BOB = 5
		}

		[Tooltip("Rancher name.")]
		public RancherName rancherName;

		[Tooltip("Rancher image.")]
		public Sprite rancherImage;

		[Tooltip("Message background material.")]
		public Material messageBackground;

		[Tooltip("Message string. (optional")]
		public string messageText;

		[Tooltip("Message prefab; overrides 'messageText'. (optional)")]
		public GameObject messagePrefab;
	}

	[Tooltip("List of rancher chat entries.")]
	public Entry[] entries;
}
