// IEMod.Mods.Options.XmlNullable<T>
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.Mods.Options.XmlNullable`1")]
public class XmlNullable<T>
{
	public T Value
	{
		[PatchedByMember("T IEMod.Mods.Options.XmlNullable`1::get_Value()")]
		get;
		[PatchedByMember("System.Void IEMod.Mods.Options.XmlNullable`1::set_Value(T)")]
		set;
	}

	[PatchedByMember("System.Void IEMod.Mods.Options.XmlNullable`1::.ctor(T)")]
	public XmlNullable(T value)
	{
		Value = value;
	}

	[PatchedByMember("System.Void IEMod.Mods.Options.XmlNullable`1::.ctor()")]
	public XmlNullable()
	{
	}

	[PatchedByMember("IEMod.Mods.Options.XmlNullable`1<T> IEMod.Mods.Options.XmlNullable`1::op_Implicit(T)")]
	public static implicit operator XmlNullable<T>(T value)
	{
		return new XmlNullable<T>(value);
	}
}
