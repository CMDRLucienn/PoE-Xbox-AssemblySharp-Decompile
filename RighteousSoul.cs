using System;
using System.Collections.Generic;
using UnityEngine;

public class RighteousSoul : GenericAbility
{
	public List<string> ResistedKeywords = new List<string>();

	public List<Affliction> ResistedAfflictions = new List<Affliction>();

	public int ResistedDefenseAdj = 15;

	public float ResistedDurationAdj = -5f;

	private GameObject m_hookedTo;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			AttachListeners();
			m_permanent = true;
		}
	}

	protected override void OnDestroy()
	{
		DetachListeners();
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void Restored()
	{
		if (!base.IsLoaded && (GameState.LoadedGame || GameState.IsRestoredLevel))
		{
			bool activated = m_activated;
			m_activated = false;
			base.Restored();
			m_activated = activated;
			DetachListeners();
			AttachListeners();
		}
	}

	private void AttachListeners()
	{
		if (Owner != null)
		{
			CharacterStats component = Owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.OnDefenseAdjustment += AdjustStatusEffectDefense;
				component.OnAddStatusEffect += AddStatusEffect;
				m_hookedTo = Owner;
			}
		}
	}

	private void DetachListeners()
	{
		if (m_hookedTo != null)
		{
			CharacterStats component = m_hookedTo.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.OnDefenseAdjustment -= AdjustStatusEffectDefense;
				component.OnAddStatusEffect -= AddStatusEffect;
				m_hookedTo = null;
			}
		}
	}

	private void AdjustStatusEffectDefense(CharacterStats.DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary, ref int def_adj)
	{
		if (IsResistedAttack(attack, isSecondary))
		{
			def_adj += ResistedDefenseAdj;
		}
	}

	private bool IsResistedAttack(AttackBase attack, bool isSecondary)
	{
		if (attack == null)
		{
			return false;
		}
		if (isSecondary || attack.SecondaryDefense == CharacterStats.DefenseType.None)
		{
			foreach (Affliction resistedAffliction in ResistedAfflictions)
			{
				if (attack.HasAffliction(resistedAffliction))
				{
					return true;
				}
			}
		}
		foreach (string resistedKeyword in ResistedKeywords)
		{
			if (attack.HasKeyword(resistedKeyword))
			{
				return true;
			}
			if ((isSecondary || attack.SecondaryDefense == CharacterStats.DefenseType.None) && attack.HasAfflictionWithKeyword(resistedKeyword))
			{
				return true;
			}
		}
		return false;
	}

	private void AddStatusEffect(GameObject sender, StatusEffect effect, bool isFromAura)
	{
		if (m_ownerStats != null && IsResistedStatusEffect(effect))
		{
			m_ownerStats.AdjustStatusEffectDuration(effect, ResistedDurationAdj);
		}
	}

	private bool IsResistedStatusEffect(StatusEffect effect)
	{
		foreach (Affliction resistedAffliction in ResistedAfflictions)
		{
			if (effect.AfflictionOrigin == resistedAffliction)
			{
				return true;
			}
		}
		if (effect.AfflictionOrigin != null && effect.AfflictionKeyword != null)
		{
			foreach (string resistedKeyword in ResistedKeywords)
			{
				if (resistedKeyword.Equals(effect.AfflictionKeyword, StringComparison.Ordinal))
				{
					return true;
				}
			}
		}
		if (effect.AbilityOrigin == null)
		{
			return false;
		}
		AttackBase component = effect.AbilityOrigin.GetComponent<AttackBase>();
		if (component == null)
		{
			return false;
		}
		foreach (string resistedKeyword2 in ResistedKeywords)
		{
			if (component.HasKeyword(resistedKeyword2))
			{
				return true;
			}
			if (component.HasAfflictionWithKeyword(resistedKeyword2))
			{
				return true;
			}
		}
		return false;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = TextUtils.FuncJoin((Affliction a) => a.DisplayName.GetText(), ResistedAfflictions, ", ");
		string text2 = TextUtils.FuncJoin((string str) => KeywordData.GetAdjective(str).GetText(), ResistedKeywords, ", ");
		if (!string.IsNullOrEmpty(text))
		{
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + ", " + text2;
			}
		}
		else
		{
			text = text2;
		}
		string effect = GUIUtils.Format(1221, TextUtils.NumberBonus(ResistedDefenseAdj), text) + "," + GUIUtils.Format(1690, GUIUtils.Format(211, ResistedDurationAdj.ToString("0.#")), text);
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(effect, base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
