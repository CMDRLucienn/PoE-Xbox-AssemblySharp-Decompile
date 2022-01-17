namespace Polenter.Serialization.Core;

public sealed class KeyValueItem
{
	public Property Key { get; set; }

	public Property Value { get; set; }

	public KeyValueItem(Property key, Property value)
	{
		Key = key;
		Value = value;
	}
}
