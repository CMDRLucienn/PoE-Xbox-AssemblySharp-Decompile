using System;
using UnityEngine;

public class Religion : MonoBehaviour
{
	[Serializable]
	public enum Deity
	{
		None,
		Berath,
		Eothas,
		Magran,
		Skaen,
		Wael,
		Count
	}

	[Serializable]
	public class DeityData
	{
		public Deity DeityName;

		public CharacterDatabaseString DisplayName = new CharacterDatabaseString();

		public Disposition.Axis[] PositiveTrait;

		public Disposition.Axis[] NegativeTrait;
	}

	[Serializable]
	public enum PaladinOrder
	{
		None,
		BleakWalkers,
		DarcozziPaladini,
		GoldpactKnights,
		KindWayfarers,
		ShieldbearersOfStElcga,
		FrermasMesCancSuolias,
		Count
	}

	[Serializable]
	public class PaladinOrderData
	{
		public PaladinOrder OrderName;

		public FactionDatabaseString DisplayName = new FactionDatabaseString();

		public Disposition.Axis[] PositiveTrait;

		public Disposition.Axis[] NegativeTrait;
	}

	public DeityData[] DeityInfo;

	public PaladinOrderData[] PaladinOrderInfo;

	private static int MAX_BONUS_LEVEL = 3;

	public float[] PositiveTraitBonus;

	public float[] NegativeTraitBonus;

	public static Religion Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'Religion' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
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

	public DeityData FindDeityData(Deity d)
	{
		DeityData[] deityInfo = DeityInfo;
		foreach (DeityData deityData in deityInfo)
		{
			if (deityData.DeityName == d)
			{
				return deityData;
			}
		}
		return null;
	}

	public PaladinOrderData FindPaladinOrderData(PaladinOrder order)
	{
		PaladinOrderData[] paladinOrderInfo = PaladinOrderInfo;
		foreach (PaladinOrderData paladinOrderData in paladinOrderInfo)
		{
			if (paladinOrderData.OrderName == order)
			{
				return paladinOrderData;
			}
		}
		return null;
	}

	public float GetCurrentBonusMultiplier(GameObject obj, GenericAbility abilityOrigin)
	{
		if ((bool)obj)
		{
			return GetCurrentBonusMultiplier(obj.GetComponent<CharacterStats>(), abilityOrigin);
		}
		return 1f;
	}

	public float GetCurrentBonusMultiplier(CharacterStats stats, GenericAbility abilityOrigin)
	{
		if (stats == null)
		{
			return 1f;
		}
		float num = 0f;
		if (GameState.s_playerCharacter != null && stats.gameObject == GameState.s_playerCharacter.gameObject)
		{
			float num2 = 1f;
			if (abilityOrigin != null)
			{
				num2 *= abilityOrigin.GatherAbilityModProduct(AbilityMod.AbilityModType.NegativeReligiousTraitMultiplier);
			}
			if (stats.CharacterClass == CharacterStats.Class.Priest)
			{
				DeityData deityData = FindDeityData(stats.Deity);
				if (deityData != null)
				{
					num += GetBonus(deityData.PositiveTrait[0], PositiveTraitBonus);
					num += GetBonus(deityData.PositiveTrait[1], PositiveTraitBonus);
					num += GetBonus(deityData.NegativeTrait[0], NegativeTraitBonus) * num2;
					num += GetBonus(deityData.NegativeTrait[1], NegativeTraitBonus) * num2;
				}
			}
			else if (stats.CharacterClass == CharacterStats.Class.Paladin)
			{
				PaladinOrderData paladinOrderData = FindPaladinOrderData(stats.PaladinOrder);
				if (paladinOrderData != null)
				{
					num += GetBonus(paladinOrderData.PositiveTrait[0], PositiveTraitBonus);
					num += GetBonus(paladinOrderData.PositiveTrait[1], PositiveTraitBonus);
					num += GetBonus(paladinOrderData.NegativeTrait[0], NegativeTraitBonus) * num2;
					num += GetBonus(paladinOrderData.NegativeTrait[1], NegativeTraitBonus) * num2;
				}
			}
		}
		return num + 1f;
	}

	private float GetBonus(Disposition.Axis axis, float[] bonus)
	{
		int num = Math.Min(ReputationManager.Instance.PlayerDisposition.GetRank(axis), MAX_BONUS_LEVEL);
		if (num == 0)
		{
			return 0f;
		}
		return bonus[num - 1];
	}

	public string GetDispositionsString(CharacterStats character, bool positive)
	{
		if (character.gameObject != GameState.s_playerCharacter.gameObject)
		{
			return "";
		}
		if (character.CharacterClass == CharacterStats.Class.Priest)
		{
			return GetDispositionsString(character.Deity, positive);
		}
		if (character.CharacterClass == CharacterStats.Class.Paladin)
		{
			return GetDispositionsString(character.PaladinOrder, positive);
		}
		return "";
	}

	public string GetDispositionsString(Deity deity, bool positive)
	{
		string text = "";
		DeityData[] deityInfo = DeityInfo;
		foreach (DeityData deityData in deityInfo)
		{
			if (deityData == null || deityData.DeityName != deity)
			{
				continue;
			}
			Disposition.Axis[] array = (positive ? deityData.PositiveTrait : deityData.NegativeTrait);
			foreach (Disposition.Axis axis in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += FactionUtils.GetDispositionAxisString(axis);
			}
			break;
		}
		return text;
	}

	public string GetDispositionsString(PaladinOrder order, bool positive)
	{
		string text = "";
		PaladinOrderData[] paladinOrderInfo = PaladinOrderInfo;
		foreach (PaladinOrderData paladinOrderData in paladinOrderInfo)
		{
			if (paladinOrderData == null || paladinOrderData.OrderName != order)
			{
				continue;
			}
			Disposition.Axis[] array = (positive ? paladinOrderData.PositiveTrait : paladinOrderData.NegativeTrait);
			foreach (Disposition.Axis axis in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += FactionUtils.GetDispositionAxisString(axis);
			}
			break;
		}
		return text;
	}
}
