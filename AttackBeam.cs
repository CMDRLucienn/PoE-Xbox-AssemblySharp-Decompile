using UnityEngine;

[AddComponentMenu("Attacks/Beam")]
public class AttackBeam : AttackBase
{
	public float BeamDuration = 10f;

	public float BeamInterval = 1f;

	public TargetType BeamTargets;

	public bool BeamExcludesMainTarget;

	public GameObject LoopingBeamVisualEffect;

	public GameObject BeamEnemyOnStartVisualEffect;

	private bool m_first_impact = true;

	private float m_intervalTimer;

	private float m_durationTimer;

	private BeamVfx m_beam_fx;

	private Transform m_hitTransform;

	private Health m_ownerHealth;

	private Health m_enemyHealth;

	private readonly FormattableTarget BEAM_TARGET = new FormattableTarget(1638, 1639);

	public override bool AlreadyActivated => !m_first_impact;

	protected override void OnDestroy()
	{
		ShutdownBeam();
		base.OnDestroy();
	}

	public override void Update()
	{
		base.Update();
		if (m_intervalTimer > 0f)
		{
			m_intervalTimer -= Time.deltaTime;
			while (m_intervalTimer <= 0f)
			{
				HandleBeam(base.Owner, m_enemy, m_destination);
				if (m_enemy != null && !BeamExcludesMainTarget)
				{
					OnImpact(base.Owner, m_enemy);
				}
				m_intervalTimer += BeamInterval;
			}
		}
		if (m_durationTimer > 0f)
		{
			m_durationTimer -= Time.deltaTime;
			if (m_durationTimer <= 0f)
			{
				ShutdownBeam();
			}
			else if (m_ownerHealth != null && (m_ownerHealth.Dead || m_ownerHealth.Unconscious))
			{
				ShutdownBeam();
			}
			else if (RequiresHitObject && m_enemyHealth != null && (m_enemyHealth.Dead || m_enemyHealth.Unconscious))
			{
				ShutdownBeam();
			}
		}
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		m_hitTransform = GetHitTransform(enemy);
		if (enemy != null)
		{
			m_enemyHealth = enemy.GetComponent<Health>();
		}
		Launch(m_hitTransform.position, enemy, variationOverride);
		return enemy;
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		OnImpact(self, m_enemy, isMainTarget: true);
	}

	public override void OnImpact(GameObject self, GameObject enemy, bool isMainTarget)
	{
		if (m_cancelled)
		{
			return;
		}
		if (m_first_impact)
		{
			m_can_cancel = false;
			TriggerNoise();
			if (m_ability != null)
			{
				m_ability.Activate(enemy);
			}
			if (m_ownerStats != null)
			{
				m_ownerStats.TriggerWhenBeamHits(enemy);
			}
			if (m_enemy != null && m_hitTransform == null)
			{
				m_hitTransform = GetHitTransform(m_enemy);
			}
			m_intervalTimer = 0.0001f;
			m_durationTimer = BeamDuration;
			if (base.Owner != null)
			{
				m_ownerHealth = base.Owner.GetComponent<Health>();
			}
			if (m_enemy != null)
			{
				m_beam_fx = BeamVfx.Create(LoopingBeamVisualEffect, m_ability, GetLaunchTransform(), GetHitTransform(m_enemy), loop: true);
				GameUtilities.LaunchEffect(BeamEnemyOnStartVisualEffect, 1f, m_hitTransform, m_ability);
			}
			else
			{
				m_beam_fx = BeamVfx.Create(LoopingBeamVisualEffect, m_ability, GetLaunchTransform(), m_destination, loop: true);
				GameUtilities.LaunchEffect(BeamEnemyOnStartVisualEffect, 1f, m_destination, m_ability);
			}
			m_first_impact = false;
		}
		else
		{
			base.OnImpact(self, enemy, isMainTarget);
		}
	}

	public void ShutdownBeam()
	{
		m_first_impact = true;
		m_intervalTimer = 0f;
		m_durationTimer = 0f;
		if (m_beam_fx != null)
		{
			m_beam_fx.ShutDown();
			m_beam_fx = null;
		}
		m_hitTransform = null;
		m_can_cancel = false;
		if (m_ability != null)
		{
			m_ability.AttackComplete = true;
		}
	}

	protected override bool AffectsTarget()
	{
		return !BeamExcludesMainTarget;
	}

	public override void GetAdditionalEffects(StringEffects stringEffects, GenericAbility ability, GameObject character)
	{
		AddEffects(BEAM_TARGET, ability, character, 1f, stringEffects, BeamTargets);
		base.GetAdditionalEffects(stringEffects, ability, character);
	}

	public override string GetDurationString(GenericAbility ability)
	{
		return string.Concat(base.GetDurationString(ability) + "\n", AttackBase.FormatWC(GUIUtils.GetText(1634), GUIUtils.Format(211, BeamDuration.ToString("#0")))).Trim();
	}
}
