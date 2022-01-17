using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class UIPartyMemberStatIconGetter : UIParentSelectorListener
{
	public enum FetchableStat
	{
		PRIMARY_DAMAGE,
		PRIMARY_DAMAGE_BEST,
		SECONDARY_DAMAGE,
		SECONDARY_DAMAGE_BEST,
		DT
	}

	public FetchableStat Stat;

	public DamagePacket.DamageType Damage = DamagePacket.DamageType.None;

	private CharacterStats m_NeedsReload;

	private UIActionBarTooltipTrigger m_TooltipTrigger;

	public static Color GetIconColor(string icon)
	{
		return icon switch
		{
			"CS_Deflection" => new Color(57f / 85f, 69f / 85f, 1f), 
			"CS_Fortitude" => new Color(1f, 0.854901969f, 0.403921574f), 
			"CS_Reflex" => new Color(26f / 51f, 1f, 0.772549033f), 
			"CS_Will" => new Color(0.8392157f, 47f / 85f, 1f), 
			_ => Color.white, 
		};
	}

	public static string GetDefenseTypeSprite(CharacterStats.DefenseType defense)
	{
		return defense switch
		{
			CharacterStats.DefenseType.Deflect => "CS_Deflection", 
			CharacterStats.DefenseType.Fortitude => "CS_Fortitude", 
			CharacterStats.DefenseType.Reflex => "CS_Reflex", 
			CharacterStats.DefenseType.Will => "CS_Will", 
			_ => "CS_Defenses", 
		};
	}

	public static string GetDamageTypeSprite(DamagePacket.DamageType damage)
	{
		return damage switch
		{
			DamagePacket.DamageType.Pierce => "CS_Pierce", 
			DamagePacket.DamageType.Crush => "CS_Blunt", 
			DamagePacket.DamageType.Slash => "CS_Slash", 
			DamagePacket.DamageType.Burn => "CS_Burn", 
			DamagePacket.DamageType.Corrode => "CS_Corrosive", 
			DamagePacket.DamageType.Shock => "CS_Shock", 
			DamagePacket.DamageType.Freeze => "CS_Freeze", 
			_ => "CS_Damage", 
		};
	}

	private void Awake()
	{
		m_TooltipTrigger = GetComponent<UIActionBarTooltipTrigger>();
		if (!m_TooltipTrigger)
		{
			m_TooltipTrigger = base.gameObject.AddComponent<UIActionBarTooltipTrigger>();
		}
	}

	private void LateUpdate()
	{
		if ((bool)m_NeedsReload)
		{
			ReloadCharacter(m_NeedsReload);
			m_NeedsReload = null;
		}
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_NeedsReload = stats;
	}

	private void ReloadCharacter(CharacterStats stats)
	{
		Equipment component = stats.GetComponent<Equipment>();
		UISprite component2 = GetComponent<UISprite>();
		DamagePacket.DamageType damageType = DamagePacket.DamageType.None;
		switch (Stat)
		{
		case FetchableStat.PRIMARY_DAMAGE:
			if ((bool)component && (bool)component.PrimaryAttack)
			{
				damageType = component.PrimaryAttack.DamageData.Type;
			}
			break;
		case FetchableStat.PRIMARY_DAMAGE_BEST:
			if ((bool)component && (bool)component.PrimaryAttack && component.PrimaryAttack.DamageData.BestOfType != DamagePacket.DamageType.None)
			{
				damageType = component.PrimaryAttack.DamageData.BestOfType;
			}
			break;
		case FetchableStat.SECONDARY_DAMAGE:
			if ((bool)component && (bool)component.SecondaryAttack)
			{
				damageType = component.SecondaryAttack.DamageData.Type;
			}
			break;
		case FetchableStat.SECONDARY_DAMAGE_BEST:
			if ((bool)component && (bool)component.SecondaryAttack && component.SecondaryAttack.DamageData.BestOfType != DamagePacket.DamageType.None)
			{
				damageType = component.SecondaryAttack.DamageData.BestOfType;
			}
			break;
		case FetchableStat.DT:
			damageType = Damage;
			break;
		}
		if (damageType != DamagePacket.DamageType.None)
		{
			component2.alpha = 1f;
			component2.spriteName = GetDamageTypeSprite(damageType);
			if ((bool)m_TooltipTrigger)
			{
				m_TooltipTrigger.Text = new GUIDatabaseString(GUIUtils.GetDamageTypeID(damageType));
			}
		}
		else
		{
			component2.alpha = 0f;
		}
	}
}
