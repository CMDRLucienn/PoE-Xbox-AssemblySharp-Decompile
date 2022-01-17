using UnityEngine;

[ClassTooltip("Triggers automatically when the owner gets hit, and overrides the Dmg Type parameter on its status effects with the type of the triggering damage.")]
public class TriggeredImmunity : GenericAbility
{
	[Tooltip("Ability triggers when owner loses at least this ratio of his endurance (0-1).")]
	public float EnduranceTrigger = 0.1f;

	private DamagePacket.DamageType m_TriggeringDamageType;

	public override bool TriggeredAutomatically => true;

	public override bool ListenForDamageEvents => true;

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (!m_activated && ReadyIgnoreRecovery && CanApply() && m_ownerHealth != null && args.GenericData[0] is DamageInfo damageInfo && args.FloatData[0] / m_ownerHealth.MaxStamina >= EnduranceTrigger)
		{
			m_TriggeringDamageType = damageInfo.DamageType;
			Apply(Owner);
		}
	}

	protected override void ActivateStatusEffects()
	{
		foreach (StatusEffect effect in m_effects)
		{
			effect.Params.DmgType = m_TriggeringDamageType;
		}
		base.ActivateStatusEffects();
	}
}
