using System;
using UnityEngine;

public static class SpecialCharacterInstanceID
{
	public enum SpecialCharacterInstance
	{
		Invalid,
		Player,
		Owner,
		Speaker,
		This,
		User,
		Party,
		PartyAll,
		PartyAny,
		Slot0,
		Slot1,
		Slot2,
		Slot3,
		Slot4,
		Slot5,
		Specified0,
		SkillCheck0,
		SkillCheck1,
		SkillCheck2,
		SkillCheck3,
		SkillCheck4,
		SkillCheck5,
		Specified1,
		Specified2,
		Specified3,
		Specified4,
		Specified5
	}

	public enum SpecialCharacterScriptEvent
	{
		Invalid,
		Player,
		This,
		User,
		Party,
		Slot0,
		Slot1,
		Slot2,
		Slot3,
		Slot4,
		Slot5,
		Specified0,
		SkillCheck0,
		SkillCheck1,
		SkillCheck2,
		SkillCheck3,
		SkillCheck4,
		SkillCheck5,
		Specified1,
		Specified2,
		Specified3,
		Specified4,
		Specified5
	}

	public enum SpecialCharacterConditional
	{
		Invalid,
		Player,
		This,
		User,
		PartyAll,
		PartyAny,
		Slot0,
		Slot1,
		Slot2,
		Slot3,
		Slot4,
		Slot5,
		Specified0,
		SkillCheck0,
		SkillCheck1,
		SkillCheck2,
		SkillCheck3,
		SkillCheck4,
		SkillCheck5,
		Specified1,
		Specified2,
		Specified3,
		Specified4,
		Specified5
	}

	public enum SpecialCharacterDialog
	{
		Invalid,
		Player,
		Owner,
		Speaker,
		Party,
		Slot0,
		Slot1,
		Slot2,
		Slot3,
		Slot4,
		Slot5,
		Specified0,
		SkillCheck0,
		SkillCheck1,
		SkillCheck2,
		SkillCheck3,
		SkillCheck4,
		SkillCheck5,
		Specified1,
		Specified2,
		Specified3,
		Specified4,
		Specified5
	}

	public static Guid InvalidGuid = new Guid("12345678-1234-1234-1234-123456789abc");

	public static Guid PlayerGuid = new Guid("b1a8e901-0000-0000-0000-000000000000");

	public static Guid OwnerGuid = new Guid("011111e9-0000-0000-0000-000000000000");

	public static Guid SpeakerGuid = new Guid("5bea12e9-0000-0000-0000-000000000000");

	public static Guid ThisGuid = new Guid("7d150000-0000-0000-0000-000000000000");

	public static Guid UserGuid = new Guid("005e9000-0000-0000-0000-000000000000");

	public static Guid PartyGuid = new Guid("b1a7e000-0000-0000-0000-000000000000");

	public static Guid PartyAllGuid = new Guid("b1a7ea77-0000-0000-0000-000000000000");

	public static Guid PartyAnyGuid = new Guid("b1a7ea1e-0000-0000-0000-000000000000");

	public static Guid Slot0Guid = new Guid("51070000-0000-0000-0000-000000000000");

	public static Guid Slot1Guid = new Guid("51071000-0000-0000-0000-000000000000");

	public static Guid Slot2Guid = new Guid("51072000-0000-0000-0000-000000000000");

	public static Guid Slot3Guid = new Guid("51073000-0000-0000-0000-000000000000");

	public static Guid Slot4Guid = new Guid("51074000-0000-0000-0000-000000000000");

	public static Guid Slot5Guid = new Guid("51075000-0000-0000-0000-000000000000");

	public static Guid Specified0Guid = new Guid("4e3d0000-0000-0000-0000-000000000000");

	public static Guid Specified1Guid = new Guid("4e3d0001-0000-0000-0000-000000000000");

	public static Guid Specified2Guid = new Guid("4e3d0002-0000-0000-0000-000000000000");

	public static Guid Specified3Guid = new Guid("4e3d0003-0000-0000-0000-000000000000");

	public static Guid Specified4Guid = new Guid("4e3d0004-0000-0000-0000-000000000000");

