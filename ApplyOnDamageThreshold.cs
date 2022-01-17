using System.Collections.Generic;
using UnityEngine;

[ClassTooltip("Whenever the owner does over a certain amount of damage to an enemy, launches its attack at that enemy.")]
public class ApplyOnDamageThreshold : GenericAbility
{
	[Tooltip("An attack must do this much damage to trigger the ability.")]
	public float Threshold = 10f;

	protected override void HandleStatsOnDamageFinal(GameObject source, CombatEventArgs args)
	{
		if (args.Damage.FinalAdjustedDamage >= Threshold)
		{
			base.Attack.SkipAnimation = true;
			base.Attack.Launch(args.Victim, this);
		}
		base.HandleStatsOnDamageFinal(source, args);
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string additionalEffects = base.GetAdditionalEffects(stringEffects, mode, ability, character);
		string fstring = GUIUtils.Format(2243, "{0}", Threshold.ToString("#0.#"));
		foreach (string key in stringEffects.Effects.Keys)
		{
			List<AttackBase.AttackEffect> list = stringEffects[key];
			for (int i = 0; i < list.Count; i++)
			{
				AttackBase.AttackEffect value = list[i];
				value.EffectPostFormat = StringUtility.Format(fstring, list[i].EffectPostFormat);
				list[i] = value;
			}
		}
		return additionalEffects;
	}
}
