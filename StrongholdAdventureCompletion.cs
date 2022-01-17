using System;
using System.Collections.Generic;

[Serializable]
public class StrongholdAdventureCompletion
{
	private int m_PremadeAdventureIndex = -1;

	public StrongholdAdventure.Type Type { get; set; }

	public string AdventurerName { get; set; }

	public Guid AdventurerGuid { get; set; }

	public List<string> RewardStrings { get; set; }

	public int PremadeAdventureIndex
	{
		get
		{
			return m_PremadeAdventureIndex;
		}
		set
		{
			m_PremadeAdventureIndex = value;
		}
	}

	public static StrongholdAdventureCompletion Create(StrongholdAdventure adventure)
	{
		return new StrongholdAdventureCompletion
		{
			Type = adventure.AdventureType,
			PremadeAdventureIndex = adventure.PremadeAdventureIndex,
			AdventurerName = adventure.Adventurer.DisplayName,
			AdventurerGuid = adventure.Adventurer.GUID
		};
	}
}
