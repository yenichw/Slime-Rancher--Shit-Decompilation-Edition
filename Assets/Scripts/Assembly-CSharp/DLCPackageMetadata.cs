using System;
using System.Collections.Generic;
using DLCPackage;
using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "DLC/Package Metadata")]
public class DLCPackageMetadata : ScriptableObject
{
	[Serializable]
	public class Content
	{
		public string id;

		public Sprite image;

		public Sprite imageLarge;
	}

	public Id id;

	public Sprite icon;

	public List<Content> contents;
}
