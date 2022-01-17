using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("Abilities/Teleport")]
public class TeleportAbility : AttackAbility
{
	private Health m_health;

	private bool m_interrupted;

	public bool DelayTeleportByAttackTime;

	[Tooltip("ONLY FOR MONSTER ABILITIES. If set, the teleport can go anywhere, even into unconnected areas of navmesh.")]
	public bool SkipRaycast;

	[Tooltip("If greater than 0, the caster will teleport back to his original position after this many seconds.")]
	public float TeleportBackAfter;

	public float TeleportBackStartVfxAt;

	[Tooltip("If set, the character will teleport back when the attached ability is deactivated.")]
	public bool TeleportBackOnAbilityDeactivate;

	private bool m_hasFiredTeleportBackVfx;

	private Vector3 m_StartPosition;

	private bool m_isOut;

	private bool m_hasSetCharacterInvisible;

	private bool m_IsReturning;

	private StatusEffect m_nonEngageEffect;

	public float TeleportBackTimer { get; private set; }

	protected override void Start()
	{
		base.SkipAnimation = false;
		m_interrupted = false;
		base.Start();
		if (m_parent != null)
		{
			m_health = m_parent.GetComponent<Health>();
		}
	}

	public override void Update()
	{
		if (TeleportBackTimer > 0f)
		{
			TeleportBackTimer -= Time.deltaTime;
			if (TeleportBackTimer <= TeleportBackStartVfxAt)
			{
				FireTeleportBackVfx();
			}
			if (TeleportBackTimer <= 0f)
			{
				TeleportBack();
			}
		}
		base.Update();
	}

	public override void OnDeactivateAbility()
	{
		if (TeleportBackOnAbilityDeactivate)
		{
			TeleportBack();
		}
		BecomeVisible();
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		enemy = base.Launch(enemy, variationOverride);
		m_destination = enemy.transform.position;
		m_hasFiredTeleportBackVfx = false;
		StatusEffectParams statusEffectParams = new StatusEffectParams();
		statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.NonEngageable;
		statusEffectParams.IsHostile = false;
		m_nonEngageEffect = CreateChildEffect(statusEffectParams, 0f, null, deleteOnClear: false);
		m_ownerStats.ApplyStatusEffect(m_nonEngageEffect);
		StartCoroutine(TeleportInvisible(m_parent, enemy));
		return enemy;
	}

	public override void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		Vector3 vector = location;
		Vector3 vector2 = vector - base.Owner.transform.position;
		if (GameUtilities.V3ToV2(vector2).magnitude > base.TotalAttackDistance)
		{
			vector2.Normalize();
			vector = base.Owner.transform.position + vector2 * base.TotalAttackDistance;
		}
		base.Launch(vector, enemy, variationOverride);
		m_destination = vector;
		m_hasFiredTeleportBackVfx = false;
		AIController.BreakAllEngagements(base.Owner);
		StartCoroutine(TeleportInvisible(m_parent, enemy));
	}

	private IEnumerator TeleportInvisible(GameObject self, GameObject enemy)
	{
		if (DelayTeleportByAttackTime)
		{
			yield return new WaitForSeconds(AttackHitTime);
		}
		if (m_health != null && !m_health.Dead && !m_health.Unconscious && !m_cancelled && !m_interrupted && m_ability != null && m_ability.Activated)
		{
			AIController.BreakAllEngagements(base.Owner);
			m_ownerStats.InvisibilityState++;
			m_hasSetCharacterInvisible = true;
			m_health.CanBeTargeted = false;
			m_health.Targetable = false;
			m_ownerStats.EvadeEverything = true;
			m_health.OnDeath += HandleOnDamaged;
			m_health.OnUnconscious += HandleOnDamaged;
		}
	}

	private void FireTeleportBackVfx()
	{
		if (!m_hasFiredTeleportBackVfx)
		{
			LaunchOnStartVisualEffect();
			GameUtilities.LaunchEffect(OnStartGroundVisualEffect, 1f, base.Owner.transform.position, m_ability);
			m_hasFiredTeleportBackVfx = true;
		}
	}

	private void TeleportBack()
	{
		if (m_isOut)
		{
			FireTeleportBackVfx();
			AIController component = ComponentUtils.GetComponent<AIController>(base.Owner);
			if ((bool)component)
			{
				component.SafePopAllStates();
			}
			m_IsReturning = true;
			OnImpact(base.Owner, m_StartPosition);
			m_IsReturning = false;
			TeleportBackTimer = 0f;
			GenericAbility component2 = GetComponent<GenericAbility>();
			if ((bool)component2)
			{
				component2.Deactivate(base.Owner);
				component2.UntriggerFromUI();
			}
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		BecomeVisible();
		m_interrupted = true;
	}

	public void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		m_health.OnDeath -= HandleOnDamaged;
		m_health.OnUnconscious -= HandleOnDamaged;
	}

	public override void OnImpact(GameObject self, GameObject enemy)
	{
		OnImpact(self, enemy.transform.position);
	}

	public override void OnImpact(GameObject self, Vector3 hitPosition)
	{
		Transform transform = m_parent.transform;
		m_StartPosition = transform.position;
		if (!m_IsReturning && !SkipRaycast && NavMesh.Raycast(transform.position, hitPosition, out var hit, int.MaxValue))
		{
			transform.position = hit.position;
		}
		else
		{
			transform.position = hitPosition;
		}
		Mover component = m_parent.GetComponent<Mover>();
		if (component != null)
		{
			transform.position = GameUtilities.NearestUnoccupiedLocation(transform.position, component.Radius, 10f, component);
		}
		else
		{
			transform.position = GameUtilities.NearestUnoccupiedLocation(transform.position, 0.5f, 10f, null);
		}
		base.OnImpact(self, transform.position);
		AIController.BreakAllEngagements(base.Owner);
		if (m_nonEngageEffect != null)
		{
			m_ownerStats.ClearEffect(m_nonEngageEffect);
			m_nonEngageEffect = null;
		}
		BecomeVisible();
		if (!m_IsReturning)
		{
			if (TeleportBackAfter > 0f)
			{
				TeleportBackTimer = TeleportBackAfter;
				m_isOut = true;
			}
			if (TeleportBackOnAbilityDeactivate)
			{
				m_isOut = true;
			}
		}
		else
		{
			m_isOut = false;
		}
	}

	private void BecomeVisible()
	{
		if (m_hasSetCharacterInvisible)
		{
			m_hasSetCharacterInvisible = false;
			m_ownerStats.InvisibilityState--;
			Transform hitTransform = GetHitTransform(m_parent);
			GameUtilities.LaunchEffect(OnHitVisualEffect, 1f, hitTransform, m_ability);
			GameUtilities.LaunchEffect(OnHitGroundVisualEffect, 1f, base.transform.position, m_ability);
		}
		m_health.CanBeTargeted = true;
		m_health.Targetable = true;
		m_ownerStats.EvadeEverything = false;
		AIController.BreakAllEngagements(base.Owner);
	}

	public override void BeginTargeting()
	{
	}

	public override void TargetingStopped()
	{
	}
}
