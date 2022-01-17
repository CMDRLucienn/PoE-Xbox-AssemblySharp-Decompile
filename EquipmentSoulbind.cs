using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Equippable))]
public class EquipmentSoulbind : MonoBehaviour
{
	public const int DegenerateRequirementMultiplier = 5;

	[Tooltip("If set, this item can be bound but never unbound.")]
	public bool CannotUnbind;

	[Tooltip("A list of classes that can bind this weapon.")]
	public CharacterStats.Class[] BindableClasses;

	[Tooltip("An ordered list of unlockables.")]
	public SoulbindUnlock[] Unlocks;

	[ResourcesImageProperty]
	public string PencilSketch;

	private int m_UnlockLevel = -1;

	private int m_NextUnlockLevel;

	private bool m_UnlockNextWhenAllowed;

	[Persistent]
	public Guid BoundGuid = Guid.Empty;

	private Equippable m_Equippable;

	private AttackBase m_Attack;

	private GameObject m_SubscribedToCharacter;

	private CharacterStats m_SubscribedStats;

	[Persistent]
	public int UnlockLevel
	{
		get
		{
			return m_UnlockLevel;
		}
		set
		{
			m_UnlockLevel = value;
		}
	}

	[Persistent]
	public float UnlockProgress { get; private set; }

	[Persistent]
	public float DegenerateUnlockProgress { get; private set; }

	[Persistent]
	public bool WasLastUnlockDegenerate { get; private set; }

	[Persistent]
	public DatabaseString CachedBoundOwnerName { get; private set; }

	[Persistent]
	public string CachedBoundOwnerOverrideName { get; private set; }

	[Persistent]
	public CharacterStats.Class BoundClass { get; private set; }

	public bool IsBound => BoundGuid != Guid.Empty;

	public bool AreUnlocksComplete
	{
		get
		{
			if (Unlocks != null)
			{
				return m_NextUnlockLevel >= Unlocks.Length;
			}
			return false;
		}
	}

	public bool CurrentLevelCanBeUnlockedDegenerately
	{
		get
		{
			if (IsBound && !AreUnlocksComplete)
			{
				return Unlocks[m_NextUnlockLevel].CanBeUnlockedDegenerately;
			}
			return false;
		}
	}

	private bool IsWeapon => m_Equippable is Weapon;

