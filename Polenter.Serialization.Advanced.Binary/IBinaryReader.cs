using System;
using System.IO;

namespace Polenter.Serialization.Advanced.Binary;

public interface IBinaryReader
{
	byte ReadElementId();

	Type ReadType();

	int ReadNumber();

	int[] ReadNumbers();

	string ReadName();

	object ReadValue(Type expectedType);

	void Open(Stream stream);

	void Close();
}
