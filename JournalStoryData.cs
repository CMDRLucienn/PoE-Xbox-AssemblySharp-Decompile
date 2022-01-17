using System;
using UnityEngine;

public class JournalStoryData : ScriptableObject
{
	[Serializable]
	public class StoryItem
	{
		public string Key;

		public BackstoryDatabaseString Text;
	}

	public StoryItem[] Items;
}
