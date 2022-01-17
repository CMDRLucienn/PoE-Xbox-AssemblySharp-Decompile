using System.Collections.Generic;
using UnityEngine;

[ClassTooltip("The owner recieves a BonusMeleeDamageMult with value (100 + <Bonus Melee Damage>)% while they are engaged by at least <Min Enemies For Bonus> enemies.")]
public class OneStandsAlone : GenericAbility
{
	public int MinEnemiesForBonus = 2;

	public int BonusMeleeDamage = 20;

	private StatusEffect m_meleeBonus;

	private StatusEffectParams m_meleeBonusParams = new StatusEffectParams();

	private const float UpdateInterval = 0.5f;

	private float m_updateTimer;

	private List<GameObject> m_enemiesInRange = new List<GameObject>();

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		m_updateTimer -= Time.deltaTime;
		if (!(m_updateTimer <= 0f))
		{
			return;
		}
		m_updateTimer = 0.5f;
		if (!(m_ownerStats != null))
		{
			return;
		}
		m_enemiesInRange.Clear();
		GameUtilities.GetEnemiesInRange(Owner, GameUtilities.FindActiveAIController(Owner), 1.5f, m_enemiesInRange);
		if (m_enemiesInRange.Count >= MinEnemiesForBonus)
		{
			if (m_meleeBonus == null)
			{
				UpdateEffectParams();
				m_meleeBonus = StatusEffect.Create(Owner, this, m_meleeBonusParams, AbilityType.Ability, null, deleteOnClear: true);
				m_ownerStats.ApplyStatusEffectImmediate(m_meleeBonus);
			}
		}
		else if (m_meleeBonus != null)
		{
			m_ownerStats.ClearEffect(m_meleeBonus);
			m_meleeBonus = null;
		}
	}

	private void UpdateEffectParams()
	{
		m_meleeBonusParams.AffectsStat = StatusEffect.ModifiedStat.BonusMeleeDamageMult;
		m_meleeBonusParams.DmgType = DamagePacket.DamageType.All;
		m_meleeBonusParams.Value = 1f + (float)BonusMeleeDamage / 100f;
		m_meleeBonusParams.LastsUntilCombatEnds = true;
		m_meleeBonusParams.Duration = 0f;
		m_meleeBonusParams.DontHideFromLog = true;
		m_meleeBonusParams.IsHostile = false;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		UpdateEffectParams();
		CharacterStats owner = (character ? character.GetComponent<CharacterStats>() : null);
		AttackBase.AddStringEffect(GetSelfTarget(), new AttackBase.AttackEffect(m_meleeBonusParams.GetString(m_meleeBonus, owner, ability), base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
