using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class BestiaryReference : MonoBehaviour, IFormatProvider, ICustomFormatter, ITreeListContent
{
	public ProductConfiguration.Package Package = ProductConfiguration.Package.BaseGame;

	[Tooltip("The number of kills before all information is revealed.")]
	public int KillsToMaster = 10;

	[Tooltip("The value that gets multiplied by the global BestiaryExperience value to determine how much XP is rewarded for unlocking all information about this creature.")]
	public float ExperienceMultiplier = 1f;

	[Tooltip("Use this to override the default reveal points for any stat.")]
	public RevealStats RevealOverrides = new RevealStats();

	[Tooltip("If set, this will appear in the UI as a child of the parent with the specified tag.")]
	public string ParentTag;

	[ResourcesImageProperty]
	public string PicturePath;

	public CyclopediaDatabaseString Description;

	public bool OverrideDamage;

	public DamagePacket PrimaryDamage;

	public DamagePacket SecondaryDamage;

	private static string[] m_IndexableNames = Enum.GetNames(typeof(IndexableStat));

	public int TotalExperienceGained => Mathf.FloorToInt(ExperienceMultiplier * (float)BonusXpManager.Instance.BestiaryXp);

	public bool IsTopLevel => string.IsNullOrEmpty(ParentTag);

	public object GetStatByIndex(IndexableStat stat, CharacterStats stats)
	{
		if (!BestiaryManager.Instance.CanSeeStat(this, stat))
		{
			return GUIUtils.GetText(301);
		}
		if (!stats)
		{
			stats = GetComponent<CharacterStats>();
		}
		switch (stat)
		{
		case IndexableStat.DESCRIPTION:
			return Description.GetText(Gender.Neuter);
		case IndexableStat.PICTURE:
			return PicturePath;
		case IndexableStat.KILLS:
			return BestiaryManager.Instance.GetKillCount(this);
		case IndexableStat.PRIMARYDAMAGE:
		case IndexableStat.SECONDARYDAMAGE:
		{
			if (OverrideDamage)
			{
				DamagePacket damagePacket = ((stat == IndexableStat.PRIMARYDAMAGE) ? PrimaryDamage : SecondaryDamage);
				if (damagePacket.DoesDamage)
				{
					return damagePacket.GetString(null, null, 1f, null, showBase: false);
				}
				return GUIUtils.GetText(343);
			}
			Equipment component = stats.GetComponent<Equipment>();
			if ((bool)component)
			{
				Equippable equippable = ((stat == IndexableStat.PRIMARYDAMAGE) ? component.DefaultEquippedItems.PrimaryWeapon : component.DefaultEquippedItems.SecondaryWeapon);
				if ((bool)equippable)
				{
					AttackBase component2 = equippable.GetComponent<AttackBase>();
					if ((bool)component2)
					{
						return component2.DamageData.GetString(stats, component2, 1f, null, showBase: false);
					}
				}
			}
			return GUIUtils.GetText(343);
		}
		case IndexableStat.ABILITIES:
		{
			if (stats == null || stats.Abilities == null)
			{
				return GUIUtils.GetText(343);
			}
			StringBuilder stringBuilder = new StringBuilder();
			GenericAbility genericAbility = null;
			for (int i = 0; i < stats.Abilities.Count; i++)
			{
				genericAbility = stats.Abilities[i];
				if (!(genericAbility == null) && !genericAbility.HideFromUi)
				{
					stringBuilder.AppendLine(genericAbility.Name());
				}
			}
			if (stringBuilder.Length == 0)
			{
				return GUIUtils.GetText(343);
			}
			return stringBuilder.ToString().Trim();
		}
		case IndexableStat.RACE:
			return GUIUtils.GetRaceString(stats.CharacterRace, stats.Gender);
		default:
			return stats.GetPropertyByIndex(stat);
		}
	}

	public object GetFormat(Type formatType)
	{
		return this;
	}

	public string Format(string format, object arg, IFormatProvider formatProvider)
	{
		BestiaryReference bestiaryReference = arg as BestiaryReference;
		if ((bool)bestiaryReference)
		{
			for (int i = 0; i < m_IndexableNames.Length; i++)
			{
				if (m_IndexableNames[i].ToLower().Equals(format.ToLower().Trim()))
				{
					return bestiaryReference.GetStatByIndex((IndexableStat)i, null).ToString();
				}
			}
			Debug.LogError("Format parameter '" + format + "' not recognized.");
			return "";
		}
		throw new InvalidOperationException("Tried to use BestiaryFormatter to format an object that wasn't a BestiaryReference.");
	}

	public string GetTreeListDisplayName()
	{
		return GetStatByIndex(IndexableStat.NAME, null).ToString();
	}
}
