using System;

namespace Polenter.Serialization.Advanced.Xml;

public interface ISimpleValueConverter
{
	string ConvertToString(object value);

	object ConvertFromString(string text, Type type);
}
