using System;

public static class OEIRandom
{
	private static Random s_Random = new Random();

	public static float FloatValue()
	{
		return (float)s_Random.NextDouble();
	}

	public static float FloatValueInclusive()
	{
		return (float)s_Random.Next() / 2.14748365E+09f;
	}

	public static double DoubleValue()
	{
		return s_Random.NextDouble();
	}

	public static double DoubleValueInclusive()
	{
		return (double)s_Random.Next() / 2147483646.0;
	}

	public static double Range(double min, double max)
	{
		return DoubleValue() * (max - min) + min;
	}

	public static double RangeInclusive(double min, double max)
	{
		return DoubleValueInclusive() * (max - min) + min;
	}

	public static float Range(float min, float max)
	{
		return FloatValue() * (max - min) + min;
	}

	public static float RangeInclusive(float min, float max)
	{
		return FloatValueInclusive() * (max - min) + min;
	}

	public static int Range(int min, int max)
	{
		return s_Random.Next(min, max + 1);
	}

	public static int Index(int count)
	{
		return s_Random.Next(count);
	}

	public static int NonNegativeInt()
	{
		return s_Random.Next(0, int.MaxValue);
	}

	public static int DieRoll(int sides)
	{
		return Range(1, sides);
	}

	public static float AngleDegrees()
	{
		return Range(0f, 360f);
	}

	public static float AngleRadians()
	{
		return Range(0f, (float)Math.PI * 2f);
	}
}
