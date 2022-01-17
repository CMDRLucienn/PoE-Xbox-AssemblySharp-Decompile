namespace Polenter.Serialization.Core;

public sealed class MultiDimensionalArrayItem
{
	public int[] Indexes { get; set; }

	public Property Value { get; set; }

	public MultiDimensionalArrayItem(int[] indexes, Property value)
	{
		Indexes = indexes;
		Value = value;
	}
}
