using System;

[Serializable]
public class CompanionInstanceID : InstanceID
{
	public CompanionNames.Companions Companion;

	private static Guid[] companionGuids = new Guid[16]
	{
		SpecialCharacterInstanceID.InvalidGuid,
		SpecialCharacterInstanceID.PartyMemberGuid00,
		SpecialCharacterInstanceID.PartyMemberGuid01,
		SpecialCharacterInstanceID.PartyMemberGuid02,
		SpecialCharacterInstanceID.PartyMemberGuid03,
		SpecialCharacterInstanceID.PartyMemberGuid04,
		SpecialCharacterInstanceID.PartyMemberGuid05,
		SpecialCharacterInstanceID.PartyMemberGuid06,
		SpecialCharacterInstanceID.PartyMemberGuid07,
		SpecialCharacterInstanceID.PartyMemberGuid08,
		SpecialCharacterInstanceID.PartyMemberGuid09,
		SpecialCharacterInstanceID.PartyMemberGuid10,
		SpecialCharacterInstanceID.PartyMemberGuid11,
		SpecialCharacterInstanceID.PartyMemberGuid12,
		SpecialCharacterInstanceID.PartyMemberGuid13,
		SpecialCharacterInstanceID.PartyMemberGuid14
	};

	protected override void OnEnable()
	{
		base.Guid = GetCompanionGuid();
		UniqueID = base.Guid.ToString();
		base.OnEnable();
	}

	protected override void Awake()
	{
		if (string.IsNullOrEmpty(UniqueID))
		{
			base.Guid = GetCompanionGuid();
			UniqueID = base.Guid.ToString();
		}
	}

	public string GetCompanionUniqueID()
	{
		return GetCompanionGuid().ToString();
	}

	public Guid GetCompanionGuid()
	{
		if (Companion == CompanionNames.Companions.Invalid)
		{
			return new Guid(UniqueID);
		}
		return companionGuids[(int)Companion];
	}

	public static Guid GetSpecialGuid(CompanionNames.Companions name)
	{
		if (name == CompanionNames.Companions.Invalid)
		{
			return Guid.NewGuid();
		}
		return companionGuids[(int)name];
	}
}
