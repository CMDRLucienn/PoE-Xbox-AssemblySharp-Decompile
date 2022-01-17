using System;
using UnityEngine;

public static class ColorListManager
{
	public enum ColorPickerType
	{
		None,
		Skin,
		Hair,
		Major,
		Minor,
		COUNT
	}

	public static ColorList MajorColorList;

	public static ColorList[] HairColorLists;

	public static ColorList[] SkinColorLists;

	private static string[] s_Subraces;

	static ColorListManager()
	{
		s_Subraces = Enum.GetNames(typeof(CharacterStats.Subrace));
		for (int i = 0; i < s_Subraces.Length; i++)
		{
			s_Subraces[i] = s_Subraces[i].Replace("_", "");
		}
	}

	public static ColorList GetColorList(ColorPickerType ptype, CharacterStats.Subrace Subrace)
	{
		Load();
		switch (ptype)
		{
		case ColorPickerType.Hair:
			if ((int)Subrace < HairColorLists.Length && HairColorLists[(int)Subrace] != null && HairColorLists[(int)Subrace].colors.Length != 0)
			{
				return HairColorLists[(int)Subrace];
			}
			return null;
		case ColorPickerType.Skin:
			if ((int)Subrace < SkinColorLists.Length && SkinColorLists[(int)Subrace] != null && SkinColorLists[(int)Subrace].colors.Length != 0)
			{
				return SkinColorLists[(int)Subrace];
			}
			return null;
		case ColorPickerType.Major:
		case ColorPickerType.Minor:
			return MajorColorList;
		default:
			return null;
		}
	}

	private static void Load()
	{
		if (MajorColorList == null)
		{
			MajorColorList = Resources.Load("Data/Lists/Color/MajorColorList") as ColorList;
			if ((bool)MajorColorList)
			{
				MajorColorList.SetAlphas(1f);
				MajorColorList.Sort();
			}
		}
		if (HairColorLists == null)
		{
			HairColorLists = new ColorList[s_Subraces.Length];
			for (int i = 1; i < HairColorLists.Length; i++)
			{
				HairColorLists[i] = Resources.Load("Data/Lists/Color/" + s_Subraces[i] + "HairColorList") as ColorList;
				if ((bool)HairColorLists[i])
				{
					HairColorLists[i].SetAlphas(1f);
					HairColorLists[i].Sort();
				}
			}
		}
		if (SkinColorLists != null)
		{
			return;
		}
		SkinColorLists = new ColorList[s_Subraces.Length];
		for (int j = 1; j < SkinColorLists.Length; j++)
		{
			SkinColorLists[j] = Resources.Load("Data/Lists/Color/" + s_Subraces[j] + "SkinColorList") as ColorList;
			if ((bool)SkinColorLists[j])
			{
				SkinColorLists[j].SetAlphas(1f);
				SkinColorLists[j].Sort();
			}
		}
	}

	public static void Unload()
	{
		if (MajorColorList != null)
		{
			Resources.UnloadAsset(MajorColorList);
			MajorColorList = null;
		}
		if (HairColorLists != null)
		{
			for (int i = 0; i < HairColorLists.Length; i++)
			{
				Resources.UnloadAsset(HairColorLists[i]);
			}
			HairColorLists = null;
		}
		if (SkinColorLists != null)
		{
			for (int j = 0; j < SkinColorLists.Length; j++)
			{
				Resources.UnloadAsset(SkinColorLists[j]);
			}
			SkinColorLists = null;
		}
	}
}
