using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AutoPauseOptions
{
	[Flags]
	public enum PauseEvent
	{
		PartyMemberAttacked = 1,
		CharacterDown = 2,
		WeaponIneffective = 4,
		CombatTimer = 8,
		SpellCast = 0x10,
		CharacterDamaged = 0x20,
		CharacterAttacked = 0x40,
		TargetDestroyed = 0x80,
		HiddenObjectFound = 0x100,
		CombatStart = 0x200,
		MeleeEngaged = 0x400,
		ExtraordinaryDefence = 0x800,
		LowStamina = 0x1000,
		LowHealth = 0x2000,
		EnemySpotted = 0x4000,
		PartyMemberCastFinished = 0x8000,
		DebugEvent = 0x40000000
	}

	private const int ALLBITS = 65535;

	private const int ALLSLOW = 512;

	private const int DEFAULT_PAUSE = 512;

	private const int DEFAULT_SLOW = 0;

	private static Dictionary<PauseEvent, int> s_stringLookup = new Dictionary<PauseEvent, int>();

	private int m_pauseBitField = 512;

	private int m_slowBitField;

	private static GUIDatabaseString s_autoPauseString = null;

	private bool m_centerOnCharacter = true;

	private float m_combatRoundTime = 2f;

	private int m_AutoslowEnemyThreshold = 3;

	private bool m_combatStopMovement;

	private bool m_enemySpottedStopsMovement;

	public float CombatRoundTime
	{
		get
		{
			return m_combatRoundTime;
		}
		set
		{
			m_combatRoundTime = Mathf.Max(1f, value);
		}
	}

	public int AutoslowEnemyThreshold
	{
		get
		{
			return m_AutoslowEnemyThreshold;
		}
		set
		{
			m_AutoslowEnemyThreshold = value;
		}
	}

	public bool EnteringCombatStopsMovement
	{
		get
		{
			return m_combatStopMovement;
		}
		set
		{
			m_combatStopMovement = value;
		}
	}

	public bool EnemySpottedStopMovement
	{
		get
		{
			return m_enemySpottedStopsMovement;
		}
		set
		{
			m_enemySpottedStopsMovement = value;
		}
	}

	public bool CenterOnCharacter
	{
		get
		{
			return m_centerOnCharacter;
		}
		set
		{
			m_centerOnCharacter = value;
		}
	}

	public void Initialize()
	{
		if (s_autoPauseString == null)
		{
			s_autoPauseString = new GUIDatabaseString(188);
			s_stringLookup.Add(PauseEvent.PartyMemberAttacked, 173);
			s_stringLookup.Add(PauseEvent.CharacterDown, 174);
			s_stringLookup.Add(PauseEvent.WeaponIneffective, 175);
			s_stringLookup.Add(PauseEvent.CombatTimer, 176);
			s_stringLookup.Add(PauseEvent.SpellCast, 177);
			s_stringLookup.Add(PauseEvent.CharacterDamaged, 178);
			s_stringLookup.Add(PauseEvent.CharacterAttacked, 179);
			s_stringLookup.Add(PauseEvent.TargetDestroyed, 180);
			s_stringLookup.Add(PauseEvent.HiddenObjectFound, 181);
			s_stringLookup.Add(PauseEvent.CombatStart, 182);
			s_stringLookup.Add(PauseEvent.MeleeEngaged, 183);
			s_stringLookup.Add(PauseEvent.ExtraordinaryDefence, 184);
			s_stringLookup.Add(PauseEvent.LowStamina, 185);
			s_stringLookup.Add(PauseEvent.LowHealth, 186);
			s_stringLookup.Add(PauseEvent.EnemySpotted, 187);
			s_stringLookup.Add(PauseEvent.PartyMemberCastFinished, 1932);
		}
		m_pauseBitField = PlayerPrefs.GetInt("autopause", 512);
		m_slowBitField = PlayerPrefs.GetInt("autoslow", 0);
		m_centerOnCharacter = PlayerPrefs.GetInt("autopauseCenter", 0) > 0;
		m_enemySpottedStopsMovement = PlayerPrefs.GetInt("enemySpottedStopsMovement", 0) > 0;
		m_combatStopMovement = PlayerPrefs.GetInt("combatStopsMovement", 0) > 0;
		m_combatRoundTime = PlayerPrefs.GetFloat("combatRoundTime", 2f);
		m_AutoslowEnemyThreshold = PlayerPrefs.GetInt("autoslowEnemyThreshold", 3);
	}

	public void SaveOptions()
	{
		PlayerPrefs.SetInt("autopause", m_pauseBitField);
		PlayerPrefs.SetInt("autoslow", m_slowBitField);
		PlayerPrefs.SetFloat("combatRoundTime", m_combatRoundTime);
		PlayerPrefs.SetInt("autoslowEnemyThreshold", m_AutoslowEnemyThreshold);
		PlayerPrefs.SetInt("combatStopsMovement", m_combatStopMovement ? 1 : 0);
		PlayerPrefs.SetInt("enemySpottedStopsMovement", m_enemySpottedStopsMovement ? 1 : 0);
		PlayerPrefs.SetInt("autopauseCenter", m_centerOnCharacter ? 1 : 0);
	}

	public bool IsEventSet(PauseEvent evt)
	{
		if (((uint)m_pauseBitField & (uint)evt) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsSlowEventSet(PauseEvent evt)
	{
		if (((uint)m_slowBitField & (uint)evt) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsAllSet()
	{
		if ((~m_pauseBitField & 0xFFFF) == 0)
		{
			return (~m_slowBitField & 0x200) == 0;
		}
		return false;
	}

	public void SetEvent(PauseEvent evt, bool isActive)
	{
		int num = (int)evt;
		if (isActive)
		{
			m_pauseBitField |= num;
			return;
		}
		num = ~num;
		m_pauseBitField &= num;
	}

	public void SetSlowEvent(PauseEvent evt, bool isActive)
	{
		int num = (int)evt;
		if (isActive)
		{
			m_slowBitField |= num;
			return;
		}
		num = ~num;
		m_slowBitField &= num;
	}

	public void SetAll(bool isActive)
	{
		EnemySpottedStopMovement = isActive;
		EnteringCombatStopsMovement = isActive;
		if (isActive)
		{
			m_pauseBitField = 65535;
		}
		else
		{
			m_pauseBitField = 0;
		}
		if (isActive)
		{
			m_slowBitField = 512;
		}
		else
		{
			m_slowBitField = 0;
		}
	}

	public static string GetDisplayName(PauseEvent evt)
	{
		if (evt == PauseEvent.DebugEvent)
		{
			return "DEBUG AUTOPAUSE - use \"DebugEvents false\" console command to disable.";
		}
		if (s_stringLookup.ContainsKey(evt))
		{
			return GUIUtils.GetTextWithLinks(s_stringLookup[evt]);
		}
		return "*AutopauseError*";
	}

	public static string GetResponseString(PauseEvent evt, GameObject character, GenericAbility ability)
	{
		if (evt == PauseEvent.CombatTimer || character == null)
		{
			return StringUtility.Format("{0}: {1}", s_autoPauseString.GetTextWithLinks(), GetDisplayName(evt));
		}
		if (evt == PauseEvent.SpellCast && (bool)ability)
		{
			return StringUtility.Format("{0}: {1} ({2}, {3})", s_autoPauseString.GetTextWithLinks(), GetDisplayName(evt), CharacterStats.Name(character), GenericAbility.Name(ability));
		}
		return StringUtility.Format("{0}: {1} ({2})", s_autoPauseString.GetTextWithLinks(), GetDisplayName(evt), CharacterStats.Name(character));
	}

	public void CopyFrom(AutoPauseOptions other)
	{
		m_pauseBitField = other.m_pauseBitField;
		m_slowBitField = other.m_slowBitField;
		m_combatRoundTime = other.m_combatRoundTime;
		m_AutoslowEnemyThreshold = other.m_AutoslowEnemyThreshold;
		m_combatStopMovement = other.m_combatStopMovement;
		m_enemySpottedStopsMovement = other.m_enemySpottedStopsMovement;
		m_centerOnCharacter = other.m_centerOnCharacter;
	}

	public bool Matches(AutoPauseOptions other)
	{
		if (m_pauseBitField == other.m_pauseBitField && m_slowBitField == other.m_slowBitField && m_combatRoundTime == other.m_combatRoundTime && m_AutoslowEnemyThreshold == other.m_AutoslowEnemyThreshold && m_combatStopMovement == other.m_combatStopMovement && m_enemySpottedStopsMovement == other.m_enemySpottedStopsMovement)
		{
			return m_centerOnCharacter == other.m_centerOnCharacter;
		}
		return false;
	}
}
