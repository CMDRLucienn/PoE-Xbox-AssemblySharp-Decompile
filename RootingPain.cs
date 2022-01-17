using UnityEngine;

public class RootingPain : GenericAbility
{
	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			CharacterStats component = Owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.OnAddStatusEffect += AddStatusEffect;
			}
			m_permanent = true;
		}
	}

	private void AddStatusEffect(GameObject sender, StatusEffect effect, bool isFromAura)
	{
		if (string.Compare(effect.Params.Tag, "Wound", ignoreCase: true) == 0)
		{
			TriggerShockwave();
		}
	}

	private void TriggerShockwave()
	{
		AttackAOE component = GetComponent<AttackAOE>();
		if (component != null)
		{
			component.OnImpact(null, Owner.transform.position);
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)Owner)
		{
			CharacterStats component = Owner.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.OnAddStatusEffect -= AddStatusEffect;
			}
		}
	}
}
