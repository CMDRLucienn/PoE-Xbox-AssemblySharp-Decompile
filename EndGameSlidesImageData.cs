using System;
using UnityEngine;

public class EndGameSlidesImageData : ScriptableObject
{
	[Serializable]
	public class ImageData
	{
		public string DesignNote;

		[ResourcesImageProperty]
		public string Image;
	}

	public ImageData[] Data;
}
