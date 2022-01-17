using System;
using System.Runtime.Serialization;

namespace Polenter.Serialization.Core;

[Serializable]
public class SimpleValueParsingException : Exception
{
	public SimpleValueParsingException()
	{
	}

	public SimpleValueParsingException(string message)
		: base(message)
	{
	}

	public SimpleValueParsingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected SimpleValueParsingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
