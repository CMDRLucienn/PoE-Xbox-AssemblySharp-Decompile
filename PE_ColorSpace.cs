using UnityEngine;

public static class PE_ColorSpace
{
	public static void HSVtoRGB(Vector3 colorHSV, out Color colorRGB)
	{
		float x = colorHSV.x;
		float y = colorHSV.y;
		float z = colorHSV.z;
		float r;
		float g;
		float b;
		if (y == 0f)
		{
			r = z;
			g = z;
			b = z;
		}
		else
		{
			float num = x * 6f;
			float num2 = Mathf.Floor(num);
			float num3 = z * (1f - y);
			float num4 = z * (1f - y * (num - num2));
			float num5 = z * (1f - y * (1f - (num - num2)));
			if (num2 == 0f)
			{
				r = z;
				g = num5;
				b = num3;
			}
			else if (num2 == 1f)
			{
				r = num4;
				g = z;
				b = num3;
			}
			else if (num2 == 2f)
			{
				r = num3;
				g = z;
				b = num5;
			}
			else if (num2 == 3f)
			{
				r = num3;
				g = num4;
				b = z;
			}
			else if (num2 == 4f)
			{
				r = num5;
				g = num3;
				b = z;
			}
			else
			{
				r = z;
				g = num3;
				b = num4;
			}
		}
		colorRGB.r = r;
		colorRGB.g = g;
		colorRGB.b = b;
		colorRGB.a = 1f;
	}

	public static void RGBtoHSV(Color colorRGB, out Vector3 colorHSV)
	{
		float r = colorRGB.r;
		float g = colorRGB.g;
		float b = colorRGB.b;
		float num = Mathf.Min(r, Mathf.Min(g, b));
		float num2 = Mathf.Max(r, Mathf.Max(g, b));
		float num3 = num2 - num;
		float z = num2;
		float num4;
		float y;
		if (num3 == 0f)
		{
			num4 = 0f;
			y = 0f;
		}
		else
		{
			float num5 = ((num2 - r) / 6f + num3 / 2f) / num3;
			float num6 = ((num2 - g) / 6f + num3 / 2f) / num3;
			float num7 = ((num2 - b) / 6f + num3 / 2f) / num3;
			y = num3 / num2;
			num4 = ((r == num2) ? (num7 - num6) : ((g != num2) ? (2f / 3f + num6 - num5) : (1f / 3f + num5 - num7)));
			if (num4 < 0f)
			{
				num4 += 1f;
			}
			if (num4 > 1f)
			{
				num4 -= 1f;
			}
		}
		colorHSV.x = num4;
		colorHSV.y = y;
		colorHSV.z = z;
	}
}
