// IEMod.Helpers.IEModException
using System;
using System.Runtime.Serialization;
using Patchwork.Attributes;

[PatchedByType("IEMod.Helpers.IEModException")]
[NewType(null, null)]
public class IEModException : Exception
{
	[PatchedByMember("System.Void IEMod.Helpers.IEModException::.ctor()")]
	public IEModException()
	{
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEModException::.ctor(System.String)")]
	public IEModException(string message)
		: base(message)
	{
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEModException::.ctor(System.String,System.Exception)")]
	public IEModException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEModException::.ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)")]
	protected IEModException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}