	public bool CanBindClass(CharacterStats.Class cl)
	{
		if (BindableClasses.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < BindableClasses.Length; i++)
		{
			if (BindableClasses[i] == cl)
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		m_Equippable = GetComponent<Equippable>();
		m_Attack = GetComponent<AttackBase>();
		PartyMemberAI.OnPartyMemberPermaDeath += OnPartyMemberPermaDeath;
	}

	public void Restored()
	{
		RecalculateNextLevel();
	}

	private void OnDestroy()
	{
		Unbind();
		UnsubscribeStatEvents();
		PartyMemberAI.OnPartyMemberPermaDeath -= OnPartyMemberPermaDeath;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		TryPreUnlockNext();
		if (!GameState.InCombat && TimeController.IsSafeToPause && IsBound && !GameState.IsLoading && UIWindowManager.Instance.AllWindowsReplaceable())
		{
			TryUnlockNext();
		}
	}

	private void OnEquip(GameObject character)
	{
		UnsubscribeStatEvents();
		if ((bool)character)
		{
			m_SubscribedToCharacter = character;
			m_SubscribedStats = m_SubscribedToCharacter.GetComponent<CharacterStats>();
			Health component = m_SubscribedToCharacter.GetComponent<Health>();
			if ((bool)m_SubscribedStats)
			{
				m_SubscribedStats.OnDamageFinal += OnEquippedCharacterDealsDamage;
				m_SubscribedStats.OnPostDamageApplied += OnEquippedCharacterIsDealtDamage;
				m_SubscribedStats.OnCausedAffliction += OnCharacterCausedAffliction;
			}
			if ((bool)component)
			{
				component.OnKill += OnEquippedCharacterKill;
				component.OnRevived += OnEquippedCharacterRevived;
				component.OnDamageDealt += OnEquippedCharacterDealsDirectDamage;
			}
		}
	}

	private void OnUnequip(GameObject character)
	{
		UnsubscribeStatEvents();
	}

	private void OnPartyMemberPermaDeath(GameObject source, GameEventArgs args)
	{
		InstanceID instanceID = (source ? source.GetComponent<InstanceID>() : null);
		if (IsBound && (bool)instanceID && instanceID.Guid == BoundGuid)
		{
			CacheOwnerName();
			Unbind();
		}
	}

	private void UnsubscribeStatEvents()
	{
		if ((bool)m_SubscribedToCharacter)
		{
			CharacterStats component = m_SubscribedToCharacter.GetComponent<CharacterStats>();
			Health component2 = m_SubscribedToCharacter.GetComponent<Health>();
			if ((bool)component)
			{
				component.OnDamageFinal -= OnEquippedCharacterDealsDamage;
				component.OnPostDamageApplied -= OnEquippedCharacterIsDealtDamage;
				component.OnCausedAffliction -= OnCharacterCausedAffliction;
			}
			if ((bool)component2)
			{
				component2.OnKill -= OnEquippedCharacterKill;
				component2.OnRevived -= OnEquippedCharacterRevived;
				component2.OnDamageDealt -= OnEquippedCharacterDealsDirectDamage;
			}
			m_SubscribedToCharacter = null;
			m_SubscribedStats = null;
		}
	}

	private void OnEquippedCharacterDealsDamage(GameObject source, CombatEventArgs args)
	{
		if (!IsBound || AreUnlocksComplete || (IsWeapon && !(args.Damage.Attack == m_Attack)) || !args.Victim || Unlocks[m_NextUnlockLevel].UnlockStatType != SoulbindUnlockType.Damage || ComponentUtils.GetComponent<Faction>(args.Victim).IsInPlayerFaction)
		{
			return;
		}
		DegenerateUnlockProgress += args.Damage.FinalAdjustedDamage;
		if (!Unlocks[m_NextUnlockLevel].CheckOtherPrerequisites(args.Attacker, args.Victim, args.Damage))
		{
			return;
		}
		if (Unlocks[m_NextUnlockLevel].CheckDamageTypePrerequisite(args.Damage))
		{
			UnlockProgress += args.Damage.DTAdjustedDamage;
		}
		if (args.Damage == null)
		{
			return;
		}
		for (int i = 0; i < args.Damage.ProcDamage.Length; i++)
		{
			if (Unlocks[m_NextUnlockLevel].CheckDamageTypePrerequisite((DamagePacket.DamageType)i))
			{
				UnlockProgress += args.Damage.ProcDamage[i];
			}
		}
	}

	private void OnEquippedCharacterDealsDirectDamage(GameObject sender, GameEventArgs args)
	{
		if (!IsBound || AreUnlocksComplete || args.GenericData.Length < 2 || !(args.GenericData[1] is StatusEffect statusEffect) || statusEffect.EquipmentOrigin != m_Equippable || !(args.GenericData[0] is DamageInfo damage))
		{
			return;
		}
		GameObject owner = args.GameObjectData[0];
		if (Unlocks[m_NextUnlockLevel].UnlockStatType == SoulbindUnlockType.Damage && !ComponentUtils.GetComponent<Faction>(sender).IsInPlayerFaction)
		{
			DegenerateUnlockProgress += args.FloatData[0];
			if (Unlocks[m_NextUnlockLevel].CheckOtherPrerequisites(owner, sender, damage) && Unlocks[m_NextUnlockLevel].CheckDamageTypePrerequisite(damage))
			{
				UnlockProgress += args.FloatData[0];
			}
		}
	}

	private void OnEquippedCharacterIsDealtDamage(GameObject source, CombatEventArgs args)
	{
		if (!IsBound || AreUnlocksComplete || Unlocks[m_NextUnlockLevel].UnlockStatType != SoulbindUnlockType.TakeDamage)
		{
			return;
		}
		Faction component = ComponentUtils.GetComponent<Faction>(args.Attacker);
		if ((bool)component && component.IsInPlayerFaction)
		{
			return;
		}
		DegenerateUnlockProgress += args.Damage.FinalAdjustedDamage;
		if (!Unlocks[m_NextUnlockLevel].CheckOtherPrerequisites(args.Victim, args.Attacker, args.Damage))
		{
			return;
		}
		if (Unlocks[m_NextUnlockLevel].CheckDamageTypePrerequisite(args.Damage))
		{
			UnlockProgress += args.Damage.DTAdjustedDamage;
		}
		if (args.Damage == null)
		{
			return;
		}
		for (int i = 0; i < args.Damage.ProcDamage.Length; i++)
		{
			if (Unlocks[m_NextUnlockLevel].CheckDamageTypePrerequisite((DamagePacket.DamageType)i))
			{
				UnlockProgress += args.Damage.ProcDamage[i];
			}
		}
	}

	private void OnEquippedCharacterKill(GameObject source, GameEventArgs args)
	{
		DamageInfo damageInfo = ((args.GenericData != null) ? (args.GenericData[0] as DamageInfo) : null);
		GameObject target = args.GameObjectData[0];
		if (IsBound && !AreUnlocksComplete && (damageInfo == null || !IsWeapon || damageInfo.Attack == m_Attack) && !ComponentUtils.GetComponent<Faction>(target).IsInPlayerFaction && Unlocks[m_NextUnlockLevel].UnlockStatType == SoulbindUnlockType.Kills)
		{
			DegenerateUnlockProgress++;
			if (Unlocks[m_NextUnlockLevel].CheckPrerequisites(source, target, damageInfo))
			{
				UnlockProgress++;
			}
		}
	}

	private void OnEquippedCharacterRevived(GameObject source, GameEventArgs args)
	{
		if (IsBound && !AreUnlocksComplete && GameState.InCombat && Unlocks[m_NextUnlockLevel].UnlockStatType == SoulbindUnlockType.BeRevived)
		{
			UnlockProgress++;
		}
	}

	private void OnCharacterCausedAffliction(GameObject enemy, Affliction afflictionPrefab)
	{
		if (IsBound && !AreUnlocksComplete && Unlocks[m_NextUnlockLevel].UnlockStatType == SoulbindUnlockType.CauseAffliction && afflictionPrefab.Tag == Unlocks[m_NextUnlockLevel].AfflictionValue.Tag)
		{
			UnlockProgress++;
		}
	}

	private void SetUnlockLevel(int value)
	{
		bool areUnlocksComplete = AreUnlocksComplete;
		UnlockLevel = value;
		RecalculateNextLevel();
		if (!areUnlocksComplete && AreUnlocksComplete && (bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumSoulboundWeaponsFullyUnlocked);
		}
	}

	private void RecalculateNextLevel()
	{
		m_NextUnlockLevel = Unlocks.Length;
		for (int i = UnlockLevel + 1; i < Unlocks.Length; i++)
		{
			if (Unlocks[i].IsValidForClass(BoundClass))
			{
				m_NextUnlockLevel = i;
				break;
			}
		}
	}

	public void DebugLevelUp()
	{
		if (IsBound && !AreUnlocksComplete)
		{
			DoUnlockNext();
		}
	}

	private void TryPreUnlockNext()
	{
		if (AreUnlocksComplete || Unlocks == null)
		{
			return;
		}
		if ((bool)m_SubscribedStats)
		{
			SoulbindUnlockType unlockStatType = Unlocks[m_NextUnlockLevel].UnlockStatType;
			if (unlockStatType == SoulbindUnlockType.AttributeAtLeast)
			{
				UnlockProgress = Mathf.Max(UnlockProgress, m_SubscribedStats.GetAttributeScore(Unlocks[m_NextUnlockLevel].AttributeValue));
			}
		}
		if (Unlocks[m_NextUnlockLevel].UnlockStatType == SoulbindUnlockType.GlobalVariable)
		{
			m_UnlockNextWhenAllowed = GlobalVariables.Instance.GetVariable(Unlocks[m_NextUnlockLevel].UnlockGlobalVar) >= Unlocks[m_NextUnlockLevel].UnlockStatLevel;
			WasLastUnlockDegenerate = false;
		}
		else if (UnlockProgress >= (float)Unlocks[m_NextUnlockLevel].UnlockStatLevel)
		{
			m_UnlockNextWhenAllowed = true;
			WasLastUnlockDegenerate = false;
		}
		else if (CurrentLevelCanBeUnlockedDegenerately && DegenerateUnlockProgress >= (float)(Unlocks[m_NextUnlockLevel].UnlockStatLevel * 5))
		{
			m_UnlockNextWhenAllowed = true;
			WasLastUnlockDegenerate = true;
		}
	}

	private void TryUnlockNext()
	{
		if (!AreUnlocksComplete && m_UnlockNextWhenAllowed)
		{
			DoUnlockNext();
		}
	}

	private void DoUnlockNext()
	{
		SetUnlockLevel(m_NextUnlockLevel);
		UnlockProgress = 0f;
		DegenerateUnlockProgress = 0f;
		m_UnlockNextWhenAllowed = false;
		ItemMod[] modsToApply = Unlocks[UnlockLevel].ModsToApply;
		foreach (ItemMod mod in modsToApply)
		{
			m_Equippable.AttachItemMod(mod);
		}
		modsToApply = Unlocks[UnlockLevel].ModsToRemove;
		foreach (ItemMod mod2 in modsToApply)
		{
			m_Equippable.DestroyFirstMod(mod2);
		}
		if ((bool)Unlocks[UnlockLevel].OverrideAppearanceWith)
		{
			m_Equippable.RebuildAppearance();
		}
		if (!UIItemInspectManager.ReloadWindowsForObject(base.gameObject, soulbindUnlockMode: true))
		{
			UIItemInspectManager.ExamineSoulbindUnlock(this, m_Equippable.EquippedOwner);
		}
		TryUnlockNext();
	}

	private void CacheOwnerName()
	{
		GameObject objectByID = InstanceID.GetObjectByID(BoundGuid);
		if ((bool)objectByID)
		{
			CharacterStats component = objectByID.GetComponent<CharacterStats>();
			CachedBoundOwnerName = component.DisplayName;
			CachedBoundOwnerOverrideName = component.OverrideName;
			return;
		}
		StoredCharacterInfo storedCompanion = Stronghold.Instance.GetStoredCompanion(BoundGuid);
		if (storedCompanion != null)
		{
			CachedBoundOwnerName = null;
			CachedBoundOwnerOverrideName = storedCompanion.DisplayName;
		}
	}

	public void Bind(GameObject character)
	{
		Unbind();
		if (!character)
		{
			return;
		}
		InstanceID component = character.GetComponent<InstanceID>();
		if (!component)
		{
			Debug.LogError("'" + base.name + "': Can't soulbind to a character with no InstanceID ('" + character.name + "').");
			return;
		}
		BoundGuid = component.Guid;
		CharacterStats component2 = character.GetComponent<CharacterStats>();
		BoundClass = component2.CharacterClass;
		CacheOwnerName();
		if ((bool)GlobalAudioPlayer.Instance)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.BindSoul);
		}
		UIItemInspectManager.ReloadWindowsForObject(base.gameObject, soulbindUnlockMode: false);
	}

	public void Unbind()
	{
		if (!CannotUnbind && IsBound)
		{
			m_Equippable.ResetItemMods();
			UIItemInspectManager[] array = UnityEngine.Object.FindObjectsOfType<UIItemInspectManager>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Reload();
			}
			BoundGuid = Guid.Empty;
			SetUnlockLevel(-1);
			UnlockProgress = 0f;
			DegenerateUnlockProgress = 0f;
			CachedBoundOwnerName = null;
			CachedBoundOwnerOverrideName = null;
			if ((bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.UnbindSoul);
			}
			UIItemInspectManager.ReloadWindowsForObject(base.gameObject, soulbindUnlockMode: false);
		}
	}

	public Texture2D GetOverrideIconTexture()
	{
		for (int num = UnlockLevel; num >= 0; num--)
		{
			if ((bool)Unlocks[num].OverrideIconTexture)
			{
				return Unlocks[num].OverrideIconTexture;
			}
		}
		return null;
	}

	public Texture2D GetOverrideIconLargeTexture()
	{
		for (int num = UnlockLevel; num >= 0; num--)
		{
			if ((bool)Unlocks[num].OverrideIconLargeTexture)
			{
				return Unlocks[num].OverrideIconLargeTexture;
			}
		}
		return null;
	}

	public GameObject GetOverrideAppearance()
	{
		for (int num = UnlockLevel; num >= 0; num--)
		{
			if ((bool)Unlocks[num].OverrideAppearanceWith)
			{
				return Unlocks[num].OverrideAppearanceWith.gameObject;
			}
		}
		return null;
	}

	public string GetPencilSketch()
	{
		for (int num = UnlockLevel; num >= 0; num--)
		{
			if (!string.IsNullOrEmpty(Unlocks[num].OverridePencilSketch))
			{
				return Unlocks[num].OverridePencilSketch;
			}
		}
		return PencilSketch;
	}

	public string GetExtraDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i <= UnlockLevel; i++)
		{
			if (Unlocks[i].IsValidForClass(BoundClass) && Unlocks[i].LoreToAdd.IsValidString)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine(Unlocks[i].LoreToAdd.GetText());
			}
		}
		return stringBuilder.ToString();
	}

	public string GetProgressString(bool degenerate)
	{
		if (AreUnlocksComplete || !IsBound || Unlocks[m_NextUnlockLevel].UnlockStatType == SoulbindUnlockType.GlobalVariable)
		{
			return "";
		}
		int num = Mathf.RoundToInt((float)Unlocks[m_NextUnlockLevel].UnlockStatLevel * (degenerate ? 5f : 1f));
		int num2 = Mathf.Min(num, Mathf.FloorToInt(degenerate ? DegenerateUnlockProgress : UnlockProgress));
		return GUIUtils.Format(1731, GUIUtils.Format(451, num2, num));
	}

	public string GetNextRequirementString(bool degenerate)
	{
		return GetRequirementString(m_NextUnlockLevel, degenerate);
	}

	public string GetLastRequirementString()
	{
		for (int num = m_NextUnlockLevel - 1; num >= 0; num--)
		{
			if (Unlocks[num].IsValidForClass(BoundClass))
			{
				return GetRequirementString(num, WasLastUnlockDegenerate);
			}
		}
		return "";
	}

	public string GetRequirementString(int unlockLevel, bool degenerate)
	{
		if (unlockLevel < 0 || unlockLevel >= Unlocks.Length)
		{
			return GUIUtils.GetText(2116);
		}
		if (unlockLevel == 0 && (Unlocks[unlockLevel].UnlockStatLevel == 0 || !IsBound))
		{
			return GUIUtils.Format(2045, GUIUtils.GetText(2048));
		}
		if (Unlocks[unlockLevel].OverrideRequirementText.IsValidString && !degenerate)
		{
			SoulbindUnlock soulbindUnlock = Unlocks[unlockLevel];
			if (degenerate && !soulbindUnlock.CanBeUnlockedDegenerately)
			{
				return "";
			}
			string text = ((float)soulbindUnlock.UnlockStatLevel * (degenerate ? 5f : 1f)).ToString("#0");
			return StringUtility.Format(Unlocks[unlockLevel].OverrideRequirementText.GetText(), text);
		}
		SoulbindUnlock soulbindUnlock2 = Unlocks[unlockLevel];
		if (degenerate && !soulbindUnlock2.CanBeUnlockedDegenerately)
		{
			return "";
		}
		string text2 = ((float)soulbindUnlock2.UnlockStatLevel * (degenerate ? 5f : 1f)).ToString("#0");
		switch (soulbindUnlock2.UnlockStatType)
		{
		case SoulbindUnlockType.Damage:
		{
			string qualifierString3 = PrerequisiteData.GetQualifierString(degenerate ? null : Unlocks[unlockLevel].UnlockPrerequisites);
			string text5 = ((soulbindUnlock2.PrereqDamageType == DamagePacket.DamageType.None || soulbindUnlock2.PrereqDamageType == DamagePacket.DamageType.All) ? GUIUtils.Format(IsWeapon ? 2047 : 2321, text2, qualifierString3, m_Equippable.Name) : GUIUtils.Format(IsWeapon ? 2174 : 2322, text2, qualifierString3, GUIUtils.GetDamageTypeString(soulbindUnlock2.PrereqDamageType), m_Equippable.Name));
			if (soulbindUnlock2.PrereqCriticalHit)
			{
				text5 = GUIUtils.Format(2175, text5);
			}
			return GUIUtils.Format(2045, text5);
		}
		case SoulbindUnlockType.Kills:
		{
			string qualifierString2 = PrerequisiteData.GetQualifierString(degenerate ? null : Unlocks[unlockLevel].UnlockPrerequisites);
			string text4 = GUIUtils.Format(2046, text2, qualifierString2, m_Equippable.Name);
			if (soulbindUnlock2.PrereqCriticalHit)
			{
				text4 = GUIUtils.Format(2175, text4);
			}
			return GUIUtils.Format(2045, text4);
		}
		case SoulbindUnlockType.CauseAffliction:
			if (soulbindUnlock2.AfflictionValue != null)
			{
				return GUIUtils.Format(2045, GUIUtils.Format(2172, text2, soulbindUnlock2.AfflictionValue.Name(), m_Equippable.Name));
			}
			return "*ERROR*: CauseAffliction unlock needs Affliction assigned.";
		case SoulbindUnlockType.BeRevived:
			return GUIUtils.Format(2045, GUIUtils.Format(2173, text2));
		case SoulbindUnlockType.GlobalVariable:
			return "*ERROR*: GlobalVariable unlocks must override display string.";
		case SoulbindUnlockType.TakeDamage:
		{
			string qualifierString = PrerequisiteData.GetQualifierString(degenerate ? null : Unlocks[unlockLevel].UnlockPrerequisites);
			string text3 = ((soulbindUnlock2.PrereqDamageType == DamagePacket.DamageType.None || soulbindUnlock2.PrereqDamageType == DamagePacket.DamageType.All) ? GUIUtils.Format(2319, text2, qualifierString) : GUIUtils.Format(2320, text2, qualifierString, GUIUtils.GetDamageTypeString(soulbindUnlock2.PrereqDamageType)));
			if (soulbindUnlock2.PrereqCriticalHit)
			{
				text3 = GUIUtils.Format(2175, text3);
			}
			return GUIUtils.Format(2045, text3);
		}
		case SoulbindUnlockType.AttributeAtLeast:
			return GUIUtils.Format(2329, Unlocks[unlockLevel].UnlockStatLevel, GUIUtils.GetAttributeScoreTypeString(Unlocks[unlockLevel].AttributeValue));
		default:
			return string.Concat("*ERROR*: Unrecognized unlock '", soulbindUnlock2.UnlockStatType, "'.");
		}
	}
}
