// IEMod.Mods.Options.SaveAttribute
using System;
using Patchwork.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[NewType(null, null)]
[PatchedByType("IEMod.Mods.Options.SaveAttribute")]
public class SaveAttribute : Attribute
{
	[PatchedByMember("System.Void IEMod.Mods.Options.SaveAttribute::.ctor()")]
	public SaveAttribute()
	{
	}
}