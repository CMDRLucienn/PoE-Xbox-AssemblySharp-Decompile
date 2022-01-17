using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class WorldMap : MonoBehaviour
{
	private static MapList s_MapList;

	private List<MapData> m_maps = new List<MapData>();

	private bool m_initialized;

	private string[] m_paths;

	public static MapList Maps
	{
		get
		{
			if (s_MapList == null)
			{
				s_MapList = Resources.Load("Data/Lists/AllMaps") as MapList;
			}
			return s_MapList;
		}
	}

	[Persistent]
	public MapData.VisibilityType[] SerializedVisibility
	{
		get
		{
			MapData.VisibilityType[] array = new MapData.VisibilityType[Maps.Maps.Length];
			for (int i = 0; i < Maps.Maps.Length; i++)
			{
				if (i < m_maps.Count)
				{
					array[i] = m_maps[i].Visibility;
				}
				else
				{
					array[i] = Maps.Maps[i].Visibility;
				}
			}
			return array;
		}
		set
		{
			InitMapsFromList();
			for (int i = 0; i < Maps.Maps.Length && i < value.Length; i++)
			{
				if (Maps.Maps[i].SceneName.StartsWith("PX2_", StringComparison.OrdinalIgnoreCase) && GlobalVariables.Instance.GetVariable("n_PX2_Critpath_Umbrella") == 0)
				{
					m_maps[i].Visibility = Maps.Maps[i].Visibility;
				}
				else
				{
					m_maps[i].Visibility = value[i];
				}
			}
		}
	}

	[Persistent]
	public bool[] SerializedVisitedStates
	{
		get
		{
			bool[] array = new bool[Maps.Maps.Length];
			for (int i = 0; i < Maps.Maps.Length; i++)
			{
				if (i < m_maps.Count)
				{
					array[i] = m_maps[i].HasBeenVisited;
				}
				else
				{
					array[i] = Maps.Maps[i].HasBeenVisited;
				}
			}
			return array;
		}
		set
		{
			InitMapsFromList();
			for (int i = 0; i < Maps.Maps.Length && i < value.Length; i++)
			{
				m_maps[i].HasBeenVisited = value[i];
			}
		}
	}

	public List<MapData> LoadedMaps => m_maps;

	public static WorldMap Instance { get; private set; }

	public string[] Paths
	{
		get
		{
			return m_paths;
		}
		set
		{
			m_paths = value;
			List<MapData> list = new List<MapData>();
			string[] paths = m_paths;
			foreach (string path in paths)
			{
				MapData mapData = GetMapFromPath(path);
				if (mapData == null)
				{
					mapData = new MapData(path, Path.GetFileNameWithoutExtension(path));
				}
				list.Add(mapData);
			}
			Maps.Maps = list.ToArray();
			m_maps.Clear();
			m_maps = list;
		}
	}

	public void InitMapsFromList()
	{
		if (!m_initialized)
		{
			MapData[] maps = Maps.Maps;
			foreach (MapData other in maps)
			{
				m_maps.Add(new MapData(other));
			}
			m_initialized = true;
			if ((bool)GameState.Instance && GameState.Instance.CurrentMap != null)
			{
				GameState.Instance.CurrentMap = GetMapFromPath(GameState.Instance.CurrentMap.Path);
			}
			if ((bool)GameState.Instance && GameState.Instance.CurrentNextMap != null)
			{
				GameState.Instance.CurrentNextMap = GetMapFromPath(GameState.Instance.CurrentNextMap.Path);
			}
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'WorldMap' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		InitMapsFromList();
		if (GameState.Instance != null)
		{
			GameState.Instance.CurrentNextMap = GetMap(SceneManager.GetActiveScene().name);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public MapData GetMapFromPath(string path)
	{
		if (m_maps == null)
		{
			return null;
		}
		for (int i = 0; i < m_maps.Count; i++)
		{
			if (m_maps[i].Equals(path))
			{
				return m_maps[i];
			}
		}
		return null;
	}

	public MapData GetMap(MapType type)
	{
		if (m_maps == null || type == MapType.Map)
		{
			return null;
		}
		int num = (int)type;
		string text = type.ToString();
		if (num < m_maps.Count && m_maps[num].SceneName == text)
		{
			return m_maps[num];
		}
		for (int i = 0; i < m_maps.Count; i++)
		{
			if (m_maps[i].SceneName == text)
			{
				return m_maps[i];
			}
		}
		Debug.LogError(string.Concat("WorldMap.GetMap() - Found no MapData for map '", type, "'."));
		return null;
	}

	public MapData GetMap(string name)
	{
		try
		{
			MapType type = (MapType)Enum.Parse(typeof(MapType), name, ignoreCase: true);
			return GetMap(type);
		}
		catch (ArgumentException exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public void SetVisibility(MapType map, MapData.VisibilityType visibility)
	{
		MapData map2 = GetMap(map);
		if (map2 != null)
		{
			map2.Visibility = visibility;
		}
	}
}
