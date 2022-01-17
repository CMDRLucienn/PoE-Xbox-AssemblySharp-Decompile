using System.Collections.Generic;
using MsgPack;

namespace FluentdNetIO;

public class FluentdEntry : IPackable
{
	public uint ID;

	public int TimeStamp { get; set; }

	public Dictionary<string, object> Message { get; set; }

	public FluentdEntry()
	{
		TimeStamp = 0;
		Message = new Dictionary<string, object>();
	}

	public void Clear()
	{
		TimeStamp = 0;
		Message.Clear();
	}

	public void GenerateData(TeleMsg msg)
	{
		Message.Clear();
		Message.Add("ID", msg.ID);
		Message.Add("Version", msg.Version);
		for (int i = 0; i < msg.Ints.Count; i++)
		{
			Message.Add("I" + i, msg.Ints[i].Int);
		}
		for (int j = 0; j < msg.Dbls.Count; j++)
		{
			Message.Add("R" + j, msg.Dbls[j].Dbl);
		}
		for (int k = 0; k < msg.Strs.Count; k++)
		{
			Message.Add("Str" + k, msg.Strs[k].Str);
		}
	}

	public void PackToMessage(Packer packer, PackingOptions options)
	{
		packer.PackArrayHeader(2);
		packer.Pack(TimeStamp);
		packer.PackMap(Message);
	}
}
