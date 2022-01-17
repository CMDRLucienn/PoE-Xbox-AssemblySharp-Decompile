using System;

[Serializable]
public class StrongholdVisitorSerializeData
{
	public string Tag { get; set; }

	public float TimeToLeave { get; set; }

	public CharacterDatabaseString AssociatedPrisoner { get; set; }
}
