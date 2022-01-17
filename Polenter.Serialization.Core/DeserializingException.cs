using System;
using System.Runtime.Serialization;

namespace Polenter.Serialization.Core;

[Serializable]
public class DeserializingException : Exception
{
	public DeserializingException()
	{
	}

	public DeserializingException(string message)
		: base(message)
	{
	}

	public DeserializingException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected DeserializingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
