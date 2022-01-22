using System;
using System.Runtime.Serialization;

public class IEModException : Exception
{
	public IEModException()
	{
	}

	public IEModException(string message)
		: base(message)
	{
	}

	public IEModException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected IEModException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}