using System;

[Serializable]
public class Gaussian
{
	private static Random s_Random = new Random();

	public double Mean;

	public double StdDev;

	public Gaussian(double mean, double stddev)
	{
		Mean = mean;
		StdDev = stddev;
	}

	public double RandomSample()
	{
		double d = s_Random.NextDouble();
		double num = s_Random.NextDouble();
		double num2 = Math.Sqrt(-2.0 * Math.Log(d)) * Math.Sin(Math.PI * 2.0 * num);
		return Mean + StdDev * num2;
	}
}
