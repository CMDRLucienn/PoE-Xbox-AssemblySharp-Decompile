using System.Collections.Generic;

public static class IEnumerableExtender
{
	public static bool AnyX<T>(this IEnumerable<T> enumerable, int target)
	{
		int num = 0;
		using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				num++;
				if (num >= target)
				{
					return true;
				}
			}
		}
		return false;
	}
}
