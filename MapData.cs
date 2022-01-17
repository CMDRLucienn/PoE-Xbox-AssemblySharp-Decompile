using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
	public enum VisibilityType
	{
		Locked,
		Unlocked,
		Hidden,
		DeveloperOnly
	}

	public enum LoadingScreenType
	{
		BurialIsle,
		CaedNua,
		Copperlane,
		Dyrford,
		Encampment,
		Generic_Cave,
		Generic_Dungeon,
		Generic_EngwithanRuins,
		Generic_Wilderness,
		GildedVale,
		OdNua,
		SunInShadow,
		TheNest,
		TwinElms,
		DefianceBay_Sewers,
		AnslogsCompass,
		CliabanRillag,
		Heritage_Hill_Tower,
		Pearlwood_Bluff_Cave,
		RaedricsHold,
		SearingFalls_Cave,
		PX1_Durgans_Battery_Ext,
		PX1_Ogre_Cave,
		PX1_Concelhaut,
		PX1_Village,
		PX1_Wilderness,
		px2_abbey,
		px2_crater,
		px2_fort,
		px2_bog,
		px2_mines
	}

	public string SceneName;

	public LoadingScreenType LoadScreenType = LoadingScreenType.Generic_Wilderness;

	public VisibilityType Visibility = VisibilityType.Unlocked;

	public bool GivesExplorationXp = true;

	[HideInInspector]
	public bool HasBeenVisited;

	public string Path;

	[Tooltip("Comma-delimited list of maps this scene should appear on.")]
	public string MapTag = "world";

	public MapsDatabaseString DisplayName;

	public AreaMusic MusicSet;

	public GroundMaterial DefaultSoundMaterial = GroundMaterial.Dirt;

	public WeatherTemperature DaytimeWeatherTemperature = WeatherTemperature.Neutral;

	public WeatherTemperature NighttimeWeatherTemperature = WeatherTemperature.Neutral;

	[NonSerialized]
	public Dictionary<string, MapDataTagSpecific> TagSpecificData = new Dictionary<string, MapDataTagSpecific>();

	private List<string> m_tags = new List<string>();

	[Tooltip("Should the player be allowed to camp on this map?")]
	public bool CanCamp;

	[Tooltip("Should the player's stash be unlocked on this map (for example, in a store).")]
	public bool CanAccessStash;

	[Tooltip("The story time difficulty will use the spawns from this difficulty (default Normal).")]
	public GameDifficulty StoryTimeSpawnSetting = GameDifficulty.Normal;

	public bool IsStronghold;

	public bool IsBeta;

	public bool StopMusicOnExit;

	public bool PlayMusicOnEnter;

	public bool IsVisibleToUser
	{
		get
		{
			if (Visibility != VisibilityType.Hidden && Visibility != VisibilityType.DeveloperOnly)
			{
				return IsAvailable;
			}
			return false;
		}
	}

	public bool IsAvailable
	{
		get
		{
			if (SceneName.StartsWith("PX1_", StringComparison.OrdinalIgnoreCase) && !GameUtilities.HasPX1())
			{
				return false;
			}
			if (SceneName.StartsWith("PX2_", StringComparison.OrdinalIgnoreCase) && !GameUtilities.HasPX2())
			{
				return false;
			}
			return true;
		}
	}

	public bool GetCanCamp()
	{
		return CanCamp;
	}

	public bool GetCanAccessStash()
	{
		if (IsStronghold && Stronghold.Instance.Activated)
		{
			return true;
		}
		return CanAccessStash;
	}

	public MapData()
	{
	}

	public MapData(string path, string sceneName)
	{
		SceneName = sceneName;
		Path = path;
	}

	public MapData(MapData other)
	{
		SceneName = other.SceneName;
		Visibility = other.Visibility;
		GivesExplorationXp = other.GivesExplorationXp;
		LoadScreenType = other.LoadScreenType;
		Path = other.Path;
		MapTag = other.MapTag;
		DisplayName = other.DisplayName;
		m_tags.AddRange(other.m_tags);
		CanCamp = other.CanCamp;
		CanAccessStash = other.CanAccessStash;
		MusicSet = other.MusicSet;
		if ((bool)MusicSet)
		{
			AreaMusicTable.GetMusicSetFrom(ref MusicSet, other.MusicSet.name);
		}
		DefaultSoundMaterial = other.DefaultSoundMaterial;
		IsStronghold = other.IsStronghold;
		IsBeta = other.IsBeta;
		StopMusicOnExit = other.StopMusicOnExit;
		PlayMusicOnEnter = other.PlayMusicOnEnter;
		DaytimeWeatherTemperature = other.DaytimeWeatherTemperature;
		NighttimeWeatherTemperature = other.NighttimeWeatherTemperature;
		StoryTimeSpawnSetting = other.StoryTimeSpawnSetting;
	}

	private void SplitTags()
	{
		string[] array = MapTag.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				array[i] = "world";
			}
			else
			{
				array[i] = array[i].ToLower().Trim();
			}
		}
		m_tags.AddRange(array);
	}

	public override string ToString()
	{
		return SceneName;
	}

	public override bool Equals(object obj)
	{
		if (obj is MapData mapData)
		{
			return mapData.Path == Path;
		}
		if (obj is string)
		{
			return Path == obj as string;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Path.GetHashCode();
	}

	public bool IsValidOnMap(string tag)
	{
		if (m_tags.Count == 0)
		{
			SplitTags();
		}
		if (string.IsNullOrEmpty(tag))
		{
			tag = "world";
		}
		return m_tags.Contains(tag.ToLower());
	}

	public int GetIconIndex(string maptag)
	{
		if (TagSpecificData.TryGetValue(maptag, out var value))
		{
			return value.IconIndex;
		}
		return -1;
	}

	public Vector2 GetMapPosition(string maptag)
	{
		if (TagSpecificData.TryGetValue(maptag, out var value))
		{
			return value.MapPosition;
		}
		return Vector2.zero;
	}

	public void SetIconIndex(string maptag, int index)
	{
		if (!TagSpecificData.ContainsKey(maptag))
		{
			TagSpecificData[maptag] = new MapDataTagSpecific();
		}
		TagSpecificData[maptag].IconIndex = index;
	}

	public void SetMapPosition(string maptag, Vector2 position)
	{
		if (!TagSpecificData.ContainsKey(maptag))
		{
			TagSpecificData[maptag] = new MapDataTagSpecific();
		}
		TagSpecificData[maptag].MapPosition = position;
	}
}
