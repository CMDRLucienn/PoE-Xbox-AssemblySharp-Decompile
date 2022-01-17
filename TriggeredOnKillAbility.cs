using UnityEngine;

[ClassTooltip("When the ability's owner kills a character, this ability's Status Effects will be applied to him, then the AoeOnKill will be immediately launched at his location.")]
public class TriggeredOnKillAbility : GenericAbility
{
	public AttackAOE AoeOnKill;

	private GameObject m_hookedTo;

	protected override void Init()
	{
		if (m_initialized)
		{
			return;
		}
		base.Init();
		if ((bool)Owner)
		{
			Health component = Owner.GetComponent<Health>();
			if (component != null)
			{
				component.OnKill += HandleHealthOnKill;
				m_hookedTo = Owner;
			}
		}
		m_permanent = true;
	}

	public override void Restored()
	{
		if (base.IsLoaded || (!GameState.LoadedGame && !GameState.IsRestoredLevel))
		{
			return;
		}
		bool activated = m_activated;
		m_activated = false;
		base.Restored();
		m_activated = activated;
		if (m_hookedTo != null)
		{
			Health component = m_hookedTo.GetComponent<Health>();
			if (component != null)
			{
				component.OnKill -= HandleHealthOnKill;
			}
			m_hookedTo = null;
		}
		if ((bool)Owner)
		{
			Health component2 = Owner.GetComponent<Health>();
			if (component2 != null)
			{
				component2.OnKill += HandleHealthOnKill;
				m_hookedTo = Owner;
			}
		}
	}

	private void HandleHealthOnKill(GameObject myObject, GameEventArgs args)
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (!(component == null))
		{
			ReportActivation(overridePassive: true);
			StatusEffectParams[] statusEffects = StatusEffects;
			foreach (StatusEffectParams param in statusEffects)
			{
				StatusEffect effect = StatusEffect.Create(base.gameObject, this, param, AbilityType.Ability, null, deleteOnClear: true);
				component.ApplyStatusEffect(effect);
			}
			if (AoeOnKill != null)
			{
				AttackAOE attackAOE = Object.Instantiate(AoeOnKill);
				attackAOE.DestroyAfterImpact = true;
				attackAOE.Owner = Owner;
				attackAOE.transform.parent = Owner.transform;
				attackAOE.SkipAnimation = true;
				attackAOE.AbilityOrigin = this;
				attackAOE.OnImpactShared(null, Owner.transform.position, null);
			}
		}
	}

	protected override bool ShowNormalActivationMessages()
	{
		return false;
	}

	protected override void ActivateStatusEffects()
	{
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = "";
		if ((bool)AoeOnKill)
		{
			text = AoeOnKill.GetString(ability, character, stringEffects);
		}
		return (text + "\n" + base.GetAdditionalEffects(stringEffects, mode, ability, character)).Trim();
	}
}
