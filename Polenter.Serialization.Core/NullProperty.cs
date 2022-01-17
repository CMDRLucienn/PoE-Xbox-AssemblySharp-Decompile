namespace Polenter.Serialization.Core;

public sealed class NullProperty : Property
{
	public NullProperty()
		: base(null, null)
	{
	}

	public NullProperty(string name)
		: base(name, null)
	{
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.Null;
	}
}
