// IEMod.Helpers.DeadEndException
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.Helpers.DeadEndException")]
public class DeadEndException : IEModException
{
	[PatchedByMember("System.Void IEMod.Helpers.DeadEndException::.ctor(System.String)")]
	public DeadEndException(string location)
		: base($"Code should be unreachable. Location: {location}")
	{
	}
}
