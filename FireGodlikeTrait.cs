using UnityEngine;

public class FireGodlikeTrait : GenericAbility
{
	[Persistent]
	private bool m_isActivated;

	private int m_originalOwnerLevel;

	private float m_minDamage;

	private float m_maxDamage;

	private const float staminaActivationThreshold = 0.5f;

	public override bool ListenForDamageEvents => true;

	public override bool OverrideStatusEffectDisplay => true;

	public override bool OverrideActivationPrerequisiteDisplay => true;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			if (m_ownerHealth != null && m_ownerHealth.CurrentStamina / m_ownerHealth.MaxStamina < 0.5f)
			{
				ActivateTrait();
			}
			m_originalOwnerLevel = 0;
			if (m_attackBase != null)
			{
				m_minDamage = m_attackBase.DamageData.Minimum;
				m_maxDamage = m_attackBase.DamageData.Maximum;
			}
		}
	}

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (IsActivated())
		{
			DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
			if (damageInfo != null && damageInfo.Attack != null && damageInfo.Attack is AttackMelee && (damageInfo.Attack.AbilityOrigin == null || !(damageInfo.Attack.AbilityOrigin is Frenzy)))
			{
				AttackBase component = GetComponent<AttackBase>();
				if ((bool)component)
				{
					component.SkipAnimation = true;
					component.Launch(args.GameObjectData[0], this);
				}
			}
		}
		else if (m_ownerHealth != null)
		{
			float num = m_ownerHealth.MaxStamina * 0.5f;
			if (m_ownerHealth.CurrentStamina < num)
			{
				ActivateTrait();
			}
		}
	}

	public override void HandleOnHealed(GameObject myObject, GameEventArgs args)
	{
		if (IsActivated() && m_ownerHealth != null)
		{
			float num = m_ownerHealth.MaxStamina * 0.5f;
			if (m_ownerHealth.CurrentStamina >= num)
			{
				DeactivateTrait();
			}
		}
	}

	protected override void Update()
	{
		if ((bool)m_ownerStats && m_ownerStats.ScaledLevel != m_originalOwnerLevel)
		{
			m_originalOwnerLevel = m_ownerStats.ScaledLevel;
			if (m_attackBase != null)
			{
				m_attackBase.DamageData.Minimum = m_minDamage * (float)m_ownerStats.ScaledLevel;
				m_attackBase.DamageData.Maximum = m_maxDamage * (float)m_ownerStats.ScaledLevel;
			}
		}
		base.Update();
	}

	protected void ActivateTrait()
	{
		if (IsActivated() || Owner == null)
		{
			return;
		}
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			effect.Params.DontHideFromLog = true;
			component.ApplyStatusEffectImmediate(effect);
		}
		m_isActivated = true;
	}

	protected void DeactivateTrait()
	{
		if (!IsActivated() || Owner == null)
		{
			return;
		}
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			component.ClearEffect(effect);
		}
		m_isActivated = false;
	}

	protected bool IsActivated()
	{
		return m_isActivated;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = "";
		CharacterStats component = character.GetComponent<CharacterStats>();
		text = ((CleanedUpStatusEffects == null || CleanedUpStatusEffects.Count <= 0) ? StatusEffectParams.ListToString(StatusEffects, component, this) : StatusEffectParams.ListToString(CleanedUpStatusEffects, component, this));
		if (!string.IsNullOrEmpty(text))
		{
			AttackBase.AddStringEffect(GetSelfTarget().GetText(), new AttackBase.AttackEffect(text, null), stringEffects);
		}
		return (GUIUtils.Format(1366, GUIUtils.Format(1277, 50f.ToString("#0"))) + "\n" + base.GetAdditionalEffects(stringEffects, mode, ability, character)).Trim();
	}
}
