using System.Globalization;
using System.Text;
using System.Xml;
using Polenter.Serialization.Advanced;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;

namespace Polenter.Serialization.Core;

internal static class DefaultInitializer
{
	public static XmlWriterSettings GetXmlWriterSettings()
	{
		return GetXmlWriterSettings(Encoding.UTF8);
	}

	public static XmlWriterSettings GetXmlWriterSettings(Encoding encoding)
	{
		return new XmlWriterSettings
		{
			Encoding = encoding,
			Indent = true,
			OmitXmlDeclaration = true
		};
	}

	public static XmlReaderSettings GetXmlReaderSettings()
	{
		return new XmlReaderSettings
		{
			IgnoreComments = true,
			IgnoreWhitespace = true
		};
	}

	public static ITypeNameConverter GetTypeNameConverter(bool includeAssemblyVersion, bool includeCulture, bool includePublicKeyToken)
	{
		return new TypeNameConverter(includeAssemblyVersion, includeCulture, includePublicKeyToken);
	}

	public static ISimpleValueConverter GetSimpleValueConverter(CultureInfo cultureInfo, ITypeNameConverter typeNameConverter)
	{
		return new SimpleValueConverter(cultureInfo, typeNameConverter);
	}
}
