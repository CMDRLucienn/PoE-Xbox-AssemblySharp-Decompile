using System;

namespace Polenter.Serialization.Advanced.Serializing;

public interface ITypeNameConverter
{
	string ConvertToTypeName(Type type);

	Type ConvertToType(string typeName);
}
