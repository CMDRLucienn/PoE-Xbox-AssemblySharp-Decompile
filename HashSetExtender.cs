using System.Collections.Generic;

public static class HashSetExtender
{
	public static void AddRange<T>(this HashSet<T> set, IList<T> other)
	{
		for (int num = other.Count - 1; num >= 0; num--)
		{
			set.Add(other[num]);
		}
	}

	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> other)
	{
		foreach (T item in other)
		{
			set.Add(item);
		}
	}
}
