using System.Globalization;
using System.Text;
using Polenter.Serialization.Core;

namespace Polenter.Serialization;

public sealed class SharpSerializerXmlSettings : SharpSerializerSettings<AdvancedSharpSerializerXmlSettings>
{
	public CultureInfo Culture { get; set; }

	public Encoding Encoding { get; set; }

	public SharpSerializerXmlSettings()
	{
		Culture = CultureInfo.InvariantCulture;
		Encoding = Encoding.UTF8;
	}
}
