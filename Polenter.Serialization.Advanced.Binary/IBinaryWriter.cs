using System;
using System.IO;

namespace Polenter.Serialization.Advanced.Binary;

public interface IBinaryWriter
{
	void WriteElementId(byte id);

	void WriteType(Type type);

	void WriteName(string name);

	void WriteValue(object value);

	void WriteNumber(int number);

	void WriteNumbers(int[] numbers);

	void Open(Stream stream);

	void Close();
}
