using System;
using System.IO;

[Serializable]
public class ConversationObject : PickerObject
{
	public string Filename;

	public ConversationObject()
		: base(PickerType.Conversation)
	{
		Filename = string.Empty;
	}

	public void Start()
	{
		string overridePath = GameResources.GetOverridePath(Path.GetFileName(Filename));
		if (!string.IsNullOrEmpty(overridePath))
		{
			Filename = overridePath;
		}
	}
}
