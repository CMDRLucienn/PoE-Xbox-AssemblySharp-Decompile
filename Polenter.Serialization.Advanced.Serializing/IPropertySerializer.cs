using System.IO;
using Polenter.Serialization.Core;

namespace Polenter.Serialization.Advanced.Serializing;

public interface IPropertySerializer
{
	void Open(Stream stream);

	void Serialize(Property property);

	void Close();
}
