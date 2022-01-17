using UnityEngine;

public class FinishingBlow : GenericAbility
{
	public float BaseHitDamageMult = 1.5f;

	[Range(0f, 100f)]
	public int BonusDamageHealthPctThreshhold = 50;

	[Range(0f, 2f)]
	public float BonusDamagePctPerHP = 0.03f;

	public void AdjustDamage(DamageInfo damage, GameObject enemy)
	{
		float num = (float)BonusDamageHealthPctThreshhold / 100f;
		Health component = enemy.GetComponent<Health>();
		float num2 = component.CurrentStamina / component.MaxStamina;
		damage.DamageMult(BaseHitDamageMult);
		if (num2 <= num)
		{
			float num3 = num - num2;
			float bonus = (BonusDamagePctPerHP + GatherAbilityModSum(AbilityMod.AbilityModType.FinishingBlowDamagePercentAdjustment)) * damage.DamageAmount * num3 * 100f;
			damage.DamageAdd(bonus);
		}
		CharacterStats component2 = Owner.GetComponent<CharacterStats>();
		if ((bool)component2)
		{
			damage.DamageMult(component2.FinishingBlowDamageMult);
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string effect = GUIUtils.Format(1144, TextUtils.MultiplierAsPercentBonus(BaseHitDamageMult));
		AttackBase.AddStringEffect(GetAbilityTarget().GetText(), new AttackBase.AttackEffect(effect, null), stringEffects);
		effect = GUIUtils.Format(1154, TextUtils.MultiplierAsPercentBonus((float)BonusDamageHealthPctThreshhold * BonusDamagePctPerHP));
		AttackBase.AddStringEffect(GetAbilityTarget().GetText(), new AttackBase.AttackEffect(effect, null), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
