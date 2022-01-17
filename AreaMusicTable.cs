using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AreaMusicTable
{
	[Serializable]
	public struct Tracks
	{
		public string NormalTrackFile;

		public string CombatTrackFile;

		public string[] ScriptedMusic;
	}

	[Serializable]
	public struct KeyTrackPairTable
	{
		public string[] keys;

		public Tracks[] tracks;
	}

	public const string FileName = "AreaMusicTable";

	public static Dictionary<string, Tracks> AreaMusicDictionary;

	public static string SaveLocation => Application.dataPath + "/data/prefabs/lists/areamusic/";

	public static string LoadLocation => Application.dataPath + "/data/";

	public static void GetMusicSetFrom(ref AreaMusic musicSet, string name)
	{
		if (!musicSet)
		{
			return;
		}
		if (AreaMusicDictionary == null)
		{
			try
			{
				AreaMusicDictionary = new Dictionary<string, Tracks>();
				KeyTrackPairTable keyTrackPairTable = JsonUtility.FromJson<KeyTrackPairTable>(File.ReadAllText(LoadLocation + "AreaMusicTable"));
				for (int i = 0; i < keyTrackPairTable.keys.Length; i++)
				{
					AreaMusicDictionary.Add(keyTrackPairTable.keys[i], keyTrackPairTable.tracks[i]);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Cannot load Area Music Table:\n" + ex.Message);
			}
		}
		Tracks tracks = AreaMusicDictionary[name];
		musicSet.NormalTrackFile = tracks.NormalTrackFile;
		musicSet.CombatTrackFile = tracks.CombatTrackFile;
		musicSet.ScriptedMusic = tracks.ScriptedMusic;
	}
}
