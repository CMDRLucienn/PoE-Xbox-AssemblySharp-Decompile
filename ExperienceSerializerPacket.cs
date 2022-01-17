using System;

[Serializable]
public class ExperienceSerializerPacket
{
	public string Name = string.Empty;

	public int ExperienceGiven;

	public float StrongholdTurnsGiven;

	public ExperienceSerializerPacket()
	{
	}

	public ExperienceSerializerPacket(string filename, int experienceGiven, int strongholdTurnsGiven)
	{
		ExperienceGiven = experienceGiven;
		StrongholdTurnsGiven = strongholdTurnsGiven;
		Name = filename;
	}
}
