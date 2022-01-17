using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Polenter.Serialization.Advanced;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Deserializing;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;
using Polenter.Serialization.Core;
using Polenter.Serialization.Deserializing;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization;

public sealed class SharpSerializer
{
	private IPropertyDeserializer _deserializer;

	private PropertyProvider _propertyProvider;

	private string _rootName;

	private IPropertySerializer _serializer;

	public PropertyProvider PropertyProvider
	{
		get
		{
			if (_propertyProvider == null)
			{
				_propertyProvider = new PropertyProvider();
			}
			return _propertyProvider;
		}
		set
		{
			_propertyProvider = value;
		}
	}

	public string RootName
	{
		get
		{
			if (_rootName == null)
			{
				_rootName = "Root";
			}
			return _rootName;
		}
		set
		{
			_rootName = value;
		}
	}

	public SharpSerializer()
	{
		initialize(new SharpSerializerXmlSettings());
	}

	public SharpSerializer(bool binarySerialization)
	{
		if (binarySerialization)
		{
			initialize(new SharpSerializerBinarySettings());
		}
		else
		{
			initialize(new SharpSerializerXmlSettings());
		}
	}

	public SharpSerializer(SharpSerializerXmlSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		initialize(settings);
	}

	public SharpSerializer(SharpSerializerBinarySettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		initialize(settings);
	}

	public SharpSerializer(IPropertySerializer serializer, IPropertyDeserializer deserializer)
	{
		if (serializer == null)
		{
			throw new ArgumentNullException("serializer");
		}
		if (deserializer == null)
		{
			throw new ArgumentNullException("deserializer");
		}
		_serializer = serializer;
		_deserializer = deserializer;
	}

	private void initialize(SharpSerializerXmlSettings settings)
	{
		PropertyProvider.PropertiesToIgnore = settings.AdvancedSettings.PropertiesToIgnore;
		PropertyProvider.AttributesToIgnore = settings.AdvancedSettings.AttributesToIgnore;
		RootName = settings.AdvancedSettings.RootName;
		ITypeNameConverter typeNameConverter = settings.AdvancedSettings.TypeNameConverter ?? DefaultInitializer.GetTypeNameConverter(settings.IncludeAssemblyVersionInTypeName, settings.IncludeCultureInTypeName, settings.IncludePublicKeyTokenInTypeName);
		ISimpleValueConverter simpleValueConverter = settings.AdvancedSettings.SimpleValueConverter ?? DefaultInitializer.GetSimpleValueConverter(settings.Culture, typeNameConverter);
		XmlWriterSettings xmlWriterSettings = DefaultInitializer.GetXmlWriterSettings(settings.Encoding);
		XmlReaderSettings xmlReaderSettings = DefaultInitializer.GetXmlReaderSettings();
		DefaultXmlReader reader = new DefaultXmlReader(typeNameConverter, simpleValueConverter, xmlReaderSettings);
		DefaultXmlWriter writer = new DefaultXmlWriter(typeNameConverter, simpleValueConverter, xmlWriterSettings);
		_serializer = new XmlPropertySerializer(writer);
		_deserializer = new XmlPropertyDeserializer(reader);
	}

	private void initialize(SharpSerializerBinarySettings settings)
	{
		PropertyProvider.PropertiesToIgnore = settings.AdvancedSettings.PropertiesToIgnore;
		PropertyProvider.AttributesToIgnore = settings.AdvancedSettings.AttributesToIgnore;
		RootName = settings.AdvancedSettings.RootName;
		ITypeNameConverter typeNameConverter = settings.AdvancedSettings.TypeNameConverter ?? DefaultInitializer.GetTypeNameConverter(settings.IncludeAssemblyVersionInTypeName, settings.IncludeCultureInTypeName, settings.IncludePublicKeyTokenInTypeName);
		IBinaryReader binaryReader = null;
		IBinaryWriter binaryWriter = null;
		if (settings.Mode == BinarySerializationMode.Burst)
		{
			binaryWriter = new BurstBinaryWriter(typeNameConverter, settings.Encoding);
			binaryReader = new BurstBinaryReader(typeNameConverter, settings.Encoding);
		}
		else
		{
			binaryWriter = new SizeOptimizedBinaryWriter(typeNameConverter, settings.Encoding);
			binaryReader = new SizeOptimizedBinaryReader(typeNameConverter, settings.Encoding);
		}
		_deserializer = new BinaryPropertyDeserializer(binaryReader);
		_serializer = new BinaryPropertySerializer(binaryWriter);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Serialize(object data, string filename)
	{
		createDirectoryIfNeccessary(filename);
		using Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
		Serialize(data, stream);
	}

	private void createDirectoryIfNeccessary(string filename)
	{
		string directoryName = Path.GetDirectoryName(filename);
		if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Serialize(object data, Stream stream)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Property property = new PropertyFactory(PropertyProvider).CreateProperty(RootName, data);
		try
		{
			_serializer.Open(stream);
			_serializer.Serialize(property);
		}
		finally
		{
			_serializer.Close();
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public object Deserialize(string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
		return Deserialize(stream);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public object Deserialize(Stream stream)
	{
		try
		{
			_deserializer.Open(stream);
			Property property = _deserializer.Deserialize();
			_deserializer.Close();
			return new ObjectFactory().CreateObject(property);
		}
		catch (Exception inner)
		{
			throw new DeserializingException("An error occured during the deserialization. Details are in the inner exception.", inner);
		}
	}
}
