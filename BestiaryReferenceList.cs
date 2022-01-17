using UnityEngine;

public class BestiaryReferenceList : ScriptableObject
{
	public BestiaryParent[] TopLevelEntries;

	public BestiaryReference[] Prefabs;

	public RevealStats DefaultReveal = new RevealStats();

	public void Initialize()
	{
		BestiaryReference[] prefabs = Prefabs;
		for (int i = 0; i < prefabs.Length; i++)
		{
			object component = prefabs[i].GetComponent<CharacterStats>();
			if (component != null)
			{
				DataManager.AdjustFromData(ref component);
			}
		}
	}
}
