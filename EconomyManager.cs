using UnityEngine;

public class EconomyManager : MonoBehaviour
{
	[Tooltip("The currency value of an item mod is determined by multiplying its 'Cost' by this.")]
	public int ItemModCostMultiplier = 1000;

	[Tooltip("Learning a spell costs (SpellLevel * this).")]
	public int LearnSpellCostMultiplier = 200;

	[Tooltip("Hiring an adventurer costs (AdventurerLevel * this).")]
	public int AdventurerCostMultiplier = 1000;

	[Tooltip("Respecing a party member costs (CharacterLevel * this).")]
	public int RespecCostMultiplier = 500;

	public static EconomyManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'EconomyManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}
}
