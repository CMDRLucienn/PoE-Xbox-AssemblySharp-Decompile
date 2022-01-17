using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Attacks/Ranged")]
public class AttackRanged : AttackBase
{
	public bool ExtraAOEOnBounce;

	public float throwSpeed = 10f;

	public float dropObjectDist = 8f;

	[Range(1f, 10f)]
	public int ProjectileCount = 1;

	public bool VeilPiercing;

	[Range(0f, 360f)]
	public float ProjectileConeAngle = 145f;

	public Projectile ProjectilePrefab;

	public GameObject ImpactEffect;

	public float ImpactDelay;

	public bool MultiHitRay;

	public float MultiHitTravelDist = 10f;

	protected float m_launchTimer;

	protected bool m_gotHitEvent;

	protected bool m_launched;

	private bool m_attachedToRangedHit;

	public virtual bool OverridesProjectileLaunchPosition => false;

	public override bool IsRangeUsed
	{
		get
		{
			if (base.IsRangeUsed)
			{
				return PathsToPos;
			}
			return false;
		}
	}

	public override bool IsBouncing
	{
		get
		{
			if (Bounces > 0)
			{
				return ActiveProjectileCount > 0;
			}
			return false;
		}
	}

	public int ActiveProjectileCount { get; set; }

	public float AdjustedMultiHitDist => GetAdjustedMultiHitDist(base.Owner);

	protected override void Start()
	{
		base.Start();
		if (ProjectileCount == 0)
		{
			ProjectileCount = 1;
			Debug.LogWarning("Ranged attack on " + base.gameObject.name + " can't have a projectile count of 0!", this);
		}
		ActiveProjectileCount = 0;
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		if (!MultiHitRay)
		{
			m_enemy = enemy;
		}
		Transform hitTransform = GetHitTransform(enemy);
		m_destination = hitTransform.position;
		Launch(m_destination, enemy, variationOverride);
		return enemy;
	}

	protected void AttackRangedBaseLaunch(Vector3 location, GameObject enemy, int variationOverride)
	{
		base.Launch(location, enemy, variationOverride);
	}

	public virtual Vector3 GetProjectileLaunchPosition(Vector3 target)
	{
		return base.Owner.transform.position;
	}

