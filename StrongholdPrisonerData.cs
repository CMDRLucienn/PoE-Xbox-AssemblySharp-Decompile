using System;

[Serializable]
public class StrongholdPrisonerData
{
	public CharacterDatabaseString PrisonerName { get; set; }

	public StrongholdDatabaseString PrisonerDescription { get; set; }

	public string GlobalVariableName { get; set; }
}
