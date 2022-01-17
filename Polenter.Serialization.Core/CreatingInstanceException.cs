using System;
using System.Runtime.Serialization;

namespace Polenter.Serialization.Core;

[Serializable]
public class CreatingInstanceException : Exception
{
	public CreatingInstanceException()
	{
	}

	public CreatingInstanceException(string message)
		: base(message)
	{
	}

	public CreatingInstanceException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected CreatingInstanceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
