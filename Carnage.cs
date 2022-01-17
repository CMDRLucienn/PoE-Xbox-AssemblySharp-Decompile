using System.Collections.Generic;
using UnityEngine;

[ClassTooltip("When the owner hits with a melee weapon attack, launches that attack at each enemy adjacent to the one hit with the specified stat adjustments.")]
public class Carnage : GenericAbility
{
	public int CarnageAccuracyBonus = -10;

	public float CarnageDamagePercent = 0.5f;

	public float ActiveCarnageTimer = 30f;

	private float m_countdownTimer;

	private bool m_attackingEnemies;

	private float m_triggerTimer;

	private AttackBase m_attack;

	private List<GameObject> m_victims = new List<GameObject>();

	public float BaseCarnageRadius => 1.1f;

	public float AdjustedCarnageRadius => BaseCarnageRadius * m_ownerStats.StatEffectRadiusMultiplier * m_ownerStats.AoERadiusMult;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			if (Passive)
			{
				m_permanent = true;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_triggerTimer > 0f)
		{
			m_triggerTimer -= Time.deltaTime;
			if (m_triggerTimer <= 0f)
			{
				m_triggerTimer = 0f;
				m_attackingEnemies = true;
				foreach (GameObject victim in m_victims)
				{
					if (victim != null)
					{
						AdditionalAttacks(m_attack, victim);
					}
				}
				m_victims.Clear();
				m_attack = null;
				m_attackingEnemies = false;
			}
		}
		if (!Passive && m_activated)
		{
			m_countdownTimer -= Time.deltaTime;
			if (m_countdownTimer <= 0f)
			{
				Deactivate(Owner);
				m_permanent = false;
			}
		}
	}

	public override void Activate(GameObject target)
	{
		m_countdownTimer = ActiveCarnageTimer;
		base.Activate(target);
	}

	public override void Activate(Vector3 target)
	{
		m_countdownTimer = ActiveCarnageTimer;
		base.Activate(target);
	}

	protected override void LaunchAttack(GameObject attackObj, bool useFullAttack, GenericAbility weaponAbility, StatusEffect[] effectsOnLaunch, AttackBase weaponAttack, int animVariation)
	{
		m_permanent = true;
		if (Passive)
		{
			Activate(Owner);
		}
		else
		{
			base.LaunchAttack(attackObj, useFullAttack, weaponAbility, effectsOnLaunch, weaponAttack, animVariation);
		}
	}

	public bool CarnageApplies(AttackBase attack)
	{
		if (attack is AttackMelee)
		{
			return attack.IsAutoAttack();
		}
		return false;
	}

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (!m_attackingEnemies && !args.Damage.IsMiss && !(args.Damage.Attack == null) && !(args.Attacker == null) && !(args.Victim == null) && !(args.Attacker != Owner) && !(m_ownerStats == null) && args.Damage.AttackIsHostile && CarnageApplies(args.Damage.Attack))
		{
			if (m_attack != null && m_attack != args.Damage.Attack)
			{
				Debug.LogError("Carnage is seeing two different attacks launched at the same time!");
			}
			m_attack = args.Damage.Attack;
			m_victims.Add(args.Victim);
			m_triggerTimer = 0.1f;
		}
	}

	private void AdditionalAttacks(AttackBase attack, GameObject victim)
	{
		float num = AdjustedCarnageRadius;
		Mover component = victim.GetComponent<Mover>();
		if (component != null)
		{
			num += component.Radius;
		}
		GameObject[] array = GameUtilities.CreaturesInRange(victim.transform.position, num, Owner, includeUnconscious: false);
		if (array == null)
		{
			return;
		}
		bool skipAnimation = attack.SkipAnimation;
		StatusEffectParams statusEffectParams = new StatusEffectParams();
		statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.BonusDamageMult;
		statusEffectParams.DmgType = DamagePacket.DamageType.All;
		statusEffectParams.Value = CarnageDamagePercent;
		statusEffectParams.OneHitUse = true;
		statusEffectParams.IsHostile = false;
		StatusEffectParams statusEffectParams2 = new StatusEffectParams();
		statusEffectParams2.AffectsStat = StatusEffect.ModifiedStat.Accuracy;
		statusEffectParams2.Value = CarnageAccuracyBonus + (int)GatherAbilityModSum(AbilityMod.AbilityModType.AttackAccuracyBonus);
		statusEffectParams2.OneHitUse = true;
		statusEffectParams.IsHostile = false;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject == victim))
			{
				m_ownerStats.ApplyStatusEffectImmediate(StatusEffect.Create(Owner, this, statusEffectParams, AbilityType.Ability, null, deleteOnClear: true));
				m_ownerStats.ApplyStatusEffectImmediate(StatusEffect.Create(Owner, this, statusEffectParams2, AbilityType.Ability, null, deleteOnClear: true));
				attack.SkipAnimation = true;
				attack.LaunchingDirectlyToImpact = false;
				attack.SkipAbilityActivation = true;
				try
				{
					attack.Launch(gameObject, this);
				}
				finally
				{
					attack.SkipAbilityActivation = false;
				}
			}
		}
		attack.SkipAnimation = skipAnimation;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = "";
		if (!Passive)
		{
			text = GUIUtils.Format(1634, GUIUtils.Format(211, ActiveCarnageTimer));
		}
		string additionalEffects = base.GetAdditionalEffects(stringEffects, mode, ability, character);
		return (text + "\n" + additionalEffects).Trim();
	}
}
