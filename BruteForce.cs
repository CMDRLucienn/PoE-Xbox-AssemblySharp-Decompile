using UnityEngine;

[ClassTooltip("When the owner attacks a target with an attack targeting 'Attacking With Defense', if the target's 'Switch To Defense' is lower, it attacks that instead.")]
public class BruteForce : GenericAbility
{
	public CharacterStats.DefenseType AttackingWithDefense;

	public CharacterStats.DefenseType SwitchToDefense = CharacterStats.DefenseType.Fortitude;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (!(args.Victim != null) || args.Damage.DefendedBy != AttackingWithDefense)
		{
			return;
		}
		CharacterStats component = args.Victim.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			bool flag = component.CalculateIsImmune(AttackingWithDefense, args.Damage.Attack, args.Attacker);
			int num = component.CalculateDefense(SwitchToDefense, args.Damage.Attack, args.Attacker);
			int num2 = component.CalculateDefense(AttackingWithDefense, args.Damage.Attack, args.Attacker);
			if (num < num2 || flag)
			{
				args.Damage.DefendedBy = SwitchToDefense;
			}
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string effect = GUIUtils.Format(2454, GUIUtils.GetDefenseTypeString(AttackingWithDefense), GUIUtils.GetDefenseTypeString(SwitchToDefense));
		AttackBase.AddStringEffect(GetAbilityTarget().GetText(), new AttackBase.AttackEffect(effect, null), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
