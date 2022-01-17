using MsgPack;

namespace FluentdNetIO;

public class FluentdMessage : IPackable
{
	private int messageIndex;

	public string Tag { get; set; }

	public FluentdEntry[] Entries { get; set; }

	public virtual bool IsFull => messageIndex >= Entries.Length;

	public virtual bool IsEmpty => messageIndex == 0;

	public FluentdMessage(string tag, int capacity)
	{
		Tag = tag;
		Entries = new FluentdEntry[capacity];
		for (int i = 0; i < Entries.Length; i++)
		{
			Entries[i] = new FluentdEntry();
		}
	}

	public void AddEntry(TeleMsg msg, int timeStamp)
	{
		if (!IsFull)
		{
			Entries[messageIndex].TimeStamp = timeStamp;
			Entries[messageIndex].GenerateData(msg);
			messageIndex++;
		}
	}

	public void Clear()
	{
		messageIndex = 0;
		for (int i = 0; i < Entries.Length; i++)
		{
			Entries[i].Clear();
		}
	}

	public void PackToMessage(Packer packer, PackingOptions options)
	{
		packer.PackArrayHeader(2);
		packer.Pack(Tag);
		packer.PackArrayHeader(messageIndex);
		for (int i = 0; i < messageIndex; i++)
		{
			packer.Pack(Entries[i]);
		}
	}
}
