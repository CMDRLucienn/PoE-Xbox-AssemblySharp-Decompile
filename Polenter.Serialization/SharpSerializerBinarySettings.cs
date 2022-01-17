using System.Text;
using Polenter.Serialization.Core;

namespace Polenter.Serialization;

public sealed class SharpSerializerBinarySettings : SharpSerializerSettings<AdvancedSharpSerializerBinarySettings>
{
	public Encoding Encoding { get; set; }

	public BinarySerializationMode Mode { get; set; }

	public SharpSerializerBinarySettings()
	{
		Encoding = Encoding.UTF8;
	}

	public SharpSerializerBinarySettings(BinarySerializationMode mode)
	{
		Encoding = Encoding.UTF8;
		Mode = mode;
	}
}
