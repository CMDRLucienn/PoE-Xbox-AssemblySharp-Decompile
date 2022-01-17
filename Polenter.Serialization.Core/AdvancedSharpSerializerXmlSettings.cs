using Polenter.Serialization.Advanced.Xml;

namespace Polenter.Serialization.Core;

public sealed class AdvancedSharpSerializerXmlSettings : AdvancedSharpSerializerSettings
{
	public ISimpleValueConverter SimpleValueConverter { get; set; }
}