	public override void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		base.Launch(location, enemy, variationOverride);
		Vector3 position = base.transform.position;
		Transform launchTransform = base.LaunchTransform;
		if (launchTransform != null)
		{
			position = launchTransform.position;
		}
		StartCoroutine(Launch(position, location, enemy));
	}

	public IEnumerator Launch(Vector3 launchPos, Vector3 desiredDestination, GameObject enemy)
	{
		return Launch(launchPos, desiredDestination, enemy, 0, 0f, null);
	}

	public IEnumerator Launch(Vector3 launchPos, Vector3 desiredDestination, GameObject enemy, int bounceCount, float distanceTraveled, List<GameObject> bounceList)
	{
		if (!m_launched)
		{
			if (base.SkipAnimation)
			{
				m_gotHitEvent = true;
				m_launchTimer = 0f;
			}
			else
			{
				m_gotHitEvent = false;
				m_launchTimer = GetLaunchTimerDuration();
				AnimationController component = base.Owner.GetComponent<AnimationController>();
				if ((bool)component)
				{
					if (!m_attachedToRangedHit && bounceCount == 0)
					{
						component.OnEventHit += anim_OnEventHit;
						m_attachedToRangedHit = true;
					}
				}
				else
				{
					m_gotHitEvent = true;
				}
			}
			m_launched = true;
		}
		if (bounceCount == 0 && !m_gotHitEvent)
		{
			do
			{
				if (m_cancelled)
				{
					yield break;
				}
				yield return null;
				m_launchTimer -= Time.deltaTime;
			}
			while (!m_gotHitEvent && m_launchTimer >= 0f);
			if (!m_gotHitEvent)
			{
				Debug.LogError("Error with " + base.Owner.name + "'s attack " + base.name + " (attack" + AttackVariation.ToString("#0.##") + ") didn't receive anim event after " + GetLaunchTimerDuration().ToString("####.00") + " seconds.");
				Cancel();
			}
		}
		if (m_cancelled)
		{
			yield break;
		}
		m_can_cancel = false;
		TriggerNoise();
		if (bounceCount == 0 && base.AbilityOrigin != null)
		{
			if (enemy != null)
			{
				base.AbilityOrigin.Activate(enemy);
			}
			else
			{
				base.AbilityOrigin.Activate(desiredDestination);
			}
		}
		bool canHitOwner = false;
		if (ApplyToSelfOnly || IsValidTarget(base.Owner))
		{
			bool flag = false;
			if (this is AttackAOE)
			{
				flag = (this as AttackAOE).ExcludeTarget;
			}
			if (!flag)
			{
				if (ApplyToSelfOnly)
				{
					canHitOwner = true;
				}
				else if (enemy == base.Owner)
				{
					canHitOwner = true;
				}
				else if (enemy == null && bounceCount > 0)
				{
					canHitOwner = true;
				}
			}
		}
		DecrementAmmo();
		if ((bool)m_ownerStats && !m_hitFrameNotified)
		{
			m_hitFrameNotified = true;
			m_ownerStats.NotifyHitFrame(enemy, new DamageInfo(null, 0f, this));
		}
		ProjectileLaunch(launchPos, desiredDestination, enemy, bounceCount, canHitOwner, distanceTraveled, bounceList);
	}

	protected virtual void DecrementAmmo()
	{
	}

	private float GetLaunchTimerDuration()
	{
		return Mathf.Max(((AttackVariation == 0) ? 0f : 1.5f) + base.AttackSpeedTime, 0.066f) * (1f / CalculateAttackSpeed());
	}

	public void ProjectileLaunch(Vector3 launchPos, Vector3 desiredDestination, GameObject enemy, int bounceCount, bool canHitOwner)
	{
		ProjectileLaunch(launchPos, desiredDestination, enemy, bounceCount, canHitOwner, 0f, null);
	}

	public void ProjectileLaunch(Vector3 launchPos, Vector3 desiredDestination, GameObject enemy, int bounceCount, bool canHitOwner, float distanceTraveled, List<GameObject> bounceList)
	{
		m_can_cancel = false;
		TriggerNoise();
		Stealth.SetInStealthMode(base.Owner, inStealth: false);
		Transform transform = null;
		if (bounceCount == 0 && base.Owner != null && m_ability != null && m_ability is GenericSpell && (m_ability as GenericSpell).SpellClass == CharacterStats.Class.Wizard)
		{
			transform = AttackBase.GetTransform(base.Owner, EffectAttachType.RightHand);
		}
		Transform launchTransform = base.LaunchTransform;
		if (transform != null)
		{
			GameUtilities.LaunchEffect(OnLaunchVisualEffect, 1f, launchPos, m_ability);
			if (!OverridesProjectileLaunchPosition)
			{
				launchPos = transform.position;
			}
		}
		else if (bounceCount == 0 && launchTransform != null && !canHitOwner)
		{
			GameUtilities.LaunchEffect(OnLaunchVisualEffect, 1f, launchTransform, m_ability);
			if (!OverridesProjectileLaunchPosition)
			{
				launchPos = launchTransform.position;
			}
		}
		else
		{
			GameUtilities.LaunchEffect(OnLaunchVisualEffect, 1f, launchPos, m_ability);
		}
		m_destination = desiredDestination;
		if (!MultiHitRay)
		{
			m_enemy = enemy;
		}
		else
		{
			desiredDestination.y = launchPos.y;
		}
		if (ProjectilePrefab != null)
		{
			int num = 0;
			Vector3 normalized = (desiredDestination - launchPos).normalized;
			if (MultiHitRay)
			{
				normalized.y = 0f;
				normalized.Normalize();
			}
			Quaternion dir = Quaternion.FromToRotation(Vector3.forward, normalized);
			foreach (Quaternion item in LaunchAngles(dir, bounceCount))
			{
				Projectile projectile = UnityEngine.Object.Instantiate(ProjectilePrefab, launchPos, Quaternion.identity);
				if (!(projectile != null))
				{
					continue;
				}
				projectile.name += num.ToString("D3");
				projectile.SetBounceCount(bounceCount);
				projectile.BounceObjects = bounceList;
				projectile.CanHitOwner = canHitOwner;
				projectile.gameObject.layer = base.gameObject.layer;
				projectile.gameObject.SetActive(value: true);
				projectile.OnImpactAttack = this;
				projectile.IsDestructible = true;
				projectile.TotalDistanceTraveled = distanceTraveled;
				Vector3 vector = desiredDestination;
				if (MultiHitRay)
				{
					vector = launchPos + item * Vector3.forward * AdjustedMultiHitDist;
				}
				Weapon component = base.gameObject.GetComponent<Weapon>();
				if (component != null)
				{
					GameUtilities.LaunchLoopingEffect(component.OnEquipVisualEffect, 1f, projectile.transform, m_ability);
					component.ApplyItemModEquipFX(projectile.transform, null);
				}
				if ((bool)m_enemy)
				{
					projectile.Launch(launchPos, item * Vector3.forward, m_enemy);
				}
				else if (MultiHitRay)
				{
					Vector3 normalized2;
					if (bounceCount == 0)
					{
						Vector3 vector2 = new Vector3(0f, 1f, 0f);
						normalized2 = (vector + vector2 - launchPos).normalized;
					}
					else
					{
						normalized2 = (vector - launchPos).normalized;
					}
					projectile.Launch(launchPos, normalized2);
				}
				else
				{
					projectile.LaunchAtPoint(launchPos, vector);
				}
				num++;
				ActiveProjectileCount++;
			}
			m_numImpacts = num;
		}
		else
		{
			Debug.LogError(base.name + " doesn't have a projectile set!", this);
		}
	}

	public IEnumerable LaunchAngles(Quaternion dir, int bounceCount)
	{
		int count = ProjectileCount;
		if (bounceCount > 0)
		{
			count = 1;
		}
		else if (m_ownerStats != null)
		{
			Weapon component = base.gameObject.GetComponent<Weapon>();
			if (component != null)
			{
				foreach (StatusEffect item in m_ownerStats.FindStatusEffectsOfType(StatusEffect.ModifiedStat.ExtraProjectilesByWeaponType))
				{
					Weapon weapon = item.Params.EquippablePrefab as Weapon;
					if ((bool)weapon && component.WeaponType == weapon.WeaponType)
					{
						count += (int)item.CurrentAppliedValue;
					}
				}
			}
		}
		float angleSpread = ProjectileConeAngle / (float)count;
		float nextAngle = 0f;
		int start = 0;
		if (count % 2 == 0)
		{
			nextAngle = angleSpread;
			start = 1;
		}
		for (int i = 0; i < count; i++)
		{
			float mult = (((i + start) % 2 == 0) ? 1f : (-1f));
			yield return dir * Quaternion.AngleAxis(nextAngle * mult, Vector3.up);
			if (mult > 0f)
			{
				nextAngle += angleSpread;
			}
		}
	}

	private void anim_OnEventHit(object sender, EventArgs e)
	{
		m_gotHitEvent = true;
		m_launched = true;
		m_attachedToRangedHit = false;
		AnimationController component = (sender as GameObject).GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.OnEventHit -= anim_OnEventHit;
		}
	}

	public override void Interrupt()
	{
		if (m_attachedToRangedHit)
		{
			AnimationController component = base.Owner.GetComponent<AnimationController>();
			if ((bool)component)
			{
				component.OnEventHit -= anim_OnEventHit;
			}
			m_attachedToRangedHit = false;
		}
		ClearLaunchFlags();
		base.Interrupt();
	}

	public override void HandleBouncing(GameObject self, GameObject enemy)
	{
		if (!MultiHitRay)
		{
			ProjectileIsDestructible(self, destructible: true);
			GameObject gameObject = FindBounceTarget(enemy, self);
			if (gameObject == null)
			{
				ResetBounceCount(self);
			}
			else
			{
				IncrementBounceCount(self);
				Transform hitTransform = GetHitTransform(gameObject);
				StartCoroutine(Launch(enemy.transform.position, hitTransform.position, gameObject, GetBounceCount(self), GetProjectileDistanceTraveled(self), GetBounceList(self)));
			}
			DestroyProjectile(self);
		}
	}

	protected void CheckBouncing(GameObject self, Vector3 wallNormal)
	{
		if (ExtraAOEOnBounce)
		{
			Transform transform = self.transform;
			AttackAOE attackAOE = UnityEngine.Object.Instantiate(ExtraAOE);
			attackAOE.DestroyAfterImpact = true;
			attackAOE.Owner = base.Owner;
			attackAOE.transform.parent = transform;
			attackAOE.SkipAnimation = true;
			attackAOE.m_ability = m_ability;
			attackAOE.ShowImpactEffect(transform.position);
			attackAOE.OnImpactShared(null, transform.position, null);
		}
		if (Bounces == 0 && (self == null || self.GetComponent<Projectile>() == null))
		{
			ResetBounceCount(self);
			ClearLaunchFlags();
			return;
		}
		if (base.MaxBounces == GetBounceCount(self))
		{
			ResetBounceCount(self);
			ClearLaunchFlags();
			return;
		}
		float bounceDelay = BounceDelay;
		if (self != null)
		{
			Projectile component = self.GetComponent<Projectile>();
			if (component != null)
			{
				component.PrepareBounceWall(bounceDelay, wallNormal);
				return;
			}
		}
		m_bounceTimer = bounceDelay;
		m_bounceSelf = self;
		m_bounceEnemy = null;
		m_bounceNormal = wallNormal;
		ProjectileIsDestructible(self, destructible: false);
		CheckBounceTimer();
	}

	public virtual void HandleBouncing(GameObject self, Vector3 wallNormal)
	{
		if (!(self == null))
		{
			ProjectileIsDestructible(self, destructible: true);
			Projectile component = self.GetComponent<Projectile>();
			if (component == null)
			{
				ResetBounceCount(self);
			}
			else
			{
				Transform transform = self.transform;
				Vector3 velocity = component.Velocity;
				velocity.Normalize();
				velocity = Vector3.Reflect(velocity, wallNormal);
				velocity.Normalize();
				Vector3 launchPos = transform.position + velocity * ((component.GetComponent<Collider>() as SphereCollider).radius + 0.1f);
				IncrementBounceCount(self);
				StartCoroutine(Launch(launchPos, velocity * AdjustedMultiHitDist + transform.position, null, GetBounceCount(self), GetProjectileDistanceTraveled(self), GetBounceList(self)));
			}
			DestroyProjectile(self);
		}
	}

	public void ClearLaunchFlags()
	{
		m_gotHitEvent = false;
		m_launched = false;
	}

	public override void OnImpact(GameObject projectile, Vector3 hitPosition)
	{
		ClearLaunchFlags();
		if (m_cancelled)
		{
			return;
		}
		m_can_cancel = false;
		TriggerNoise();
		if (m_enemy != null && RequiresHitObject)
		{
			return;
		}
		ShowImpactEffect(hitPosition);
		base.OnImpact(projectile, hitPosition);
		if (projectile != null && MultiHitRay)
		{
			Projectile component = projectile.GetComponent<Projectile>();
			if ((bool)component && (bool)component.GetComponent<Collider>())
			{
				Vector3 velocity = component.Velocity;
				float magnitude = velocity.magnitude;
				velocity.Normalize();
				Vector3 vector = velocity * (0f - (component.GetComponent<Collider>() as SphereCollider).radius) * 1.1f;
				vector = projectile.transform.position + vector;
				Vector3 forward = Vector3.forward;
				if (Physics.SphereCast(vector, (component.GetComponent<Collider>() as SphereCollider).radius, velocity, out var hitInfo, magnitude * 1f))
				{
					forward = hitInfo.normal;
					CheckBouncing(projectile, forward);
				}
				else
				{
					velocity *= -1f;
					CheckBouncing(projectile, forward);
				}
			}
		}
		if (m_ability != null && !(this is AttackPulsedAOE))
		{
			m_ability.AttackComplete = true;
		}
	}

	public override void OnImpact(GameObject projectile, GameObject enemy)
	{
		ClearLaunchFlags();
		if (m_cancelled)
		{
			return;
		}
		m_can_cancel = false;
		TriggerNoise();
		Projectile component = projectile.GetComponent<Projectile>();
		if ((m_enemy != null && m_enemy != enemy) || (bool)enemy.GetComponent<Projectile>())
		{
			return;
		}
		if (enemy.GetComponent<Faction>() == null && component != null)
		{
			Vector3 velocity = component.Velocity;
			float magnitude = velocity.magnitude;
			velocity.Normalize();
			Vector3 vector = velocity * (0f - magnitude) * 0.5f;
			vector = projectile.transform.position + vector;
			Vector3 forward = Vector3.forward;
			forward = ((!Physics.SphereCast(vector, (component.GetComponent<Collider>() as SphereCollider).radius, velocity, out var hitInfo, magnitude * 2f)) ? (velocity * -1f) : hitInfo.normal);
			CheckBouncing(projectile, forward);
			return;
		}
		if (!MultiHitRay && m_enemy == null)
		{
			if ((m_destination - projectile.transform.position).sqrMagnitude < 0.5f)
			{
				OnImpact(projectile, projectile.transform.position);
			}
			return;
		}
		ShowImpactEffect(enemy.transform.position);
		if (enemy != base.Owner || (component != null && component.CanHitOwner))
		{
			Health component2 = enemy.GetComponent<Health>();
			if (component2 == null || !component2.Dead || TargetTypeUtils.ValidTargetDead(ValidTargets))
			{
				base.OnImpact(projectile, enemy);
				return;
			}
		}
		m_numImpacts--;
		if (m_numImpacts <= 0)
		{
			CleanUpAttack(enemy);
			if (m_ability != null && !(this is AttackPulsedAOE))
			{
				m_ability.AttackComplete = true;
			}
		}
	}

	public virtual void OnImpactForceTarget(GameObject projectile, GameObject enemy)
	{
		m_enemy = enemy;
		OnImpact(projectile, enemy);
	}

	public virtual void ShowImpactEffect(Vector3 position)
	{
		GameUtilities.LaunchEffect(ImpactEffect, 1f, position, m_ability);
	}

	public virtual void DestroyProjectile(GameObject projectile)
	{
		if (projectile != null)
		{
			Projectile component = projectile.GetComponent<Projectile>();
			if (component != null && component.IsDestructible)
			{
				component.TurnOffProjectile();
				GameUtilities.Destroy(projectile, component.DestroyDelayTime);
			}
		}
	}

	private Transform FindBone(Transform current, string name)
	{
		if (current.name == name)
		{
			return current;
		}
		for (int i = 0; i < current.childCount; i++)
		{
			Transform transform = FindBone(current.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public override float GetTotalAttackDistance(GameObject character)
	{
		float num = base.GetTotalAttackDistance(character);
		if (character != null)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			if (component != null && !IgnoreCharacterStats)
			{
				num = num * component.RangedAttackDistanceMultiplier * component.StatRangedAttackDistanceMultiplier;
			}
		}
		return num;
	}

	public float GetAdjustedMultiHitDist(GameObject character)
	{
		float num = MultiHitTravelDist;
		if ((bool)character)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			if (component != null && !IgnoreCharacterStats)
			{
				num *= component.StatEffectRadiusMultiplier * component.AoERadiusMult;
			}
		}
		return num;
	}

	protected override string GetRangeString(GenericAbility ability, GameObject character)
	{
		if (ApplyToSelfOnly || !PathsToPos)
		{
			return "";
		}
		if (AttackDistance <= 1f)
		{
			return "";
		}
		if ((bool)ability && (ability.Modal || ability.Passive))
		{
			return "";
		}
		float totalAttackDistance = GetTotalAttackDistance(character);
		string text = TextUtils.FormatBase(AttackDistance, totalAttackDistance, (float v) => GUIUtils.Format(1533, v.ToString("#0.##")));
		string text2 = "";
		if (ProjectileCount > 1)
		{
			text2 = AttackBase.FormatWC(GUIUtils.GetText(1755), ProjectileCount);
		}
		return (text + "\n" + text2).Trim();
	}

	protected override string GetAoeString(GenericAbility ability, GameObject character)
	{
		string text = base.GetAoeString(ability, character);
		if (MultiHitRay)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += " + ";
			}
			GameObject character2 = (character ? character : base.Owner);
			text += GUIUtils.Format(1541, TextUtils.FormatBase(MultiHitTravelDist, GetAdjustedMultiHitDist(character2), (float l) => GUIUtils.Format(1533, l.ToString("#0.#"))));
		}
		return text.Trim();
	}
}
