using System;
using System.Collections;
using System.IO;
using System.Text;
using MinigameData;
using UnityEngine;
using UnityEngine.Networking;

public static class GUIUtils
{
	public enum RelativeRating
	{
		VERY_LOW,
		LOW,
		AVERAGE,
		HIGH,
		VERY_HIGH
	}

	private static readonly int[] GenderIDs = new int[3] { 80, 81, 321 };

	private static readonly int[] GenderDescriptionIDs = new int[3] { 467, 468, 248 };

	private static readonly int[] AttributeScoreIDs = new int[6] { 44, 45, 46, 47, 273, 274 };

	private static readonly int[] AttributeScoreShortIDs = new int[6] { 317, 312, 313, 316, 314, 315 };

	private static readonly int[] AttributeScoreDescriptionIDs = new int[6] { 469, 470, 471, 472, 473, 474 };

	private static readonly int[] DefenseTypeIDs = new int[4] { 40, 41, 42, 43 };

	private static readonly int[] DefenseTypeShortIDs = new int[4] { 226, 227, 228, 229 };

	private static readonly int[] DamageTypeIDs = new int[11]
	{
		69, 70, 71, 72, 73, 74, 75, -1, -1, 1319,
		1325
	};

	private static readonly int[] CombatLogDamageTypeIDs = new int[11]
	{
		815, 816, 817, 818, 819, 820, 821, -1, -1, 1319,
		1325
	};

	private static readonly int[] CombatLogAttackRollTypeIDs = new int[4] { 811, 812, 813, 814 };

	private static readonly int[] SkillTypeIDs = new int[6] { 34, 35, 36, 37, 38, 39 };

	private static readonly int[] SkillTypeDescriptionIDs = new int[6] { 1396, 1397, 1398, 1399, 1400, 343 };

	private static readonly int[] RaceIDs = new int[15]
	{
		248, 82, 83, 84, 87, 86, 324, 85, 322, 323,
		429, 430, 431, 432, 433
	};

	private static readonly int[] RaceDescriptionIDs = new int[8] { 248, 475, 476, 477, 478, 479, 248, 480 };

	private static readonly int[] SubraceIDs = new int[18]
	{
		248, 234, 235, 236, 237, 238, 239, 240, 241, 242,
		260, 243, 244, 245, 246, 247, 686, 2229
	};

	private static readonly int[] SubraceDescriptionIDs = new int[16]
	{
		248, 481, 482, 483, 484, 485, 486, 487, 488, 489,
		490, 491, 492, 493, 494, 495
	};

	private static readonly int[] ClassIDs = new int[12]
	{
		248, 249, 250, 251, 252, 253, 254, 255, 256, 257,
		258, 259
	};

	private static readonly int[] PluralClassIDs = new int[52]
	{
		248, 2060, 2061, 2062, 2063, 2064, 2065, 2066, 2067, 2068,
		2069, 2070, 2071, 2072, 2073, 2074, 2075, 2076, 2077, 2078,
		2079, 2080, 2081, 2082, 2083, 2084, 2085, 2086, 2087, 2088,
		2089, 2090, 2091, 2092, 2093, 2094, 2095, 2096, 2097, 2098,
		2099, 2100, 2101, 2102, 2103, 1440, 2105, 2106, 2107, 2108,
		-1, 2423
	};

	private static readonly int[] ClassDescriptionIDs = new int[12]
	{
		248, 496, 497, 498, 499, 500, 501, 502, 503, 504,
		505, 506
	};

	private static readonly int[] CultureIDs = new int[12]
	{
		248, 331, 332, 333, 334, 335, 532, 533, 687, 688,
		689, 690
	};

	private static readonly int[] CultureDescriptionIDs = new int[8] { 248, 507, 508, 509, 510, 511, 513, 514 };

	private static readonly int[] BackgroundIDs = new int[23]
	{
		248, 534, 535, 536, 537, 538, 539, 540, 541, 542,
		543, 544, 545, 546, 547, 550, 548, 549, 1721, 1722,
		1723, 1724, 2059
	};

