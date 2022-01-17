using System;
using System.IO;

namespace Polenter.Serialization.Advanced.Xml;

public interface IXmlWriter
{
	void WriteStartElement(string elementId);

	void WriteEndElement();

	void WriteAttribute(string attributeId, string text);

	void WriteAttribute(string attributeId, Type type);

	void WriteAttribute(string attributeId, int number);

	void WriteAttribute(string attributeId, int[] numbers);

	void WriteAttribute(string attributeId, object value);

	void Open(Stream stream);

	void Close();
}
