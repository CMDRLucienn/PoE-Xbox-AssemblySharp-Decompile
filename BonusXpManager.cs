using UnityEngine;

public class BonusXpManager : MonoBehaviour
{
	[Tooltip("The amount of experience awarded for unlocking all information about a creature. Note that bestiary references may also use an XP multiplier to determine the final value.")]
	public int BestiaryXp = 36;

	[Tooltip("The amount of experience awarded to the party for entering a scene for the first time.")]
	public int MapExplorationXp = 10;

	[Tooltip("The amount of experience awarded to the party for discovering a map marker that grants XP.")]
	public int MapMarkerDiscoveryXp = 25;

	[Tooltip("This value is multiplied by the difficulty of a disarmed trap to determine how much XP to give the party.")]
	public int DisarmTrapXpModifier = 5;

	[Tooltip("This value is multiplied by the difficulty of a picked lock to determine how much XP to give the party.")]
	public int PickLockXpModifier = 5;

	public static BonusXpManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'BonusXpManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