	private static readonly int[] BackgroundDescriptionIDs = new int[22]
	{
		248, 515, 516, 517, 518, 519, 520, 521, 522, 523,
		524, 525, 526, 527, 528, 531, 529, 530, -1, -1,
		-1, -1
	};

	private static readonly int[] DeityDescriptionIDs = new int[6] { 248, 723, 724, 725, 726, 727 };

	private static readonly int[] PaladinOrderDescriptionIDs = new int[7] { 248, 739, 740, 741, 742, 743, 744 };

	private static readonly int[] DifficultyIDs = new int[5] { 154, 155, 156, 153, 2248 };

	private static readonly int[] ArmorCategoryIDs = new int[3] { 438, 439, 440 };

	private static readonly int[] ShieldCategoryIDs = new int[3] { 1422, 1423, 1424 };

	private static readonly int[] EquipmentSlotIDs = new int[14]
	{
		342, 1005, 1006, 1007, 1007, 1008, 1009, 1010, 1011, 875,
		1013, 1014, 1014, 343
	};

	private static readonly int[] NotReadyIDs = new int[17]
	{
		455, -1, 456, 457, -1, 458, 459, 460, 461, 462,
		975, -1, -1, 714, -1, 2233, 2234
	};

	private static readonly int[] AbilityPrereqIDs = new int[38]
	{
		1354, 1355, 1356, 1357, 1358, 1359, 1360, 1361, 1362, 1363,
		1364, 1365, 1366, 1372, 1367, 1368, 1369, 1370, 1371, 1455,
		-1, -1, 2207, -1, -1, 2025, -1, -1, -1, -1,
		-1, -1, -1, 1354, -1, -1, -1, -1
	};

