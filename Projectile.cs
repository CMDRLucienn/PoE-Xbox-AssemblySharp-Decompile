using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
	public bool Arcing;

	public bool Wild;

	public float InitialSpeed = 10f;

	public float AccelerationDelay;

	public float AccelerationMetersPerSecond;

	public float DestroyDelayTime;

	private float MaxDistance = 50f;

	private AttackRanged m_attack;

	private bool m_launched;

	private GameObject m_target;

	private Vector3[] m_flightPath = new Vector3[4];

	private float m_travelDist;

	private float m_distanceTraveled;

	private float m_totalDistanceTraveled;

	private float m_speed;

	private float m_flightTime;

	private float m_radius;

	private bool m_useTargetPoint;

	private Vector3 m_targetPoint = Vector3.zero;

	private Vector3 m_velocity = Vector3.zero;

	private int m_bounceCount;

	private List<GameObject> m_bounceObjects;

	private float m_bounceTimer;

	private GameObject m_bounceEnemy;

	private Vector3 m_bounceNormal;

	private bool m_is_destructible = true;

	private bool m_destruction_accounted_for;

	private bool m_allow_updates = true;

	private bool m_can_hit_owner;

	private float m_pulse_duration;

	private float m_pulse_interval;

	private Vector3 m_pulse_position;

	public bool OwnerMayBeUnconscious { get; set; }

	public bool TargetMustBeUnconscious { get; set; }

	public GameObject Owner
	{
		get
		{
			if (m_attack != null)
			{
				return m_attack.Owner;
			}
			return null;
		}
	}

	public GenericAbility AbilityOrigin
	{
		get
		{
			if (m_attack != null)
			{
				return m_attack.AbilityOrigin;
			}
			return null;
		}
	}

	public AttackRanged OnImpactAttack
	{
		get
		{
			return m_attack;
		}
		set
		{
			m_attack = value;
		}
	}

	public bool IsDestructible
	{
		get
		{
			return m_is_destructible;
		}
		set
		{
			m_is_destructible = value;
		}
	}

	public bool CanHitOwner
	{
		get
		{
			return m_can_hit_owner;
		}
		set
		{
			m_can_hit_owner = value;
		}
	}

	public GameObject TargetObject
	{
		get
		{
			return m_target;
		}
		set
		{
			m_target = value;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			if (m_velocity.sqrMagnitude < float.Epsilon)
			{
				m_velocity = (m_flightPath[3] - m_flightPath[0]).normalized;
			}
			return m_velocity;
		}
	}

	public List<GameObject> BounceObjects
	{
		get
		{
			return m_bounceObjects;
		}
		set
		{
			m_bounceObjects = value;
		}
	}

	public float TotalDistanceTraveled
	{
		get
		{
			return m_totalDistanceTraveled;
		}
		set
		{
			m_totalDistanceTraveled = value;
		}
	}

	private void Start()
	{
		GetComponent<Rigidbody>().useGravity = false;
		GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().detectCollisions = true;
		m_speed = InitialSpeed;
		m_allow_updates = true;
		base.gameObject.layer = LayerMask.NameToLayer("Dynamics");
		if (GetComponent<Collider>() != null)
		{
			m_radius = GetComponent<Collider>().bounds.extents.x;
		}
	}

	public virtual void LaunchAtPoint(Vector3 launchPosition, Vector3 destination)
	{
		m_useTargetPoint = true;
		m_targetPoint = destination;
		Vector3 normalized = (destination - launchPosition).normalized;
		Launch(launchPosition, normalized);
	}

	public virtual void Launch(Vector3 launchPosition, Vector3 initialDirection, GameObject target)
	{
		TargetObject = target;
		Launch(launchPosition, initialDirection);
	}

	public virtual void Launch(Vector3 launchPosition, Vector3 direction)
	{
		m_flightTime = 0f;
		base.transform.position = launchPosition;
		Vector3 vector = direction;
		if (Arcing)
		{
			vector += base.transform.up * 3f;
		}
		vector.Normalize();
		Vector3 vector2 = direction;
		float num = MaxDistance;
		if ((bool)m_target)
		{
			vector2 = m_attack.GetHitTransform(m_target).position - launchPosition;
			num = vector2.magnitude;
			vector2.Normalize();
		}
		else if (m_useTargetPoint)
		{
			vector2 = m_targetPoint - launchPosition;
			num = vector2.magnitude;
			vector2.Normalize();
		}
		else if (m_attack.MultiHitRay)
		{
			num = m_attack.AdjustedMultiHitDist;
		}
		m_flightPath[0] = launchPosition;
		if (m_attack.MultiHitRay)
		{
			m_flightPath[1] = launchPosition + vector * num * 0.3333f;
		}
		else
		{
			m_flightPath[1] = launchPosition + vector * num * 0.5f;
		}
		Quaternion quaternion = Quaternion.AngleAxis(OEIRandom.RangeInclusive(-90f, 90f), Vector3.up);
		if (Wild)
		{
			m_flightPath[2] = launchPosition + quaternion * ((vector + vector2).normalized * num * 0.9f);
		}
		else if (m_attack.MultiHitRay)
		{
			m_flightPath[2] = launchPosition + vector * num * 0.6666f;
		}
		else
		{
			m_flightPath[2] = m_flightPath[1] + (vector + vector2).normalized * num * 0.25f;
		}
		if ((bool)m_target)
		{
			UpdateFlightEndpointForTarget();
		}
		else if (m_useTargetPoint)
		{
			m_flightPath[3] = m_targetPoint;
		}
		else if (m_attack.MultiHitRay)
		{
			m_flightPath[3] = vector * num + launchPosition;
		}
		else
		{
			m_flightPath[3] = vector * MaxDistance + launchPosition;
		}
		m_travelDist = Vector3.Distance(m_flightPath[0], m_flightPath[1]) + Vector3.Distance(m_flightPath[1], m_flightPath[2]) + Vector3.Distance(m_flightPath[2], m_flightPath[3]);
		if (m_travelDist == 0f)
		{
			HandleImpact();
		}
		m_distanceTraveled = 0f;
		base.transform.rotation = Quaternion.LookRotation(vector2);
		m_launched = true;
	}

	private void Update()
	{
		if (GameState.Paused || !m_launched || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (m_attack == null)
		{
			GameUtilities.Destroy(base.gameObject);
			return;
		}
		Transform transform = base.transform;
		if (m_bounceTimer > 0f)
		{
			m_bounceTimer -= Time.deltaTime;
			CheckBounceTimer();
		}
		if (m_pulse_duration > 0f && m_attack is AttackPulsedAOE)
		{
			m_pulse_interval -= Time.deltaTime;
			if (m_pulse_interval <= 0f)
			{
				GameObject forcedTarget = m_attack.ForcedTarget;
				if ((bool)forcedTarget)
				{
					m_pulse_position = forcedTarget.transform.position;
					transform.position = m_pulse_position;
				}
				(m_attack as AttackPulsedAOE).Pulse(base.gameObject, m_pulse_position);
				m_pulse_interval += AttackData.Instance.PulseAoeIntervalRate;
			}
			m_pulse_duration -= Time.deltaTime;
			if ((bool)m_target && TargetMustBeUnconscious)
			{
				Health component = m_target.GetComponent<Health>();
				if ((bool)component && !component.Dead && !component.Unconscious)
				{
					m_pulse_duration = 0f;
				}
			}
			if (m_attack.Owner != null && !OwnerMayBeUnconscious)
			{
				Health component2 = m_attack.Owner.GetComponent<Health>();
				if ((bool)component2 && (component2.Dead || component2.Unconscious))
				{
					m_pulse_duration = 0f;
				}
			}
			if (m_attack.AbilityOrigin != null)
			{
				if (m_attack.AbilityOrigin.CombatOnly && !GameState.InCombat)
				{
					m_pulse_duration = 0f;
				}
				else if (m_attack.AbilityOrigin.NonCombatOnly && GameState.InCombat)
				{
					m_pulse_duration = 0f;
				}
				if (m_attack.AbilityOrigin.Passive && !m_attack.AbilityOrigin.Ready)
				{
					if ((m_attack.AbilityOrigin.WhyNotReady & GenericAbility.NotReadyValue.NotWhileMoving) != 0)
					{
						m_pulse_duration = 0f;
					}
					if ((m_attack.AbilityOrigin.WhyNotReady & GenericAbility.NotReadyValue.FailedPrerequisite) != 0)
					{
						m_pulse_duration = 0f;
					}
				}
				if (m_attack.Owner == null && GenericAbility.AbilityTypeIsAnyEquipment(m_attack.AbilityOrigin.EffectType))
				{
					m_pulse_duration = 0f;
				}
			}
			if (m_pulse_duration <= 0f)
			{
				DestroyProjectile();
			}
		}
		else if (m_allow_updates)
		{
			m_flightTime += Time.deltaTime;
			UpdateFlightEndpointForTarget();
			if (m_flightTime > AccelerationDelay)
			{
				m_speed += AccelerationMetersPerSecond * Time.deltaTime;
			}
			Vector3 position = transform.position;
			transform.position = GetInterpolatedSplinePoint(m_distanceTraveled / m_travelDist);
			m_velocity = (transform.position - position).normalized * m_speed;
			if (m_velocity.sqrMagnitude < float.Epsilon)
			{
				m_velocity = (m_flightPath[3] - m_flightPath[0]).normalized;
			}
			transform.rotation = Quaternion.LookRotation(m_velocity);
			float num = m_speed * Time.deltaTime;
			m_distanceTraveled += num;
			TotalDistanceTraveled += num;
			float num2 = m_attack.TotalAttackDistance;
			if (m_attack.MultiHitRay)
			{
				num2 = m_attack.AdjustedMultiHitDist;
			}
			if (m_distanceTraveled / m_travelDist > 0.99f)
			{
				HandleImpact();
			}
			else if (m_attack.Bounces > 0 && !m_attack.RequiresHitObject && TotalDistanceTraveled >= num2)
			{
				m_attack.ClearLaunchFlags();
				DestroyProjectile();
			}
		}
	}

	private void UpdateFlightEndpointForTarget()
	{
		if (!m_target || !m_target.activeInHierarchy)
		{
			return;
		}
		AttackBase.EffectAttachType effectAttachType = (m_attack ? m_attack.OnHitAttach : AttackBase.EffectAttachType.Root);
		if (effectAttachType != AttackBase.EffectAttachType.Root)
		{
			AnimationBoneMapper component = m_target.GetComponent<AnimationBoneMapper>();
			if ((bool)component)
			{
				component.Initialize();
				if (component.HasBone(m_target, effectAttachType))
				{
					Transform bone = component.GetBone(m_target, effectAttachType);
					if ((bool)bone)
					{
						m_flightPath[3] = bone.transform.position;
						return;
					}
				}
			}
		}
		m_flightPath[3] = m_target.transform.position;
	}

	private void HandleImpact()
	{
		if (m_attack != null && m_attack.ImpactDelay > 0f)
		{
			m_allow_updates = false;
			StartCoroutine(HandleImpactDelay(m_attack.ImpactDelay));
		}
		else
		{
			HandleImpactHelper();
		}
	}

	private IEnumerator HandleImpactDelay(float time)
	{
		yield return new WaitForSeconds(time);
		m_allow_updates = true;
		HandleImpactHelper();
	}

	private void HandleImpactHelper()
	{
		CheckForAudio();
		bool forceDestroy = false;
		if (!Cutscene.CutsceneActive && ConversationManager.Instance.GetActiveConversationForHUD() == null)
		{
			if (m_target != null)
			{
				m_attack.OnImpactForceTarget(base.gameObject, m_target);
			}
			else if (m_useTargetPoint)
			{
				m_attack.OnImpact(base.gameObject, m_targetPoint);
			}
			else
			{
				m_attack.OnImpact(base.gameObject, base.transform.position);
			}
		}
		else
		{
			m_attack.ClearLaunchFlags();
		}
		if (m_target == null && m_pulse_duration == 0f)
		{
			forceDestroy = true;
		}
		HandlePostImpact(forceDestroy);
	}

	private void HandlePostImpact(bool forceDestroy)
	{
		if (forceDestroy)
		{
			DestroyProjectile();
		}
		else
		{
			if (m_attack.MultiHitRay)
			{
				return;
			}
			if (IsDestructible)
			{
				DestroyProjectile();
				return;
			}
			TurnOffProjectile();
			if (m_pulse_duration == 0f)
			{
				GameUtilities.ShutDownLoopingEffect(base.gameObject);
			}
		}
	}

	public void TurnOffProjectile()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		if (componentsInChildren != null)
		{
			MeshRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
		ParticleSystem[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		if (componentsInChildren2 != null)
		{
			ParticleSystem[] array2 = componentsInChildren2;
			for (int i = 0; i < array2.Length; i++)
			{
				ParticleSystem.EmissionModule emission = array2[i].emission;
				emission.enabled = false;
			}
		}
		m_allow_updates = false;
	}

	private void CheckForAudio()
	{
		AudioSource component = base.gameObject.GetComponent<AudioSource>();
		if (component != null)
		{
			GlobalAudioPlayer.Instance.PlayOneShot(component, component.clip, component.volume);
		}
	}

	private Vector3 GetInterpolatedSplinePoint(float t)
	{
		return GetPointOnSpline(t, m_flightPath[0], m_flightPath[1], m_flightPath[2], m_flightPath[3]);
	}

	private Vector3 GetPointOnSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = t * t;
		float num2 = num * t;
		return p0 * Mathf.Pow(1f - t, 3f) + p1 * 3f * Mathf.Pow(1f - t, 2f) * t + p2 * (3f * (1f - t) * num) + p3 * num2;
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if (m_attack == null || !m_attack.MultiHitRay)
		{
			return;
		}
		if (m_target != null)
		{
			GameObject gameObject = other.gameObject;
			while (gameObject.transform.parent != null)
			{
				gameObject = gameObject.transform.parent.gameObject;
			}
			if (gameObject == m_target)
			{
				HandleImpact();
			}
			return;
		}
		if (TotalDistanceTraveled < m_radius && !m_attack.OverridesProjectileLaunchPosition)
		{
			Vector3 normalized = (other.gameObject.transform.position - base.gameObject.transform.position).normalized;
			if (Vector3.Dot(base.gameObject.transform.forward, normalized) < 0f)
			{
				return;
			}
		}
		if ((bool)other.gameObject.GetComponent<Health>())
		{
			if ((CanHitOwner && other.gameObject == m_attack.Owner) || (other.gameObject != m_attack.Owner && m_attack.IsValidTarget(other.gameObject)))
			{
				m_attack.OnImpact(base.gameObject, other.gameObject);
				HandlePostImpact(forceDestroy: false);
			}
		}
		else if (!other.isTrigger && (other.gameObject.layer == LayerMask.NameToLayer("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Doors")))
		{
			m_attack.OnImpact(base.gameObject, base.transform.position);
			HandlePostImpact(forceDestroy: false);
		}
	}

	public void PrepareBounceEnemy(float timer, GameObject enemy)
	{
		m_bounceTimer = timer;
		m_bounceEnemy = enemy;
		IsDestructible = false;
		CheckBounceTimer();
	}

	public void PrepareBounceWall(float timer, Vector3 normal)
	{
		m_bounceTimer = timer;
		m_bounceEnemy = null;
		m_bounceNormal = normal;
		IsDestructible = false;
		CheckBounceTimer();
	}

	private void CheckBounceTimer()
	{
		if (m_bounceTimer <= 0f)
		{
			m_bounceTimer = 0f;
			if (m_bounceEnemy != null)
			{
				m_attack.HandleBouncing(base.gameObject, m_bounceEnemy);
			}
			else
			{
				m_attack.HandleBouncing(base.gameObject, m_bounceNormal);
			}
		}
	}

	public int GetBounceCount()
	{
		return m_bounceCount;
	}

	public int IncrementBounceCount()
	{
		m_bounceCount++;
		return m_bounceCount;
	}

	public void SetBounceCount(int count)
	{
		m_bounceCount = count;
	}

	public void ResetBounceCount()
	{
		m_bounceCount = 0;
	}

	public void SetPulse(float duration, Vector3 position)
	{
		if (m_attack is AttackPulsedAOE)
		{
			if (duration <= 0f)
			{
				if (m_attack.AbilityOrigin != null)
				{
					Debug.LogError("Illegal duration of zero or less on Pulse AOE attack of ability " + m_attack.AbilityOrigin.Name());
				}
				duration = 0.1f;
			}
			m_pulse_duration = duration;
			m_pulse_interval = AttackData.Instance.PulseAoeIntervalRate;
			m_pulse_position = position;
			IsDestructible = false;
		}
		else
		{
			DestroyProjectile();
		}
	}

	public void DestroyProjectile()
	{
		if (!(m_attack != null))
		{
			return;
		}
		m_pulse_duration = 0f;
		IsDestructible = true;
		if (m_attack is AttackPulsedAOE)
		{
			GenericAbility abilityOrigin = m_attack.AbilityOrigin;
			if (abilityOrigin != null)
			{
				abilityOrigin.Deactivate(m_target);
			}
		}
		m_attack.DestroyProjectile(base.gameObject);
		m_attack.ActiveProjectileCount--;
		m_destruction_accounted_for = true;
	}

	private void OnDestroy()
	{
		if (m_attack != null && !m_destruction_accounted_for)
		{
			m_attack.ActiveProjectileCount--;
		}
	}
}
