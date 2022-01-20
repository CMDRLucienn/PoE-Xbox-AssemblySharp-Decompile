// IEMod.Mods.StringTable.IEModString
using System.Collections.Generic;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.Mods.StringTable.IEModString")]
public class IEModString : GUIDatabaseString
{
	private static readonly Dictionary<int, string> IeModStringTable = new Dictionary<int, string>();

	private static int _lastId = 1;

	[PatchedByMember("System.Void IEMod.Mods.StringTable.IEModString::.ctor(System.Int32)")]
	private IEModString(int id)
		: base(id)
	{
		StringTable = StringTableType.IEModGUI;
	}

	[PatchedByMember("System.String IEMod.Mods.StringTable.IEModString::GetString(System.Int32)")]
	public static string GetString(int id)
	{
		if (!IeModStringTable.ContainsKey(id))
		{
			return $"?? IEMod {id} ??";
		}
		return IeModStringTable[id];
	}

	[PatchedByMember("System.Void IEMod.Mods.StringTable.IEModString::Unregister()")]
	public void Unregister()
	{
		IeModStringTable.Remove(StringID);
	}

	[PatchedByMember("IEMod.Mods.StringTable.IEModString IEMod.Mods.StringTable.IEModString::Register(System.String)")]
	public static IEModString Register(string str)
	{
		int lastId = _lastId;
		_lastId++;
		IeModStringTable[lastId] = str;
		return new IEModString(lastId);
	}

	[PatchedByMember("DatabaseString/StringTableType IEMod.Mods.StringTable.IEModString::GetStringTable()")]
	public override StringTableType GetStringTable()
	{
		return StringTableType.IEModGUI;
	}
}
