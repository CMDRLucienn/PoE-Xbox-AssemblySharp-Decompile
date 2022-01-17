using UnityEngine;

public class UIAudioList : ScriptableObject
{
	public enum UIAudioType
	{
		None,
		ButtonDown,
		ButtonUp,
		PickInventoryItem,
		DropInventoryItem,
		AddSpellGrimoire,
		RemoveSpellGrimoire,
		AddSpellGeneric,
		RemoveSpellGeneric,
		AddChant,
		RemoveChant,
		ReceiveGold,
		RemoveGold,
		LevelUp,
		QuestRecieved,
		QuestUpdated,
		QuestComplete,
		QuestFailed,
		Rest,
		StealthOn,
		StealthOff,
		PartyDead,
		ItemCrafted,
		Back,
		Trade,
		Check,
		Uncheck,
		FinalSelect,
		Hover,
		Increment,
		Decrement,
		ChangeWeaponSet,
		WindowShowJournal,
		ChangeWeaponSetShield,
		LockpickSuccess,
		LockpickFailed,
		StealthForceBreak,
		StealthSuspicious,
		TrapDiscovered,
		TakeAll,
		HiddenDiscovered,
		TutorialShown,
		TutorialHidden,
		ItemEnchanted,
		Error,
		ManagerRecruitCharacter,
		ManagerDismissCharacter,
		BindSoul,
		UnbindSoul,
		ItemCraftedScroll,
		ItemCraftedPotion
	}

	public UIAudioEntry[] AudioList = new UIAudioEntry[39]
	{
		new UIAudioEntry
		{
			UIType = UIAudioType.ButtonDown,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.ButtonUp,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.PickInventoryItem,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.DropInventoryItem,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.AddSpellGrimoire,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.RemoveSpellGrimoire,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.AddSpellGeneric,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.RemoveSpellGeneric,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.AddChant,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.RemoveChant,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.ReceiveGold,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.RemoveGold,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.LevelUp,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.QuestRecieved,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.QuestUpdated,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.QuestComplete,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.QuestFailed,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Rest,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.StealthOn,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.StealthOff,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.PartyDead,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.ItemCrafted,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Back,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Trade,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Check,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Uncheck,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.FinalSelect,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Hover,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Increment,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.Decrement,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.ChangeWeaponSet,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.WindowShowJournal,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.ChangeWeaponSetShield,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.LockpickSuccess,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.LockpickFailed,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.StealthForceBreak,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.StealthSuspicious,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.TrapDiscovered,
			Clips = null
		},
		new UIAudioEntry
		{
			UIType = UIAudioType.TakeAll,
			Clips = null
		}
	};

	public UIWeaponSetAudioEntry[] WeaponChangeList;

	public UIWeaponHitAudioEntry[] WeaponHitList;

	public UIItemPickupAudioEntry[] ItemPickupList;

	public UIItemDropAudioEntry[] ItemDropList;

	public UIEquipAudioEntry[] EquipList;

	public UIStrongholdAudioEntry[] StrongholdList;

	public UIUseAudioEntry[] UseList;

	public UIHudWindowEntry[] WindowOpenList;

	public UIHudWindowEntry[] WindowCloseList;

	private void OnEnable()
	{
		UIAudioEntry[] audioList = AudioList;
		for (int i = 0; i < audioList.Length; i++)
		{
			audioList[i].UpdateName();
		}
	}
}
