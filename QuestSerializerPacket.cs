using System;
using System.Collections.Generic;

[Serializable]
public class QuestSerializerPacket
{
	public List<int> ActiveStates;

	public string Name = string.Empty;

	public int QuestDescriptionID;

	public QuestSerializerPacket()
	{
	}

	public QuestSerializerPacket(string filename, List<int> activeStates, int questDescriptionID)
	{
		ActiveStates = activeStates;
		Name = filename;
		QuestDescriptionID = questDescriptionID;
	}
}
