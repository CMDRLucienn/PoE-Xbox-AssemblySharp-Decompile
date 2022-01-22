using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grimoire : MonoBehaviour
{
	[Serializable]
	public class SpellChapter
	{
		public GenericSpell[] SpellData = new GenericSpell[4];

		public List<GenericSpell> SerializedData
		{
			get
			{
				List<GenericSpell> list = new List<GenericSpell>();
				list.AddRange(SpellData);
				return list;
			}
			set
			{
				SpellData = value.ToArray();
			}
		}

		public bool IsFull()
		{
			return SpellData.Count((GenericSpell gs) => gs) >= 4;
		}
	}

	public const int MaxSpellLevel = 8;

	public const int MaxSpellsPerLevel = 4;

	[Persistent]
	private string m_PrimaryOwnerName = "";

	public SpellChapter[] Spells = new SpellChapter[8];

	public string PrimaryOwnerName
	{
		get
		{
			return m_PrimaryOwnerName;
		}
		set
		{
			m_PrimaryOwnerName = value;
		}
	}

	[Persistent]
	public SpellChapter[] SerializedSpells
	{
		get
		{
			return Spells;
		}
		set
		{
			Spells = value;
		}
	}

	[Persistent]
	public List<string> SerializedSpellNames
	{
		get
		{
			List<string> list = new List<string>();
			SpellChapter[] spells = Spells;
			for (int i = 0; i < spells.Length; i++)
			{
				GenericSpell[] spellData = spells[i].SpellData;
				foreach (GenericSpell genericSpell in spellData)
				{
					if ((bool)genericSpell)
					{
						list.Add(genericSpell.gameObject.name.Replace("(Clone)", ""));
					}
				}
			}
			return list;
		}
		set
		{
			int[] array = new int[8];
			Spells = new SpellChapter[8];
			for (int i = 0; i < Spells.Length; i++)
			{
				Spells[i] = new SpellChapter();
			}
			foreach (string item in value)
			{
				if (string.IsNullOrEmpty(item))
				{
					continue;
				}
				GenericSpell genericSpell = GameResources.LoadPrefab<GenericSpell>(item, instantiate: false);
				if ((bool)genericSpell)
				{
					int num = genericSpell.SpellLevel - 1;
					int num2 = array[num];
					if (Spells.Length > num && Spells[num].SpellData.Length > num2)
					{
						Spells[num].SpellData[num2] = genericSpell;
						array[num]++;
					}
				}
			}
		}
	}

	private void FixBrokenGrimiore()
	{
		for (int i = 0; i < Spells.Length; i++)
		{
			if (Spells[i] == null || Spells[i].SpellData == null)
			{
				continue;
			}
			for (int j = 0; j < Spells[i].SpellData.Length; j++)
			{
				if (Spells[i].SpellData[j] == null)
				{
					Spells[i].SpellData[j] = null;
				}
			}
		}
	}

	public void Restored()
	{
		FixBrokenGrimiore();
	}

	private void Start()
	{
		if (this.Spells.Length != MaxSpellLevel)
		{
			if (this.Spells.Length > MaxSpellLevel)
			{
				Debug.LogError("Too many spell levels in grimoire '" + base.name + "': some will be dropped!");
			}
			Grimoire.SpellChapter[] array = new Grimoire.SpellChapter[MaxSpellLevel];
			this.Spells.CopyTo(array, 0);
			this.Spells = array;

		}
		for (int i = 0; i < this.Spells.Length; i++)
		{
			if (this.Spells[i] == null)
			{
				this.Spells[i] = new Grimoire.SpellChapter();
			}
			else if (this.Spells[i].SpellData.Length != MaxSpellsPerLevel + (int)IEModOptions.ExtraWizardSpells)
			{
				if (this.Spells[i].SpellData.Length > MaxSpellsPerLevel + (int)IEModOptions.ExtraWizardSpells)
				{
					Debug.LogError(string.Concat(new object[]
					{
							"Too many spell slots in grimoire '",
							base.name,
							"' for level ",
							i + 1,
							": some will be dropped!"
					}));
				}
				GenericSpell[] array2 = new GenericSpell[(MaxSpellsPerLevel + (int)IEModOptions.ExtraWizardSpells)];
				for (int j = 0; j < Mathf.Min(array2.Length, this.Spells[i].SpellData.Length); j++)
				{
					array2[j] = this.Spells[i].SpellData[j];
				}
				this.Spells[i].SpellData = array2;
			}
		}
	}

	public bool IsLevelFull(int level)
	{
		level--;

		if (level >= this.Spells.Length)
		{
			return true;
		}

		int numBonusSpells = (int)IEModOptions.ExtraWizardSpells;

		if (numBonusSpells == 0 && Spells[level].IsFull())
		{
			return true;
		}
		int preparedCount = Spells[level].SpellData.Count(x => x);
		var isFull = preparedCount == Spells[level].SpellData.Length;
		return isFull;
	}

	public void RemoveAllSpells()
	{
		if (Spells == null)
		{
			return;
		}
		for (int i = 0; i < Spells.Length; i++)
		{
			if (Spells[i] != null && Spells[i].SpellData != null)
			{
				for (int j = 0; j < Spells[i].SpellData.Length; j++)
				{
					Spells[i].SpellData[j] = null;
				}
			}
		}
	}

	public bool HasSpell(GenericSpell spell)
	{
		if (spell != null)
		{
			int num = spell.SpellLevel - 1;
			if (num >= 0 && num < MaxSpellLevel)
			{
				for (int i = 0; i < MaxSpellsPerLevel + (int)IEModOptions.ExtraWizardSpells; i++)
				{
					if (this.Spells[num].SpellData[i] != null && this.Spells[num].SpellData[i].DisplayName.StringID == spell.DisplayName.StringID)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void FindNewSpells(List<GenericSpell> newSpells, CharacterStats casterStats, int maxSpellLevel)
	{
		if (!casterStats)
		{
			return;
		}
		int num = Mathf.Min(maxSpellLevel, 8);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < MaxSpellsPerLevel + (int)IEModOptions.ExtraWizardSpells; j++)
			{
				GenericSpell spellData = this.Spells[i].SpellData[j];
				if (spellData != null)
				{
					bool flag = false;
					IEnumerator<GenericAbility> enumerator = casterStats.ActiveAbilities.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							GenericAbility current = enumerator.Current;
							if (!(current is GenericSpell) || spellData.DisplayName.StringID != current.DisplayName.StringID)
							{
								continue;
							}
							flag = true;
							break;
						}
					}
					finally
					{
						if (enumerator == null)
						{
						}
						enumerator.Dispose();
					}
					if (!flag && !newSpells.Contains(spellData))
					{
						newSpells.Add(spellData);
					}
				}
			}
		}
		//if (caster == null)
		//{
		//	return null;
		//}
		//CharacterStats component = caster.GetComponent<CharacterStats>();
		//if (component == null)
		//{
		//	return null;
		//}
		//List<GenericSpell> list = new List<GenericSpell>();
		//int num = MaxSpellLevel;
		//if (max_spell_level < num)
		//{
		//	num = max_spell_level;
		//}
		//for (int i = 0; i < num; i++)
		//{
		//	for (int j = 0; j < MaxSpellsPerLevel + (int)IEModOptions.ExtraWizardSpells; j++)
		//	{
		//		if (this.Spells[i].SpellData[j] != null)
		//		{
		//			bool flag = false;
		//			foreach (GenericAbility current in component.ActiveAbilities)
		//			{
		//				if (current is GenericSpell && this.Spells[i].SpellData[j].DisplayName.StringID == current.DisplayName.StringID)
		//				{
		//					flag = true;
		//					break;
		//				}
		//			}
		//			if (!flag)
		//			{
		//				list.Add(this.Spells[i].SpellData[j]);
		//			}
		//		}
		//	}
		//}
		//GenericSpell result = null;
		//if (list.Count > 0)
		//{
		//	int index = UnityEngine.Random.Range(0, list.Count);
		//	result = list[index];
		//}
		//return result;
	}
	public static Grimoire Find(GameObject character)
	{
		Equipment component = character.GetComponent<Equipment>();
		if ((bool)component)
		{
			Equippable grimoire = component.CurrentItems.Grimoire;
			if ((bool)grimoire)
			{
				Grimoire component2 = grimoire.GetComponent<Grimoire>();
				if ((bool)component2)
				{
					return component2;
				}
				Debug.LogError("Tried to find grimoire of '" + character.name + "' but found a non-grimoire item in that slot.");
			}
		}
		return null;
	}
}
