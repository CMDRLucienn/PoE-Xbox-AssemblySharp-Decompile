using System.IO;
using Polenter.Serialization.Core;

namespace Polenter.Serialization.Advanced.Deserializing;

public interface IPropertyDeserializer
{
	void Open(Stream stream);

	Property Deserialize();

	void Close();
}