	private static readonly int[] AbilityPrereqQualifierIDs = new int[38]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, 2433, -1, -1, -1, 2451, 2432, 2433, -1,
		-1, -1, 2432, -1, -1, -1, -1, 2055, -1, -1,
		-1, -1, -1, -1, 2430, -1, 2330, 2331
	};

	private static readonly int[] AbilityPrereqBaseQualifierIDs = new int[38]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		2058, 2056, -1, 2057, -1, 432, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1
	};

	private static readonly int[] WhyCantHireIDs = new int[6] { -1, 657, 658, 659, 660, 841 };

	private static readonly int[] WhyCantBuildIDs = new int[6] { -1, 665, 666, 667, 668, 669 };

	private static readonly int[] StrongholdVisitorIDs = new int[6] { 773, 774, 775, 776, 777, -1 };

	private static readonly int[] StrongholdStatIDs = new int[3] { -1, 1433, 1432 };

	private static readonly int[] StatusEffectTriggerTypeIDs = new int[14]
	{
		-1, 1545, 1546, 1547, 1548, 1549, 1550, 1551, 1553, 1552,
		1554, 1554, 2217, 1820
	};

	private static readonly int[] StrongholdAdventureTypeIDs = new int[7] { -1, 1307, 1308, 1309, 1310, 1311, -1 };

	private static readonly int[] AttackSpeedTypeIDs = new int[6] { -1, 1474, 1475, 1476, 1892, 1893 };

	private static readonly DatabaseString[] WeaponTypeIDs = new DatabaseString[29]
	{
		new DatabaseString(DatabaseString.StringTableType.Items, 0),
		new DatabaseString(DatabaseString.StringTableType.Items, 1),
		new DatabaseString(DatabaseString.StringTableType.Items, 2),
		new DatabaseString(DatabaseString.StringTableType.Items, 3),
		new DatabaseString(DatabaseString.StringTableType.Items, 4),
		new DatabaseString(DatabaseString.StringTableType.Items, 5),
		new DatabaseString(DatabaseString.StringTableType.Items, 6),
		new DatabaseString(DatabaseString.StringTableType.Items, 7),
		new DatabaseString(DatabaseString.StringTableType.Items, 8),
		new DatabaseString(DatabaseString.StringTableType.Items, 11),
		new DatabaseString(DatabaseString.StringTableType.Items, 12),
		new DatabaseString(DatabaseString.StringTableType.Items, 13),
		new DatabaseString(DatabaseString.StringTableType.Items, 15),
		new DatabaseString(DatabaseString.StringTableType.Items, 17),
		new DatabaseString(DatabaseString.StringTableType.Items, 18),
		new DatabaseString(DatabaseString.StringTableType.Items, 19),
		new DatabaseString(DatabaseString.StringTableType.Items, 10),
		new DatabaseString(DatabaseString.StringTableType.Items, 20),
		new DatabaseString(DatabaseString.StringTableType.Items, 21),
		new DatabaseString(DatabaseString.StringTableType.Items, 22),
		new DatabaseString(DatabaseString.StringTableType.Items, 210),
		new DatabaseString(DatabaseString.StringTableType.Items, 25),
		new DatabaseString(DatabaseString.StringTableType.Items, 23),
		new DatabaseString(DatabaseString.StringTableType.Items, 24),
		new DatabaseString(DatabaseString.StringTableType.Items, 26),
		new DatabaseString(DatabaseString.StringTableType.Items, 27),
		new DatabaseString(DatabaseString.StringTableType.Items, 29),
		new DatabaseString(DatabaseString.StringTableType.Items, 30),
		new GUIDatabaseString(1280)
	};

	private static readonly int[] InterruptScaleIDs = new int[8] { -1, 1568, 1569, 1570, 1953, 1571, 1572, 1573 };

	private static readonly int[] FactionRepStrengthIDs = new int[6] { -1, 1777, 1778, 1779, 1780, 1781 };

	private static readonly int[] ReputationAxisIDs = new int[2] { 1785, 1786 };

	private static readonly int[] RelativeRatingIDs = new int[5] { 1861, 1862, 1953, 1863, 1864 };

	private static readonly int[] ModTriggerModeIDs = new int[10] { -1, 1821, 1822, 1820, -1, 2127, 2128, 2217, 2261, 2324 };

	private static readonly int[] DozensResults = new int[6] { 2129, 2130, 2131, 2132, 2133, 2134 };

	private static readonly int[] OrlansResults = new int[7] { 2135, 2136, 2137, 2138, 2139, 2140, 2141 };

	private static readonly int[] AggressionTypeDescIds = new int[4] { 2160, 2161, 2162, 2163 };

	private static readonly int[] StorePageTitleIds = new int[4] { 746, 835, 745, 2176 };

	public static int GetDamageTypeID(DamagePacket.DamageType type)
	{
		return DamageTypeIDs[(int)type];
	}

	public static string GetText(int id)
	{
		return StringTableManager.GetText(DatabaseString.StringTableType.Gui, id);
	}

	public static string GetText(int id, Gender gender)
	{
		return StringTableManager.GetText(DatabaseString.StringTableType.Gui, id, gender);
	}

	public static string FormatWithLinks(int id, params object[] parameters)
	{
		try
		{
			return string.Format(GetTextWithLinks(id), parameters);
		}
		catch (FormatException)
		{
			return "";
		}
	}

	public static string Format(Gender gender, int id, params object[] parameters)
	{
		try
		{
			return string.Format(GetText(id, gender), parameters);
		}
		catch (FormatException)
		{
			return "";
		}
	}

	public static string Format(int id, params object[] parameters)
	{
		try
		{
			return string.Format(GetText(id), parameters);
		}
		catch (FormatException)
		{
			return "";
		}
	}

	public static string GetTextWithLinks(int id)
	{
		return GetTextWithLinks(StringTableManager.GetText(DatabaseString.StringTableType.Gui, id));
	}

	public static string GetTextWithLinks(string text)
	{
		if (Glossary.Instance != null)
		{
			text = Glossary.Instance.AddUrlTags(text);
		}
		return text;
	}

	public static string Seconds(float seconds)
	{
		return Format(211, seconds.ToString("#0.0"));
	}

	public static string Comma()
	{
		return GetText(2036);
	}

	public static string GetPlayerStatString(object statObject)
	{
		return GetStatString(statObject, CharacterStats.GetGender(GameState.s_playerCharacter));
	}

	public static string GetStatString(object statObject, Gender gender)
	{
		if (statObject == null)
		{
			return string.Empty;
		}
		if (statObject is CharacterStats.AttributeScoreType)
		{
			return GetAttributeScoreTypeString((CharacterStats.AttributeScoreType)statObject);
		}
		if (statObject is CharacterStats.DefenseType)
		{
			return GetDefenseTypeString((CharacterStats.DefenseType)statObject);
		}
		if (statObject is CharacterStats.SkillType)
		{
			return GetSkillTypeString((CharacterStats.SkillType)statObject);
		}
		if (statObject is Disposition.Axis)
		{
			return FactionUtils.GetDispositionAxisString((Disposition.Axis)statObject);
		}
		if (statObject is Disposition.Strength)
		{
			return GetDispositionStrengthString((Disposition.Strength)statObject);
		}
		if (statObject is CharacterStats.Class)
		{
			return GetClassString((CharacterStats.Class)statObject, gender);
		}
		if (statObject is CharacterStats.Race)
		{
			return GetRaceString((CharacterStats.Race)statObject, gender);
		}
		if (statObject is CharacterStats.Subrace)
		{
			return GetSubraceString((CharacterStats.Subrace)statObject, gender);
		}
		if (statObject is Gender)
		{
			return GetGenderString((Gender)statObject);
		}
		if (statObject is Religion.Deity)
		{
			return GetDeityString((Religion.Deity)statObject);
		}
		if (statObject is Religion.PaladinOrder)
		{
			return GetPaladinOrderString((Religion.PaladinOrder)statObject, gender);
		}
		if (statObject is CharacterStats.Culture)
		{
			return GetCultureString((CharacterStats.Culture)statObject, gender);
		}
		if (statObject is CharacterStats.Background)
		{
			return GetBackgroundString((CharacterStats.Background)statObject, gender);
		}
		if (statObject is Reputation.Axis)
		{
			return GetReputationAxisString((Reputation.Axis)statObject);
		}
		if (statObject is Reputation.ChangeStrength)
		{
			return GetReputationChangeStrengthString((Reputation.ChangeStrength)statObject);
		}
		if (statObject is FactionName)
		{
			return ReputationManager.Instance.GetReputation((FactionName)statObject).Name.GetText();
		}
		return statObject.ToString();
	}

	public static string GetGenderString(Gender gender)
	{
		return GetString(GenderIDs, (int)gender);
	}

	public static string GetGenderDescriptionString(Gender gender)
	{
		return GetString(GenderDescriptionIDs, (int)gender);
	}

	public static string GetAttributeScoreTypeString(CharacterStats.AttributeScoreType attributeScoreType)
	{
		return GetString(AttributeScoreIDs, (int)attributeScoreType);
	}

	public static string GetAttributeScoreTypeShortString(CharacterStats.AttributeScoreType attributeScoreType)
	{
		return GetString(AttributeScoreShortIDs, (int)attributeScoreType);
	}

	public static string GetAttributeScoreDescriptionString(CharacterStats.AttributeScoreType attributeScoreType)
	{
		return GetString(AttributeScoreDescriptionIDs, (int)attributeScoreType);
	}

	public static string GetDefenseTypeString(CharacterStats.DefenseType defenseType)
	{
		return GetString(DefenseTypeIDs, (int)defenseType);
	}

	public static string GetDefenseTypeShortString(CharacterStats.DefenseType defenseType)
	{
		return GetString(DefenseTypeShortIDs, (int)defenseType);
	}

	public static string GetDefenseTypeDescription(CharacterStats.DefenseType defenseType)
	{
		return defenseType switch
		{
			CharacterStats.DefenseType.Deflect => StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 86), 
			CharacterStats.DefenseType.Fortitude => StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 88), 
			CharacterStats.DefenseType.Will => StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 92), 
			CharacterStats.DefenseType.Reflex => StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 90), 
			_ => "", 
		};
	}

	public static string GetDamageTypeString(DamagePacket.DamageType damageType)
	{
		return GetString(DamageTypeIDs, (int)damageType);
	}

	public static string GetCombatLogDamageTypeString(DamagePacket.DamageType damageType)
	{
		return GetString(CombatLogDamageTypeIDs, (int)damageType);
	}

	public static string GetCombatLogAttackRollString(HitType hitType)
	{
		return GetString(CombatLogAttackRollTypeIDs, (int)hitType);
	}

	public static string GetSkillTypeString(CharacterStats.SkillType skillType)
	{
		return GetString(SkillTypeIDs, (int)skillType);
	}

	public static string GetSkillTypeDescriptionString(CharacterStats.SkillType skillType)
	{
		return GetString(SkillTypeDescriptionIDs, (int)skillType);
	}

	public static string GetRaceString(CharacterStats.Race race, Gender gender)
	{
		return GetString(RaceIDs, (int)race, gender);
	}

	public static string GetPlayerRaceString(CharacterStats.Race race)
	{
		return GetString(RaceIDs, (int)race);
	}

	public static string GetRaceDescriptionString(CharacterStats.Race race, Gender gender)
	{
		return GetString(RaceDescriptionIDs, (int)race, gender);
	}

	public static string GetPlayerRaceDescriptionString(CharacterStats.Race race)
	{
		return GetString(RaceDescriptionIDs, (int)race);
	}

	public static string GetSubraceString(CharacterStats.Subrace subrace, Gender gender)
	{
		return GetString(SubraceIDs, (int)subrace, gender);
	}

	public static string GetPlayerSubraceString(CharacterStats.Subrace subrace)
	{
		return GetString(SubraceIDs, (int)subrace);
	}

	public static string GetSubraceDescriptionString(CharacterStats.Subrace subrace, Gender gender)
	{
		return GetString(SubraceDescriptionIDs, (int)subrace, gender);
	}

	public static string GetPlayerSubraceDescriptionString(CharacterStats.Subrace subrace)
	{
		return GetString(SubraceDescriptionIDs, (int)subrace);
	}

	public static string GetClassString(CharacterStats.Class characterClass, Gender gender)
	{
		return GetString(ClassIDs, (int)characterClass, gender);
	}

	public static string GetPluralClassString(CharacterStats.Class characterClass, Gender gender)
	{
		return GetString(PluralClassIDs, (int)characterClass, gender);
	}

	public static string GetClassDescriptionString(CharacterStats.Class characterClass, Gender gender)
	{
		return GetString(ClassDescriptionIDs, (int)characterClass, gender);
	}

	public static string GetPlayerClassDescriptionString(CharacterStats.Class characterClass)
	{
		return GetString(ClassDescriptionIDs, (int)characterClass);
	}

	public static string GetCultureString(CharacterStats.Culture culture, Gender gender)
	{
		return GetString(CultureIDs, (int)culture, gender);
	}

	public static string GetPlayerCultureString(CharacterStats.Culture culture)
	{
		return GetString(CultureIDs, (int)culture);
	}

	public static string GetCultureDescriptionString(CharacterStats.Culture culture, Gender gender)
	{
		return GetString(CultureDescriptionIDs, (int)culture, gender);
	}

	public static string GetPlayerCultureDescriptionString(CharacterStats.Culture culture)
	{
		return GetString(CultureDescriptionIDs, (int)culture);
	}

	public static string GetBackgroundString(CharacterStats.Background background, Gender gender)
	{
		return GetString(BackgroundIDs, (int)background, gender);
	}

	public static string GetPlayerBackgroundString(CharacterStats.Background background)
	{
		return GetString(BackgroundIDs, (int)background);
	}

	public static string GetBackgroundDescriptionString(CharacterStats.Background background, Gender gender)
	{
		return GetString(BackgroundDescriptionIDs, (int)background, gender);
	}

	public static string GetPlayerBackgroundDescriptionString(CharacterStats.Background background)
	{
		return GetString(BackgroundDescriptionIDs, (int)background);
	}

	public static string GetArmorCategoryString(Armor.Category category)
	{
		return GetString(ArmorCategoryIDs, (int)category);
	}

	public static string GetShieldCategoryString(Shield.Category category)
	{
		return GetString(ShieldCategoryIDs, (int)category);
	}

	public static string GetEquipmentSlotString(Equippable.EquipmentSlot slot)
	{
		return GetString(EquipmentSlotIDs, (int)slot);
	}

	public static string GetNotReadyString(GenericAbility.NotReadyValue value, Gender gender)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 11; i++)
		{
			int num = 1 << i;
			if ((int)((uint)value & (uint)num) > 0 && num != 2 && num != 16)
			{
				stringBuilder.AppendLine(GetText(NotReadyIDs[i], gender));
			}
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static string GetAbilityPrereqString(PrerequisiteType prereq)
	{
		return GetString(AbilityPrereqIDs, (int)prereq);
	}

	public static string GetAbilityPrereqQualifierString(PrerequisiteType prereq)
	{
		if (AbilityPrereqQualifierIDs[(int)prereq] >= 0)
		{
			return GetString(AbilityPrereqQualifierIDs, (int)prereq);
		}
		return null;
	}

	public static string GetAbilityPrereqBaseQualifierString(PrerequisiteType prereq)
	{
		if (AbilityPrereqBaseQualifierIDs[(int)prereq] >= 0)
		{
			return GetString(AbilityPrereqBaseQualifierIDs, (int)prereq);
		}
		return null;
	}

	public static string GetWhyCantHireString(Stronghold.WhyCantHire co, Gender gender)
	{
		return GetString(WhyCantHireIDs, (int)co, gender);
	}

	public static string GetWhyCantBuildString(Stronghold.WhyCantBuild co)
	{
		return GetString(WhyCantBuildIDs, (int)co);
	}

	public static string GetStrongholdVisitorString(StrongholdVisitor.Type ty, Gender gender)
	{
		return GetString(StrongholdVisitorIDs, (int)ty, gender);
	}

	public static string GetStrongholdAventureTypeString(StrongholdAdventure.Type ty)
	{
		return GetString(StrongholdAdventureTypeIDs, (int)ty);
	}

	public static string GetStrongholdStatString(Stronghold.StatType ty)
	{
		return GetString(StrongholdStatIDs, (int)ty);
	}

	private static bool StringExists(int[] strings, int index)
	{
		if (index < strings.Length)
		{
			return strings[index] != -1;
		}
		return false;
	}

	private static string GetString(int[] strings, int index)
	{
		if (index < strings.Length)
		{
			return GetText(strings[index]);
		}
		return string.Empty;
	}

	private static string GetString(int[] strings, int index, Gender gender)
	{
		if (index < strings.Length)
		{
			return GetText(strings[index], gender);
		}
		return string.Empty;
	}

	public static IEnumerator LoadTexture2DFromPathCallback(string path, Action<Texture2D> callback)
	{
		path = "file://" + Path.Combine(Application.dataPath, path);
		using UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(path);
		yield return textureRequest.SendWebRequest();
		if (textureRequest.isNetworkError || textureRequest.isHttpError)
		{
			Debug.LogError("Error loading texture at path '" + path + "'\n" + textureRequest.error);
			callback?.Invoke(Texture2D.whiteTexture);
			yield break;
		}
		callback?.Invoke(DownloadHandlerTexture.GetContent(textureRequest));
	}

	public static string GetPaladinOrderString(Religion.PaladinOrder order, Gender gender)
	{
		Religion.PaladinOrderData paladinOrderData = Religion.Instance.FindPaladinOrderData(order);
		if (paladinOrderData != null)
		{
			return paladinOrderData.DisplayName.GetText(gender);
		}
		return GetText(343);
	}

	public static string GetDeityString(Religion.Deity deity)
	{
		Religion.DeityData deityData = Religion.Instance.FindDeityData(deity);
		if (deityData != null)
		{
			return deityData.DisplayName.GetText();
		}
		return GetText(343);
	}

	public static string GetPaladinOrderDescriptionString(Religion.PaladinOrder order, Gender gender)
	{
		return GetString(PaladinOrderDescriptionIDs, (int)order, gender);
	}

	public static string GetDifficultyString(GameDifficulty difficulty)
	{
		return GetString(DifficultyIDs, (int)difficulty);
	}

	public static string GetDeityDescriptionString(Religion.Deity deity)
	{
		return GetString(DeityDescriptionIDs, (int)deity);
	}

	public static string GetAttackSpeedTypeString(AttackBase.UIAttackSpeedType type)
	{
		return GetString(AttackSpeedTypeIDs, (int)type);
	}

	public static string GetWeaponTypeIDs(WeaponSpecializationData.WeaponType type)
	{
		if ((int)type < WeaponTypeIDs.Length)
		{
			return WeaponTypeIDs[(int)type].GetText();
		}
		return string.Empty;
	}

	public static string GetStatusEffectTriggerTypeString(StatusEffectTrigger.TriggerType type)
	{
		return GetString(StatusEffectTriggerTypeIDs, (int)type);
	}

	public static string GetInterruptScaleString(AttackData.InterruptScale type)
	{
		return GetString(InterruptScaleIDs, (int)type);
	}

	public static string GetFactionRepStrengthString(int strength)
	{
		return GetString(FactionRepStrengthIDs, strength);
	}

	public static bool ModTriggerModeStringExists(ItemMod.TriggerMode mode)
	{
		return StringExists(ModTriggerModeIDs, (int)mode);
	}

	public static string GetModTriggerModeString(ItemMod.TriggerMode mode)
	{
		return GetString(ModTriggerModeIDs, (int)mode);
	}

	public static string GetDozensResultString(Dozens.Result result)
	{
		return GetString(DozensResults, (int)result);
	}

	public static string GetOrlanResultString(OrlansHead.Result result)
	{
		return GetString(OrlansResults, (int)result);
	}

	public static string GetAggressionTypeDesc(AIController.AggressionType agg)
	{
		return GetString(AggressionTypeDescIds, (int)agg);
	}

	public static string GetStoreTypeTitle(UIStorePageType tab)
	{
		return GetString(StorePageTitleIds, (int)tab);
	}

	public static string GetDispositionStrengthString(Disposition.Strength type)
	{
		return type switch
		{
			Disposition.Strength.Minor => GetText(1782), 
			Disposition.Strength.Average => GetText(1783), 
			Disposition.Strength.Major => GetText(1784), 
			_ => "*UnknownDispositionStrength*", 
		};
	}

	public static string GetReputationAxisString(Reputation.Axis type)
	{
		return GetString(ReputationAxisIDs, (int)type);
	}

	public static string GetReputationChangeStrengthString(Reputation.ChangeStrength type)
	{
		return type switch
		{
			Reputation.ChangeStrength.VeryMinor => GetText(1787), 
			Reputation.ChangeStrength.Minor => GetText(1782), 
			Reputation.ChangeStrength.Average => GetText(1783), 
			Reputation.ChangeStrength.Major => GetText(1784), 
			Reputation.ChangeStrength.VeryMajor => GetText(1788), 
			_ => "*UnknownReputationChangeStrength*", 
		};
	}

	public static string FormatReputationChangeStrength(Reputation.ChangeStrength type, string factionName)
	{
		return type switch
		{
			Reputation.ChangeStrength.VeryMinor => Format(1297, factionName), 
			Reputation.ChangeStrength.Minor => Format(1297, factionName), 
			Reputation.ChangeStrength.Average => Format(1298, factionName), 
			Reputation.ChangeStrength.Major => Format(1299, factionName), 
			Reputation.ChangeStrength.VeryMajor => Format(1300, factionName), 
			_ => "*UnknownReputationChangeStrengthFormat*", 
		};
	}

	public static string GetRelativeRatingString(RelativeRating type)
	{
		return GetString(RelativeRatingIDs, (int)type);
	}

	public static string FormatSpellLevel(CharacterStats.Class spellClass, int spellLevel)
	{
		return Format(1485, Ordinal.Get(spellLevel), Format(1486, GetClassString(spellClass, Gender.Neuter)));
	}

	public static string FormatMasteredLevel(CharacterStats.Class spellClass)
	{
		return Format(2251, GetClassString(spellClass, Gender.Neuter));
	}
}
