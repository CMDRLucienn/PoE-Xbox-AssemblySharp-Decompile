using System;
using UnityEngine;

public class ColorList : ScriptableObject
{
	public Color[] colors;

	public Color[] sortedColors;

	public void Sort()
	{
		sortedColors = new Color[colors.Length];
		Array.Copy(colors, sortedColors, colors.Length);
		Array.Sort(sortedColors, CompareColors);
	}

	public void SetAlphas(float alpha)
	{
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i].a = alpha;
		}
	}

	public Color GetUnsortedColor(int index)
	{
		return colors[index];
	}

	public Color GetSortedColor(int index)
	{
		return sortedColors[index];
	}

	public Color GetRandomColor()
	{
		return colors[OEIRandom.Index(colors.Length)];
	}

	private int CompareColors(Color color1, Color color2)
	{
		HSBColor hSBColor = HSBColor.FromColor(color1);
		HSBColor hSBColor2 = HSBColor.FromColor(color2);
		float num = Mathf.Abs(hSBColor.h - hSBColor2.h);
		float num2 = Mathf.Abs(hSBColor.s - hSBColor2.s);
		if (num <= 0f)
		{
			if (num2 <= 0f)
			{
				if (hSBColor.b > hSBColor2.b)
				{
					return 1;
				}
				if (hSBColor.b == hSBColor2.b)
				{
					return 0;
				}
				return -1;
			}
			if (hSBColor.s > hSBColor2.s)
			{
				return 1;
			}
			return -1;
		}
		if (hSBColor.h > hSBColor2.h)
		{
			return 1;
		}
		return -1;
	}
}
