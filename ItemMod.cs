using System.Text;
using UnityEngine;

public class ItemMod : MonoBehaviour, ITooltipContent
{
	public enum TriggerMode
	{
		OnUI,
		OnScoringCriticalHit,
		OnBeingCriticallyHit,
		OnUnconscious,
		AsSecondaryAttack,
		OnSpiritShift,
		OnScoringCritOrHit,
		OnBeingCritOrHit,
		OnStaminaBelowRatio,
		OnScoringKill
	}

	public enum EnchantCategory
	{
		None,
		Quality,
		Lashing,
		Slaying,
		Proofing,
		Attributes,
		Unique,
		WhiteForge
	}

	public enum TargetMode
	{
		Self,
		Enemy
	}

	public const int MaxModPerCategory = 1;

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.ItemMods);

	public EnchantCategory ModEnchantCategory;

	public int Cost;

	public bool ShowEvenIfCostZero;

	public float FriendlyRadius;

	public bool CursesItem;

	[Tooltip("Status Effects applied to the person equipping the item.")]
	public StatusEffectParams[] StatusEffectsOnEquip;

	public GenericAbility[] AbilitiesToModOnEquip = new GenericAbility[1];

	public AbilityMod[] AbilityModsOnEquip;

	[Tooltip("A visual effect to apply to the item when it's equipped.")]
	public GameObject OnEquipVisualEffect;

	[Tooltip("Status effects to apply to the attacker when launching an attack with this item (only weapons and shields).")]
	public StatusEffectParams[] StatusEffectsOnLaunch;

	[Tooltip("Status effects to apply to the victim of the attack on impact (only weapons and shields).")]
	public StatusEffectParams[] StatusEffectsOnAttack;

	[Tooltip("Additional damage procs to add to the weapon attack (only weapons and shields).")]
	public DamagePacket.DamageProcType[] DamageProcs;

	public GenericAbility AbilityPrefab;

	public TriggerMode AbilityTriggeredOn;

	public float AbilityTriggerValue;

	public TargetMode AbilityTarget;

	[Tooltip("The chance for this ability to trigger. Not supported for OnUI.")]
	[Range(0f, 1f)]
	public float AbilityTriggerChance = 1f;

	public static int MaximumModValue
	{
		get
		{
			if (GameUtilities.HasPX2())
			{
				return 14;
			}
			return 12;
		}
	}

	public bool IsAura => FriendlyRadius > 0f;

	public bool IsQualityMod => ModEnchantCategory == EnchantCategory.Quality;

	public bool Charged
	{
		get
		{
			if ((bool)AbilityPrefab)
			{
				return AbilityPrefab.CooldownType == GenericAbility.CooldownMode.Charged;
			}
			return false;
		}
	}

	private void OnDestroy()
	{
		Debug.LogError("ItemMod '" + base.name + "' is being destroyed! ItemMods are prefabs and should never be destroyed.");
	}

	public string GetTooltipContent(GameObject owner)
	{
		return GetEffects(null, StatusEffectFormatMode.Default, null);
	}

	public string GetTooltipName(GameObject owner)
	{
		return DisplayName.GetText();
	}

	public Texture GetTooltipIcon()
	{
		return null;
	}

	public string GetEffects(ItemModComponent component, StatusEffectFormatMode mode, StringEffects stringEffects)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (CursesItem)
		{
			stringBuilder.AppendLine(GUIUtils.GetText(2158));
		}
		if (StatusEffectsOnEquip.Length != 0)
		{
			if (stringEffects != null)
			{
				StatusEffectParams.ListToStringEffects(StatusEffectsOnEquip, null, null, null, null, null, mode, GenericAbility.TARGET_SELF, "", AttackBase.TargetType.All, stringEffects);
			}
			else
			{
				string value = StatusEffectParams.ListToString(StatusEffectsOnEquip, null, null, null, null, mode, AttackBase.TargetType.All);
				if (!string.IsNullOrEmpty(value))
				{
					stringBuilder.AppendLine(value);
				}
			}
		}
		if (DamageProcs.Length != 0)
		{
			string text = "";
			for (int i = 0; i < DamageProcs.Length; i++)
			{
				if (DamageProcs[i] != null)
				{
					text += DamageProcs[i].ToString();
					if (i < DamageProcs.Length - 1)
					{
						text += GUIUtils.Comma();
					}
				}
			}
			if (text.Length > 0)
			{
				stringBuilder.AppendLine(text);
			}
		}
		if (AbilitiesToModOnEquip.Length != 0)
		{
			string text2 = "";
			for (int j = 0; j < AbilitiesToModOnEquip.Length; j++)
			{
				if (!(AbilitiesToModOnEquip[j] == null))
				{
					text2 += GenericAbility.Name(AbilitiesToModOnEquip[j]);
					if (j < AbilitiesToModOnEquip.Length - 1)
					{
						text2 += GUIUtils.Comma();
					}
				}
			}
			if (text2.Length > 0)
			{
				stringBuilder.AppendGuiFormat(1022, text2);
				stringBuilder.AppendLine();
			}
		}
		if (StatusEffectsOnLaunch.Length != 0)
		{
			string text3 = StatusEffectParams.ListToString(StatusEffectsOnLaunch, null, null, mode);
			if (text3.Length > 0)
			{
				stringBuilder.AppendLine(text3);
			}
		}
		if (StatusEffectsOnAttack.Length != 0)
		{
			string text4 = StatusEffectParams.ListToString(StatusEffectsOnAttack, null);
			if (text4.Length > 0)
			{
				stringBuilder.AppendGuiFormat(1025, text4);
				stringBuilder.AppendLine();
			}
		}
		if (AbilityPrefab != null)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			bool flag = false;
			string text5 = $"[url=ability://{AbilityPrefab.name}]{GenericAbility.Name(AbilityPrefab)}[/url]";
			if (GUIUtils.ModTriggerModeStringExists(AbilityTriggeredOn))
			{
				TriggerMode abilityTriggeredOn = AbilityTriggeredOn;
				text5 = ((abilityTriggeredOn != TriggerMode.OnStaminaBelowRatio) ? StringUtility.Format(GUIUtils.GetModTriggerModeString(AbilityTriggeredOn), text5) : StringUtility.Format(GUIUtils.GetModTriggerModeString(AbilityTriggeredOn), text5, GUIUtils.Format(1277, (AbilityTriggerValue * 100f).ToString("#0"))));
			}
			if (AbilityTriggerChance < 1f)
			{
				flag = true;
				text5 = GUIUtils.Format(2126, GUIUtils.Format(1277, (AbilityTriggerChance * 100f).ToString("#0")), text5);
			}
			stringBuilder2.Append(text5);
			string frequencyString = AbilityPrefab.GetFrequencyString();
			if (!string.IsNullOrEmpty(frequencyString))
			{
				stringBuilder2.Append(GUIUtils.Format(1731, frequencyString.Replace("\r\n", GUIUtils.Comma())));
			}
			if (!flag)
			{
				stringBuilder.AppendGuiFormat(1023, stringBuilder2.ToString());
				if ((bool)component)
				{
					if (component.Charged && (bool)component.ChargeAbility)
					{
						stringBuilder.AppendGuiFormat(1731, GUIUtils.Format(1924, component.ChargeAbility.UsesLeft()));
					}
				}
				else if (Charged && (bool)AbilityPrefab)
				{
					stringBuilder.AppendGuiFormat(1731, GUIUtils.Format(1924, AbilityPrefab.Cooldown));
				}
				stringBuilder.AppendLine();
			}
			else
			{
				stringBuilder.AppendLine(stringBuilder2.ToString());
			}
		}
		string text6 = "";
		if (IsAura)
		{
			text6 = GenericAbility.TARGET_FRIENDLY_AURA.GetText(AttackBase.TargetType.Friendly) + GUIUtils.Format(1731, GUIUtils.Format(1533, TextUtils.FormatBase(FriendlyRadius, FriendlyRadius, "#0.##"))) + ": ";
		}
		return text6 + stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		ItemMod itemMod = obj as ItemMod;
		if ((bool)itemMod)
		{
			return DisplayName.StringID.Equals(itemMod.DisplayName.StringID);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return DisplayName.StringID;
	}
}
