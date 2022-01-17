using System;

[Serializable]
public abstract class PickerObject
{
	public enum PickerType
	{
		Script,
		Conversation,
		GlobalVariable,
		Quest
	}

	public PickerType Type { get; protected set; }

	public PickerObject(PickerType type)
	{
		Type = type;
	}
}
