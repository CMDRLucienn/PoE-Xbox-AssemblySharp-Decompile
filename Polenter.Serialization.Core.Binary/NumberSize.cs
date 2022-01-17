namespace Polenter.Serialization.Core.Binary;

public static class NumberSize
{
	public const byte Zero = 0;

	public const byte B1 = 1;

	public const byte B2 = 2;

	public const byte B4 = 4;

	public static byte GetNumberSize(int value)
	{
		if (value == 0)
		{
			return 0;
		}
		if (value > 32767 || value < -32768)
		{
			return 4;
		}
		if (value < 0 || value > 255)
		{
			return 2;
		}
		return 1;
	}
}
