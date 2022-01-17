using System;

public static class ArrayExtender
{
	public static void Remove(this Array array, object element)
	{
		int i;
		for (i = 0; i < array.Length && array.GetValue(i) != element; i++)
		{
		}
		bool flag = i < array.Length;
		for (i++; i < array.Length; i++)
		{
			array.SetValue(array.GetValue(i), i - 1);
		}
		if (flag)
		{
			array.SetValue(null, array.Length - 1);
		}
	}

	public static void RemoveAt(this Array array, int index)
	{
		int num = index;
		bool flag = num < array.Length;
		for (num++; num < array.Length; num++)
		{
			array.SetValue(array.GetValue(num), num - 1);
		}
		if (flag)
		{
			array.SetValue(null, array.Length - 1);
		}
	}

	public static bool PushIfSpace(this Array array, object element)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array.GetValue(i) == null)
			{
				array.SetValue(element, i);
				return true;
			}
		}
		return false;
	}

	public static T[] Compress<T>(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = 0;
		T[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] != null)
			{
				num++;
			}
		}
		T[] array3 = new T[num];
		int num2 = 0;
		array2 = array;
		foreach (T val in array2)
		{
			if (val != null)
			{
				array3[num2] = val;
				num2++;
			}
		}
		return array3;
	}

	public static bool IsNullOrEmpty<T>(T[] array)
	{
		if (array != null)
		{
			return array.Length == 0;
		}
		return true;
	}
}
