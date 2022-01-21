using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

public class DropdownChoice<T>
{
	public readonly T Value;

	public readonly string Label;

	public DropdownChoice(string label, T value)
	{
		Label = label;
		Value = value;
	}

	public override string ToString()
	{
		return Label;
	}
}

public static class DropdownChoice
{
	public static DropdownChoice<T>[] FromEnum<T>()
	{
		List<DropdownChoice<T>> list = new List<DropdownChoice<T>>();
		Type typeFromHandle = typeof(T);
		foreach (object value in Enum.GetValues(typeFromHandle))
		{
			FieldInfo field = typeFromHandle.GetField(value.ToString());
			string text = ReflectHelper.GetCustomAttribute<DescriptionAttribute>(field)?.Description;
			text = text ?? value.ToString();
			list.Add(new DropdownChoice<T>(text, (T)value));
		}
		return list.ToArray();
	}
}