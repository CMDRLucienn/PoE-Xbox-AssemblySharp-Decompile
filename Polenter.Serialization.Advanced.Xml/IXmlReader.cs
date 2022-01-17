using System;
using System.Collections.Generic;
using System.IO;

namespace Polenter.Serialization.Advanced.Xml;

public interface IXmlReader
{
	string ReadElement();

	IEnumerable<string> ReadSubElements();

	string GetAttributeAsString(string attributeName);

	Type GetAttributeAsType(string attributeName);

	int GetAttributeAsInt(string attributeName);

	int[] GetAttributeAsArrayOfInt(string attributeName);

	object GetAttributeAsObject(string attributeName, Type expectedType);

	void Open(Stream stream);

	void Close();
}
