using System;
using System.Collections.Generic;
using UnityEngine;

public static class IListExtender
{
	public static void AddRange<T>(this IList<T> list, IEnumerable<T> other)
	{
		foreach (T item in other)
		{
			list.Add(item);
		}
	}

	public static IList<T> Shuffle<T>(this IList<T> list)
	{
		List<T> list2 = new List<T>(list);
		int num = list2.Count;
		while (num > 1)
		{
			num--;
			int index = UnityEngine.Random.Range(0, num + 1);
			T value = list2[index];
			list2[index] = list2[num];
			list2[num] = value;
		}
		return list2;
	}

	public static void StableSort<T>(this IList<T> list, Comparison<T> comparison)
	{
		for (int i = 1; i < list.Count; i++)
		{
			int num = i - 1;
			while (num >= 0 && comparison(list[num], list[num + 1]) > 0)
			{
				T value = list[num];
				list[num] = list[num + 1];
				list[num + 1] = value;
				num--;
			}
		}
	}
}
