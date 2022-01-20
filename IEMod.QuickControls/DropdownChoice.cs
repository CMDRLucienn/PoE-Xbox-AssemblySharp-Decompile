// IEMod.QuickControls.DropdownChoice
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.DropdownChoice")]
public static class DropdownChoice
{
	[PatchedByMember("IEMod.QuickControls.DropdownChoice`1<T>[] IEMod.QuickControls.DropdownChoice::FromEnum()")]
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
