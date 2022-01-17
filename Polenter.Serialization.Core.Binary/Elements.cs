namespace Polenter.Serialization.Core.Binary;

public static class Elements
{
	public const byte Collection = 1;

	public const byte ComplexObject = 2;

	public const byte Dictionary = 3;

	public const byte MultiArray = 4;

	public const byte Null = 5;

	public const byte SimpleObject = 6;

	public const byte SingleArray = 7;

	public const byte ComplexObjectWithId = 8;

	public const byte Reference = 9;

	public const byte CollectionWithId = 10;

	public const byte DictionaryWithId = 11;

	public const byte SingleArrayWithId = 12;

	public const byte MultiArrayWithId = 13;

	public static bool IsElementWithId(byte elementId)
	{
		return elementId switch
		{
			8 => true, 
			10 => true, 
			11 => true, 
			12 => true, 
			13 => true, 
			_ => false, 
		};
	}
}
