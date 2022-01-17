using System;
using System.Collections.Generic;

namespace FluentdNetIO;

public class TeleMsg
{
	public struct FIELDuuid
	{
		public string Name;

		public Guid UUid;
	}

	public struct FIELDint
	{
		public string Name;

		public long Int;
	}

	public struct FIELDdbl
	{
		public string Name;

		public double Dbl;
	}

	public struct FIELDstr
	{
		public string Name;

		public string Str;
	}

	public uint ID;

	public uint Version;

	public string Name;

	public List<FIELDuuid> Uuids = new List<FIELDuuid>();

	public List<FIELDint> Ints = new List<FIELDint>();

	public List<FIELDdbl> Dbls = new List<FIELDdbl>();

	public List<FIELDstr> Strs = new List<FIELDstr>();

	public void Clear()
	{
		ID = 0u;
		Version = 0u;
		Name = string.Empty;
		Uuids.Clear();
		Ints.Clear();
		Dbls.Clear();
		Strs.Clear();
	}
}
