namespace Polenter.Serialization.Core;

public sealed class ReferenceInfo
{
	public int Count { get; set; }

	public int Id { get; set; }

	public bool IsProcessed { get; set; }

	public ReferenceInfo()
	{
		Count = 1;
	}

	public override string ToString()
	{
		return $"{GetType().Name}, Count={Count}, Id={Id}, IsProcessed={IsProcessed}";
	}
}
