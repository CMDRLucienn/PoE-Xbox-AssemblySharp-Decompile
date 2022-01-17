using UnityEngine;

public class SpellMax : MonoBehaviour
{
	public SpellMaxReference Reference;

	public static SpellMax Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'SpellMax' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
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

	public int GetSpellCastMax(GameObject caster, int spellLevel)
	{
		int num = int.MaxValue;
		if (caster != null && spellLevel >= 1)
		{
			CharacterStats component = caster.GetComponent<CharacterStats>();
			if (component != null)
			{
				num = SpellCastMaxLookup(component.CharacterClass, component.ScaledLevel, spellLevel);
				switch (num)
				{
				case int.MaxValue:
					if (CharacterStats.IsPlayableClass(component.CharacterClass) && caster.GetComponent<PartyMemberAI>() != null)
					{
						num = 0;
					}
					break;
				case -1:
					num = int.MaxValue;
					break;
				}
				if (num > 0 && num < int.MaxValue)
				{
					num += component.SpellCastBonus[spellLevel - 1];
				}
			}
		}
		return num;
	}

	public int GetPerEncounterCharacterLevel(GameObject caster, int spellLevel)
	{
		int result = int.MaxValue;
		if (caster != null)
		{
			CharacterStats component = caster.GetComponent<CharacterStats>();
			if (component != null)
			{
				result = SpellPerEncounterLevelLookup(component.CharacterClass, spellLevel);
			}
		}
		return result;
	}

	public int GetMaxSpellLevel(GameObject caster)
	{
		for (int i = 1; i <= 8; i++)
		{
			if (GetSpellCastMax(caster, i) == 0)
			{
				return i - 1;
			}
		}
		return 8;
	}

	public int GetSpellLevelNowTriggeredPerEncounter(CharacterStats.Class casterClass, int prevLevel, int newLevel)
	{
		SpellMaxData spellMaxData = null;
		if (Reference != null && Reference.SpellMaxList != null)
		{
			for (int i = 0; i < Reference.SpellMaxList.Length; i++)
			{
				spellMaxData = Reference.SpellMaxList[i];
				if (spellMaxData == null || spellMaxData.MinLevelPerEncounter == null || spellMaxData.Class != casterClass)
				{
					continue;
				}
				for (int j = 0; j < spellMaxData.MinLevelPerEncounter.Length; j++)
				{
					if (SpellCastMaxLookup(casterClass, newLevel, j + 1) > 0 && spellMaxData.MinLevelPerEncounter[j] > prevLevel && spellMaxData.MinLevelPerEncounter[j] <= newLevel)
					{
						return j + 1;
					}
				}
				break;
			}
		}
		return 0;
	}

	private int SpellCastMaxLookup(CharacterStats.Class casterClass, int casterLevel, int spellLevel)
	{
		if (Reference != null && Reference.SpellMaxList != null)
		{
			SpellMaxData[] spellMaxList = Reference.SpellMaxList;
			foreach (SpellMaxData spellMaxData in spellMaxList)
			{
				if (spellMaxData.Class == casterClass)
				{
					int num = Mathf.Max(0, casterLevel - 1);
					if (num >= spellMaxData.MaxCastByLevel.Length)
					{
						num = spellMaxData.MaxCastByLevel.Length - 1;
					}
					if (spellLevel - 1 >= spellMaxData.MaxCastByLevel[num].MaxCast.Length)
					{
						return 0;
					}
					try
					{
						return spellMaxData.MaxCastByLevel[num].MaxCast[spellLevel - 1];
					}
					catch
					{
						return int.MaxValue;
					}
				}
			}
		}
		return int.MaxValue;
	}

	public int SpellPerEncounterLevelLookup(CharacterStats.Class casterClass, int spellLevel)
	{
		if (Reference != null && Reference.SpellMaxList != null)
		{
			SpellMaxData[] spellMaxList = Reference.SpellMaxList;
			foreach (SpellMaxData spellMaxData in spellMaxList)
			{
				if (spellMaxData.Class == casterClass)
				{
					return spellMaxData.MinLevelPerEncounter[spellLevel - 1];
				}
			}
		}
		return int.MaxValue;
	}
}