	public static Guid Specified5Guid = new Guid("4e3d0005-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid00 = new Guid("b1a7e800-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid01 = new Guid("b1a7e801-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid02 = new Guid("b1a7e802-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid03 = new Guid("b1a7e803-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid04 = new Guid("b1a7e804-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid05 = new Guid("b1a7e805-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid06 = new Guid("b1a7e806-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid07 = new Guid("b1a7e807-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid08 = new Guid("b1a7e808-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid09 = new Guid("b1a7e809-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid10 = new Guid("b1a7e810-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid11 = new Guid("b1a7e811-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid12 = new Guid("b1a7e812-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid13 = new Guid("b1a7e813-0000-0000-0000-000000000000");

	public static Guid PartyMemberGuid14 = new Guid("b1a7e814-0000-0000-0000-000000000000");

	public static Guid SkillCheck0Guid = new Guid("6dcee000-0000-0000-0000-000000000000");

	public static Guid SkillCheck1Guid = new Guid("6dcee001-0000-0000-0000-000000000000");

	public static Guid SkillCheck2Guid = new Guid("6dcee002-0000-0000-0000-000000000000");

	public static Guid SkillCheck3Guid = new Guid("6dcee003-0000-0000-0000-000000000000");

	public static Guid SkillCheck4Guid = new Guid("6dcee004-0000-0000-0000-000000000000");

	public static Guid SkillCheck5Guid = new Guid("6dcee005-0000-0000-0000-000000000000");

	public static readonly int SpecialCharacterScriptEventLength = Enum.GetValues(typeof(SpecialCharacterScriptEvent)).Length;

	public static readonly int SpecialCharacterConditionalLength = Enum.GetValues(typeof(SpecialCharacterConditional)).Length;

	public static Guid[] s_specialIDs = new Guid[27]
	{
		InvalidGuid, PlayerGuid, OwnerGuid, SpeakerGuid, ThisGuid, UserGuid, PartyGuid, PartyAllGuid, PartyAnyGuid, Slot0Guid,
		Slot1Guid, Slot2Guid, Slot3Guid, Slot4Guid, Slot5Guid, Specified0Guid, SkillCheck0Guid, SkillCheck1Guid, SkillCheck2Guid, SkillCheck3Guid,
		SkillCheck4Guid, SkillCheck5Guid, Specified1Guid, Specified2Guid, Specified3Guid, Specified4Guid, Specified5Guid
	};

	public static SpecialCharacterInstance[] FromScriptEvent = new SpecialCharacterInstance[23]
	{
		SpecialCharacterInstance.Invalid,
		SpecialCharacterInstance.Player,
		SpecialCharacterInstance.This,
		SpecialCharacterInstance.User,
		SpecialCharacterInstance.Party,
		SpecialCharacterInstance.Slot0,
		SpecialCharacterInstance.Slot1,
		SpecialCharacterInstance.Slot2,
		SpecialCharacterInstance.Slot3,
		SpecialCharacterInstance.Slot4,
		SpecialCharacterInstance.Slot5,
		SpecialCharacterInstance.Specified0,
		SpecialCharacterInstance.SkillCheck0,
		SpecialCharacterInstance.SkillCheck1,
		SpecialCharacterInstance.SkillCheck2,
		SpecialCharacterInstance.SkillCheck3,
		SpecialCharacterInstance.SkillCheck4,
		SpecialCharacterInstance.SkillCheck5,
		SpecialCharacterInstance.Specified1,
		SpecialCharacterInstance.Specified2,
		SpecialCharacterInstance.Specified3,
		SpecialCharacterInstance.Specified4,
		SpecialCharacterInstance.Specified5
	};

	public static SpecialCharacterInstance[] FromConditional = new SpecialCharacterInstance[24]
	{
		SpecialCharacterInstance.Invalid,
		SpecialCharacterInstance.Player,
		SpecialCharacterInstance.This,
		SpecialCharacterInstance.User,
		SpecialCharacterInstance.PartyAll,
		SpecialCharacterInstance.PartyAny,
		SpecialCharacterInstance.Slot0,
		SpecialCharacterInstance.Slot1,
		SpecialCharacterInstance.Slot2,
		SpecialCharacterInstance.Slot3,
		SpecialCharacterInstance.Slot4,
		SpecialCharacterInstance.Slot5,
		SpecialCharacterInstance.Specified0,
		SpecialCharacterInstance.SkillCheck0,
		SpecialCharacterInstance.SkillCheck1,
		SpecialCharacterInstance.SkillCheck2,
		SpecialCharacterInstance.SkillCheck3,
		SpecialCharacterInstance.SkillCheck4,
		SpecialCharacterInstance.SkillCheck5,
		SpecialCharacterInstance.Specified1,
		SpecialCharacterInstance.Specified2,
		SpecialCharacterInstance.Specified3,
		SpecialCharacterInstance.Specified4,
		SpecialCharacterInstance.Specified5
	};

	public static Guid[] s_companionGuids = new Guid[15]
	{
		PartyMemberGuid00, PartyMemberGuid01, PartyMemberGuid02, PartyMemberGuid03, PartyMemberGuid04, PartyMemberGuid05, PartyMemberGuid06, PartyMemberGuid07, PartyMemberGuid08, PartyMemberGuid09,
		PartyMemberGuid10, PartyMemberGuid11, PartyMemberGuid12, PartyMemberGuid13, PartyMemberGuid14
	};

	public static string[] s_specialNames = new string[27]
	{
		"oei_invalid", "oei_player", "oei_owner", "oei_speaker", "oei_this", "oei_user", "oei_party", "oei_party_all", "oei_party_any", "oei_slot0",
		"oei_slot1", "oei_slot2", "oei_slot3", "oei_slot4", "oei_slot5", "oei_specified0", "oei_skillcheck0", "oei_skillcheck1", "oei_skillcheck2", "oei_skillcheck3",
		"oei_skillcheck4", "oei_skillcheck5", "oei_specified1", "oei_specified2", "oei_specified3", "oei_specified4", "oei_specified5"
	};

	public static SpecialCharacterInstance[] s_slotGuids = new SpecialCharacterInstance[6]
	{
		SpecialCharacterInstance.Slot0,
		SpecialCharacterInstance.Slot1,
		SpecialCharacterInstance.Slot2,
		SpecialCharacterInstance.Slot3,
		SpecialCharacterInstance.Slot4,
		SpecialCharacterInstance.Slot5
	};

	public static SpecialCharacterInstance[] s_skillCheckGuids = new SpecialCharacterInstance[6]
	{
		SpecialCharacterInstance.SkillCheck0,
		SpecialCharacterInstance.SkillCheck1,
		SpecialCharacterInstance.SkillCheck2,
		SpecialCharacterInstance.SkillCheck3,
		SpecialCharacterInstance.SkillCheck4,
		SpecialCharacterInstance.SkillCheck5
	};

	public static SpecialCharacterInstance[] s_specifiedGuids = new SpecialCharacterInstance[6]
	{
		SpecialCharacterInstance.Specified0,
		SpecialCharacterInstance.Specified1,
		SpecialCharacterInstance.Specified2,
		SpecialCharacterInstance.Specified3,
		SpecialCharacterInstance.Specified4,
		SpecialCharacterInstance.Specified5
	};

	public static bool SkillCheckDebugEnabled = false;

	public static void Add(GameObject obj, SpecialCharacterInstance type)
	{
		if (obj == null)
		{
			Remove(type);
		}
		else
		{
			InstanceID.AddSpecialObjectID(obj, s_specialIDs[(int)type]);
		}
	}

	public static void Add(Guid obj, SpecialCharacterInstance type)
	{
		if (obj != Guid.Empty && InstanceID.ObjectIsActive(obj))
		{
			Add(InstanceID.GetObjectByID(obj), type);
		}
		else
		{
			Add(null, type);
		}
	}

	public static void Remove(SpecialCharacterInstance type)
	{
		InstanceID.RemoveSpecialObjectID(s_specialIDs[(int)type]);
	}

	public static Guid GetSpecialGUID(SpecialCharacterInstance type)
	{
		return s_specialIDs[(int)type];
	}

	public static Guid GetSkillCheckGuid(int checkIndex)
	{
		return GetSpecialGUID(s_skillCheckGuids[checkIndex]);
	}

	public static Guid GetSpecifiedGuid(int index)
	{
		return GetSpecialGUID(s_specifiedGuids[index]);
	}

	public static SpecialCharacterInstance GetSpecialTypeFromGuid(Guid guid)
	{
		int num = Enum.GetNames(typeof(SpecialCharacterInstance)).Length;
		for (int i = 0; i < num; i++)
		{
			if (s_specialIDs[i] == guid)
			{
				return (SpecialCharacterInstance)i;
			}
		}
		return SpecialCharacterInstance.Invalid;
	}

	public static SpecialCharacterInstance Parse(string name)
	{
		for (int i = 0; i < s_specialNames.Length; i++)
		{
			if (s_specialNames[i] == name)
			{
				return (SpecialCharacterInstance)i;
			}
		}
		return SpecialCharacterInstance.Invalid;
	}

	public static string GetSkillCheckDebug()
	{
		string text = "Skill Check Tokens:";
		for (int i = 0; i < s_skillCheckGuids.Length; i++)
		{
			GameObject objectByID = InstanceID.GetObjectByID(GetSkillCheckGuid(i));
			text = text + "\n" + i + " | " + (objectByID ? objectByID.name : "*null*") + " | " + i;
		}
		return text;
	}
}
