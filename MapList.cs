using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapList : ScriptableObject, IEnumerable<MapData>, IEnumerable
{
	public MapData[] Maps;

	public MapData this[int index]
	{
		get
		{
			return Maps[index];
		}
		set
		{
			Maps[index] = value;
		}
	}

	public int Length => Maps.Length;

	public MapData GetMap(string name)
	{
		List<string> list = new List<string>();
		list.AddRange(Enum.GetNames(typeof(MapType)));
		int num = list.IndexOf(name);
		if (num >= 0)
		{
			MapType type = (MapType)Enum.GetValues(typeof(MapType)).GetValue(num);
			return GetMapData(type);
		}
		Debug.LogError("WorldMap.GetMap() - Unknown map " + name + " requested.");
		return null;
	}

	public MapData GetMapData(MapType type)
	{
		if (Maps == null || type == MapType.Map)
		{
			return null;
		}
		int num = (int)type;
		string text = type.ToString();
		if (num < Maps.Length && Maps[num].SceneName == text)
		{
			return Maps[num];
		}
		int num2 = Maps.Length;
		for (int i = 0; i < num2; i++)
		{
			if (Maps[i].SceneName == text)
			{
				return Maps[i];
			}
		}
		return null;
	}

	public IEnumerator<MapData> GetEnumerator()
	{
		MapData[] maps = Maps;
		for (int i = 0; i < maps.Length; i++)
		{
			yield return maps[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Maps.GetEnumerator();
	}
}
