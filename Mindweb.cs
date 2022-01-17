using UnityEngine;

[ClassTooltip("Must grant a MindwebEffect status effect via an aura. Manages those effects so that all affected characters \nhave the maximum defense value of any affected character.")]
public class Mindweb : GenericCipherAbility
{
	public GameObject BeamEffect;

	public AttackBase.EffectAttachType BeamAttachment;

	protected override void ActivateStatusEffects()
	{
		base.ActivateStatusEffects();
		StatusEffect statusEffect = null;
		for (int i = 0; i < m_effects.Count; i++)
		{
			if (m_effects[i].Params.AffectsStat == StatusEffect.ModifiedStat.MindwebEffect)
			{
				statusEffect = m_effects[i];
				break;
			}
		}
		foreach (GameObject auraEffectsAppliedCharacter in statusEffect.AuraEffectsAppliedCharacters)
		{
			Transform transform = AttackBase.GetTransform(auraEffectsAppliedCharacter, BeamAttachment);
			if (!transform)
			{
				continue;
			}
			foreach (GameObject auraEffectsAppliedCharacter2 in statusEffect.AuraEffectsAppliedCharacters)
			{
				Transform transform2 = AttackBase.GetTransform(auraEffectsAppliedCharacter2, BeamAttachment);
				if ((bool)transform2)
				{
					BeamVfx.Create(BeamEffect, this, transform, transform2, loop: false);
				}
			}
		}
		Transform transform3 = AttackBase.GetTransform(Owner, BeamAttachment);
		if (!transform3)
		{
			return;
		}
		foreach (GameObject auraEffectsAppliedCharacter3 in statusEffect.AuraEffectsAppliedCharacters)
		{
			Transform transform4 = AttackBase.GetTransform(auraEffectsAppliedCharacter3, BeamAttachment);
			if ((bool)transform4)
			{
				BeamVfx.Create(BeamEffect, this, transform3, transform4, loop: false);
			}
		}
	}

	public int CalculateDefense(CharacterStats.DefenseType defenseType, AttackBase attack, GameObject enemy, bool isSecondary)
	{
		StatusEffect statusEffect = null;
		for (int i = 0; i < m_effects.Count; i++)
		{
			if (m_effects[i].Params.AffectsStat == StatusEffect.ModifiedStat.MindwebEffect)
			{
				statusEffect = m_effects[i];
				break;
			}
		}
		int num = m_ownerStats.CalculateDefense(defenseType, attack, enemy, isSecondary, allowRedirect: false);
		if (statusEffect == null)
		{
			Debug.LogError("Mindweb ability has no MindwebEffect status effect ('" + base.name + "').");
			return num;
		}
		foreach (GameObject auraEffectsAppliedCharacter in statusEffect.AuraEffectsAppliedCharacters)
		{
			CharacterStats characterStats = (auraEffectsAppliedCharacter ? auraEffectsAppliedCharacter.GetComponent<CharacterStats>() : null);
			if ((bool)characterStats)
			{
				num = Mathf.Max(num, characterStats.CalculateDefense(defenseType, attack, enemy, isSecondary, allowRedirect: false));
			}
		}
		return num;
	}
}
