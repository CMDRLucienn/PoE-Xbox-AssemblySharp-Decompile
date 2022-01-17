using UnityEngine;

public class StrongholdPrisoner : MonoBehaviour
{
	public CharacterDatabaseString PrisonerName = new CharacterDatabaseString();

	public StrongholdDatabaseString PrisonerDescription = new StrongholdDatabaseString();

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when the prisoner is at the stronghold and 0 when he is not.")]
	public string GlobalVariableName;
}
