using UnityEngine;

public class PartyAISettings : MonoBehaviour
{
	[Persistent]
	public bool UseInstructionSet = true;

	[Persistent]
	public bool UsePerRestAbilitiesInInstructionSet;

	[Persistent]
	public int InstructionSetIndex = -1;
}
