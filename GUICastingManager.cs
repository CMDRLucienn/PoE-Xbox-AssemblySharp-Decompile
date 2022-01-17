using System;
using System.Collections.Generic;
using UnityEngine;

public class GUICastingManager : MonoBehaviour
{
	public enum ColorScheme
	{
		None,
		Friendly,
		Hostile,
		WeaponRange,
		HostileFoeOnly
	}

	private const float TARGET_REFRESH_TIME = 0.1f;

	private float m_TargetRefreshTime;

	[Tooltip("Width of the fade effect on the cast shape.")]
	public float FadeWidth = 0.5f;

	[Tooltip("Alpha of the center of the cast shape.")]
	public float MinAlpha = 0.4f;

	[Tooltip("Alpha of the edge of the cast shape.")]
	public float MaxAlpha = 0.6f;

	[Tooltip("Width of the fade effect on the cast shape, for weapon ranges.")]
	public float WeaponRangeFadeWidth = 0.25f;

	[Tooltip("Alpha of the center of the cast shape, for weapon ranges.")]
	public float WeaponRangeMinAlpha;

	[Tooltip("Alpha of the edge of the cast shape, for weapon ranges.")]
	public float WeaponRangeMaxAlpha = 0.3f;

	public float WeaponRangeRotSpeed = 0.2f;

	private float m_WeaponRangeAngle;

	private List<GameObject> m_AoeTargets = new List<GameObject>();

	private List<GameObject> m_AoeOldTargets = new List<GameObject>();

	public Color Color = Color.green;

	public Color ExtraRadiusColor = Color.blue;

	public Color HostileColor = Color.red;

	public Color HostileFoeOnlyColor = Color.yellow;

	public Color HostileExtraRadiusColor = Color.yellow;

	public Color WeaponRangeColor = Color.green;

	public bool DebugLock;

	private int m_AlphaProp;

	private TweenValue m_ValueTween;

	public static GUICastingManager Instance { get; private set; }

	public Material Material { get; private set; }

	public Material ExtraRadiusMaterial { get; private set; }

	public Material BeamMaterial { get; private set; }

	public Material HostileFoeOnlyBeamMaterial { get; private set; }

	public Material HostileBeamMaterial { get; private set; }

	public Material HostileMaterial { get; private set; }

	public Material HostileFoeOnlyMaterial { get; private set; }

	public Material HostileExtraRadiusMaterial { get; private set; }

	public Material WeaponRangeMaterial { get; private set; }

	private void Awake()
	{
		Instance = this;
		GameState.PersistAcrossSceneLoadsTracked(base.gameObject);
		m_ValueTween = GetComponent<TweenValue>();
		m_AlphaProp = Shader.PropertyToID("_Alpha");
		Material = new Material(Shader.Find("Trenton/UI/PE_InGameCastElement"));
		Material.color = Color;
		ExtraRadiusMaterial = new Material(Shader.Find("Trenton/UI/PE_InGameCastElement"));
		ExtraRadiusMaterial.color = ExtraRadiusColor;
		ExtraRadiusMaterial.renderQueue = 2002;
		BeamMaterial = new Material(Shader.Find("Trenton/UI/PE_InGameCastElement"));
		BeamMaterial.color = Color;
		BeamMaterial.renderQueue = 2001;
		HostileFoeOnlyBeamMaterial = new Material(BeamMaterial);
		HostileFoeOnlyBeamMaterial.color = HostileFoeOnlyColor;
		HostileBeamMaterial = new Material(BeamMaterial);
		HostileBeamMaterial.color = HostileColor;
		HostileMaterial = new Material(Material);
		HostileMaterial.color = HostileColor;
		HostileFoeOnlyMaterial = new Material(Material);
		HostileFoeOnlyMaterial.color = HostileFoeOnlyColor;
		HostileExtraRadiusMaterial = new Material(ExtraRadiusMaterial);
		HostileExtraRadiusMaterial.color = HostileExtraRadiusColor;
		HostileExtraRadiusMaterial.renderQueue = 2002;
		WeaponRangeMaterial = new Material(Material);
		WeaponRangeMaterial.color = WeaponRangeColor;
		WeaponRangeMaterial.renderQueue = 2003;
	}

	private void OnDestroy()
	{
		GUICastingCircle.ClearCastingPool();
		GUICastingBeam.ClearCastingPool();
		GUICastingWall.ClearCastingPool();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		Material.SetFloat(m_AlphaProp, m_ValueTween.Value);
		ExtraRadiusMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		BeamMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		HostileFoeOnlyBeamMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		HostileBeamMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		HostileMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		HostileFoeOnlyMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		HostileExtraRadiusMaterial.SetFloat(m_AlphaProp, m_ValueTween.Value);
		m_WeaponRangeAngle += WeaponRangeRotSpeed * Time.unscaledDeltaTime;
	}

	private void Begin()
	{
		GUICastingCircle.Begin();
		GUICastingBeam.Begin();
		GUICastingWall.Begin();
	}

	private void End()
	{
		if (!DebugLock)
		{
			GUICastingCircle.End();
			GUICastingBeam.End();
			GUICastingWall.End();
		}
	}

	private void WeaponRange(Vector3 position, float range)
	{
		GUICastingCircle.Create(position, Quaternion.AngleAxis(m_WeaponRangeAngle, Vector3.up), range, extended: false, 360f, ColorScheme.WeaponRange);
		WeaponRangeMaterial.SetFloat("_DashWidth", 0.07f);
		WeaponRangeMaterial.SetFloat("_DashRepeat", 0.09f / range);
	}

	private void Blast(Vector3 position, Quaternion forward, float radius, float extendedRadius, float angleDeg, ColorScheme scheme)
	{
		radius = Mathf.Min(radius, extendedRadius);
		GUICastingCircle.Create(position, forward, radius, extended: false, angleDeg, scheme);
		if (extendedRadius > radius)
		{
			GUICastingCircle.Create(position, forward, extendedRadius, extended: true, angleDeg, scheme);
		}
	}

	private void Circlecast(Vector3 origin, Quaternion rotation, float radius, float distance, ColorScheme scheme)
	{
		GUICastingBeam.Create(origin, rotation, radius, distance, scheme);
	}

	private void Wall(Vector3 from, Vector3 to, float radius, ColorScheme scheme)
	{
		GUICastingWall.Create(from, to, radius, scheme);
	}

	public void UpdateCasting(AttackBase castAttack, GenericAbility castAbility, PartyMemberAI castPartyMemberAI)
	{
		Begin();
		bool flag = false;
		m_TargetRefreshTime -= TimeController.sUnscaledDelta;
		if (m_TargetRefreshTime <= 0f)
		{
			flag = true;
			List<GameObject> aoeOldTargets = m_AoeOldTargets;
			m_AoeOldTargets = m_AoeTargets;
			m_AoeTargets = aoeOldTargets;
			m_AoeTargets.Clear();
		}
		try
		{
			GenericAbility hoveredAbility = UIAbilityBar.HoveredAbility;
			Item hoveredItem = UIAbilityBar.HoveredItem;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if ((bool)hoveredAbility)
			{
				castPartyMemberAI = UIAbilityBar.GetSelectedAIForBars();
				if (((bool)hoveredAbility.Attack && (bool)hoveredAbility.Attack.ForcedTarget) || hoveredAbility is Chant || hoveredAbility.IsAura)
				{
					flag4 = true;
					castAbility = hoveredAbility;
					castAttack = hoveredAbility.Attack;
				}
				else if (hoveredAbility.UsePrimaryAttack || hoveredAbility.UseFullAttack)
				{
					castAbility = hoveredAbility;
					flag2 = true;
					flag4 = true;
					flag3 = true;
				}
				else
				{
					castAbility = hoveredAbility;
					castAttack = hoveredAbility.Attack;
					flag3 = true;
				}
			}
			if ((bool)castAbility && (castAbility.UsePrimaryAttack || castAbility.UseFullAttack))
			{
				flag2 = true;
			}
			if (flag2)
			{
				GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
				Equipment equipment = (selectedForBars ? selectedForBars.GetComponent<Equipment>() : null);
				if ((bool)equipment)
				{
					castAttack = equipment.PrimaryAttack;
				}
				if (!castAttack.IsInRange(selectedForBars, GameCursor.CharacterUnderCursor, GameCursor.CharacterUnderCursor ? GameCursor.CharacterUnderCursor.transform.position : GameInput.WorldMousePosition))
				{
					flag4 = true;
				}
			}
			else if ((bool)hoveredItem)
			{
				flag4 = true;
				castPartyMemberAI = (castPartyMemberAI ? castPartyMemberAI : UIAbilityBar.GetSelectedAIForBars());
				castAttack = hoveredItem.GetComponent<AttackBase>();
			}
			else if ((bool)hoveredAbility)
			{
				flag4 = true;
				castPartyMemberAI = (castPartyMemberAI ? castPartyMemberAI : UIAbilityBar.GetSelectedAIForBars());
				castAttack = hoveredAbility.GetComponent<AttackBase>();
			}
			else if ((bool)castAttack && !castAttack.IsInRange(castPartyMemberAI.gameObject, GameCursor.CharacterUnderCursor, GameCursor.CharacterUnderCursor ? GameCursor.CharacterUnderCursor.transform.position : GameInput.WorldMousePosition))
			{
				flag4 = true;
			}
			if (!GameState.Option.AoeHighlighting)
			{
				return;
			}
			AttackAOE attackAOE = castAttack as AttackAOE;
			AttackRanged attackRanged = castAttack as AttackRanged;
			bool flag5 = false;
			if ((bool)castAttack && flag4 && castAttack.AttackDistance > 0f && castAttack.IsRangeUsed)
			{
				float num = castAttack.AttackDistance;
				if (castAttack is AttackMelee)
				{
					Mover component = castPartyMemberAI.GetComponent<Mover>();
					if ((bool)component)
					{
						num += component.Radius;
					}
				}
				if (castPartyMemberAI == null)
				{
					castPartyMemberAI = UIAbilityBar.GetSelectedAIForBars();
				}
				WeaponRange(castPartyMemberAI.transform.position, num);
			}
			if ((!hoveredAbility && (bool)castAbility && (GameState.s_playerCharacter.GetCastingCursor() == GameCursor.CursorType.CastAbilityInvalid || GameCursor.UiCursor != 0)) || !castPartyMemberAI || flag3)
			{
				return;
			}
			GetCastAoeHostility(castAttack, out var hostile, out var colorScheme);
			CreateWallAttack createWallAttack = castAttack as CreateWallAttack;
			if (createWallAttack != null)
			{
				createWallAttack.ComputeBounds(GameInput.WorldMousePosition, out var left, out var right);
				float radius = 0.5f;
				if ((bool)createWallAttack.TrapPrefab && (bool)createWallAttack.TrapPrefab.GetComponent<Collider>())
				{
					radius = ((createWallAttack.TrapPrefab.GetComponent<Collider>() is SphereCollider) ? (createWallAttack.TrapPrefab.GetComponent<Collider>() as SphereCollider).radius : ((!(createWallAttack.TrapPrefab.GetComponent<Collider>() is BoxCollider)) ? createWallAttack.TrapPrefab.GetComponent<Collider>().bounds.extents.x : ((createWallAttack.TrapPrefab.GetComponent<Collider>() as BoxCollider).size.x / 2f)));
				}
				Wall(left, right, radius, colorScheme);
				return;
			}
			if (castAbility is Chant)
			{
				CharacterStats component2 = castPartyMemberAI.GetComponent<CharacterStats>();
				Blast(castPartyMemberAI.transform.position, Quaternion.identity, AttackData.Instance.ChanterPhraseRadius, component2 ? component2.ChantRadius : AttackData.Instance.ChanterPhraseRadius, 360f, ColorScheme.HostileFoeOnly);
				return;
			}
			if ((bool)attackRanged && attackRanged.MultiHitRay)
			{
				Vector3 vector = ((attackRanged.RequiresHitObject && (bool)GameCursor.ObjectUnderCursor) ? GameCursor.ObjectUnderCursor.transform.position : GameInput.WorldMousePosition);
				Vector3 projectileLaunchPosition = attackRanged.GetProjectileLaunchPosition(vector);
				vector.y = projectileLaunchPosition.y;
				float num2 = 0.5f;
				if ((bool)attackRanged.ProjectilePrefab && (bool)attackRanged.ProjectilePrefab.GetComponent<Collider>())
				{
					num2 = ((!(attackRanged.ProjectilePrefab.GetComponent<Collider>() is SphereCollider)) ? attackRanged.ProjectilePrefab.GetComponent<Collider>().bounds.extents.x : (attackRanged.ProjectilePrefab.GetComponent<Collider>() as SphereCollider).radius);
				}
				if (attackRanged.ProjectileConeAngle >= 360f && attackRanged.ProjectileCount >= 3)
				{
					float num3 = num2 / Mathf.Sin((float)Math.PI / (float)attackRanged.ProjectileCount);
					Blast(projectileLaunchPosition, Quaternion.identity, num3, num3, 360f, colorScheme);
				}
				{
					foreach (Quaternion item in attackRanged.LaunchAngles(Quaternion.FromToRotation(Vector3.forward, (vector - projectileLaunchPosition).normalized), attackRanged.GetBounceCount(null)))
					{
						float num4 = attackRanged.AdjustedMultiHitDist;
						Vector3 vector2 = Vector3.up * 0.2f;
						int layerMask = LayerUtility.FindLayerMask("Wall") | LayerUtility.FindLayerMask("Doors");
						if (Physics.SphereCast(projectileLaunchPosition + vector2, num2, item * Vector3.forward + vector2, out var hitInfo, attackRanged.AdjustedMultiHitDist, layerMask))
						{
							num4 = hitInfo.distance;
						}
						Vector3 vector3 = projectileLaunchPosition + item * Vector3.forward * num4;
						if (flag)
						{
							for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
							{
								Faction faction = Faction.ActiveFactionComponents[i];
								if (faction == null || ((bool)castAttack && !castAttack.IsValidTarget(faction.gameObject)))
								{
									continue;
								}
								float num5 = num2 + faction.Mover.Radius;
								if (projectileLaunchPosition != castPartyMemberAI.transform.position && (faction.transform.position - projectileLaunchPosition).sqrMagnitude <= num5 * num5)
								{
									m_AoeTargets.Add(faction.gameObject);
									continue;
								}
								if ((faction.transform.position - vector3).sqrMagnitude <= num5 * num5)
								{
									m_AoeTargets.Add(faction.gameObject);
									continue;
								}
								float num6 = Vector3.Dot(item * Vector3.forward, faction.gameObject.transform.position - projectileLaunchPosition);
								if (num6 > 0f && num6 <= num4 && Vector3.Cross(item * Vector3.forward, faction.gameObject.transform.position - projectileLaunchPosition).sqrMagnitude <= num5 * num5)
								{
									m_AoeTargets.Add(faction.gameObject);
								}
							}
						}
						if (projectileLaunchPosition == castPartyMemberAI.transform.position)
						{
							Circlecast(projectileLaunchPosition, item, num2, num4, colorScheme);
						}
						else
						{
							Wall(projectileLaunchPosition, projectileLaunchPosition + item * Vector3.forward * num4, num2, colorScheme);
						}
						if (!attackRanged.ExtraAOE)
						{
							continue;
						}
						attackRanged.ExtraAOE.Owner = attackRanged.Owner;
						attackAOE = attackRanged.ExtraAOE;
						flag5 = attackAOE;
						GetCastAoeData(attackRanged.ExtraAOE, isExtra: true, out var hitPoint, out var forward, out var radius2, out var baseRadius, out var angle);
						if (!(forward != Vector3.zero))
						{
							continue;
						}
						GetCastAoeHostility(castAttack, out var _, out var colorScheme2);
						Blast(projectileLaunchPosition + item * Vector3.forward * num4, item, baseRadius, radius2, angle, colorScheme2);
						if (flag && (bool)attackAOE)
						{
							hitPoint = projectileLaunchPosition + item * Vector3.forward * num4;
							m_AoeTargets.AddRange(attackAOE.FindAoeTargets(attackAOE.Owner, forward, hitPoint, forUi: true));
							if ((bool)attackAOE.SecondAOE)
							{
								m_AoeTargets.AddRange(attackAOE.SecondAOE.FindAoeTargets(attackAOE.Owner, forward, hitPoint, forUi: true));
							}
						}
					}
					return;
				}
			}
			if ((bool)castPartyMemberAI && (bool)castAbility && castAbility.IsAura)
			{
				Blast(castPartyMemberAI.transform.position, Quaternion.identity, castAbility.FriendlyRadius, castAbility.AdjustedFriendlyRadius, 360f, ColorScheme.Friendly);
				if (flag)
				{
					m_AoeTargets.Add(castPartyMemberAI.gameObject);
					m_AoeTargets.AddRange(StatusEffect.GetAuraTargets(castPartyMemberAI.gameObject, castAbility.AdjustedFriendlyRadius));
				}
				return;
			}
			if (!attackAOE && (bool)castAttack && (bool)castAttack.ExtraAOE)
			{
				attackAOE = castAttack.ExtraAOE;
				flag5 = attackAOE;
				castAttack.ExtraAOE.Owner = castAttack.Owner;
				GetCastAoeHostility(attackAOE, out hostile, out colorScheme);
			}
			else if ((bool)castPartyMemberAI && (bool)castAttack && castAttack.IsValidPrimaryTarget(GameCursor.CharacterUnderCursor))
			{
				CharacterStats component3 = castPartyMemberAI.GetComponent<CharacterStats>();
				for (int j = 0; j < component3.ActiveAbilities.Count; j++)
				{
					Blast blast = component3.ActiveAbilities[j] as Blast;
					if ((bool)blast && blast.Activated)
					{
						if (blast.BlastApplies(castAttack))
						{
							attackAOE = blast.Attack as AttackAOE;
							flag5 = attackAOE;
						}
						continue;
					}
					Carnage carnage = component3.ActiveAbilities[j] as Carnage;
					if ((bool)carnage && carnage.Activated)
					{
						if (!carnage.CarnageApplies(castAttack))
						{
							continue;
						}
						Vector3 position = GameCursor.CharacterUnderCursor.transform.position;
						Mover component4 = GameCursor.CharacterUnderCursor.GetComponent<Mover>();
						float num7 = carnage.BaseCarnageRadius;
						float num8 = carnage.AdjustedCarnageRadius;
						if ((bool)component4)
						{
							num7 += component4.Radius;
							num8 += component4.Radius;
						}
						Blast(position, Quaternion.identity, num7, num8, 360f, ColorScheme.HostileFoeOnly);
						if (flag)
						{
							GameObject[] array = GameUtilities.CreaturesInRange(position, num8, castPartyMemberAI.gameObject, includeUnconscious: false);
							if (array != null)
							{
								m_AoeTargets.AddRange(array);
							}
						}
						continue;
					}
					PowderBurns powderBurns = component3.ActiveAbilities[j] as PowderBurns;
					if ((bool)powderBurns && powderBurns.Activated && powderBurns.Activated && powderBurns.PowderBurnsApplies(castAttack))
					{
						attackAOE = powderBurns.GetBurnForAttack(castAttack) as AttackAOE;
						if ((bool)attackAOE)
						{
							attackAOE.Owner = castPartyMemberAI.gameObject;
							flag5 = true;
						}
					}
				}
			}
			GetCastAoeHostility(attackAOE, out hostile, out colorScheme);
			flag5 = flag5 || ((bool)castAttack && attackAOE == castAttack.ExtraAOE);
			GetCastAoeData(attackAOE ?? castAttack, flag5, out var hitPoint2, out var forward2, out var radius3, out var baseRadius2, out var angle2);
			if (!(forward2 != Vector3.zero))
			{
				return;
			}
			Quaternion forward3 = Quaternion.LookRotation(forward2) * Quaternion.AngleAxis(-90f, Vector3.up);
			Blast(hitPoint2, forward3, baseRadius2, radius3, angle2, colorScheme);
			if (!flag)
			{
				return;
			}
			if ((bool)attackAOE)
			{
				m_AoeTargets.AddRange(attackAOE.FindAoeTargets(attackAOE.Owner, forward2, hitPoint2, forUi: true));
				if ((bool)attackAOE.SecondAOE)
				{
					m_AoeTargets.AddRange(attackAOE.SecondAOE.FindAoeTargets(attackAOE.Owner, forward2, hitPoint2, forUi: true));
				}
			}
			if ((bool)castAttack && (bool)castAttack.ExtraAOE)
			{
				m_AoeTargets.AddRange(castAttack.ExtraAOE.FindAoeTargets(castAttack.Owner, forward2, hitPoint2, forUi: true));
			}
		}
		finally
		{
			if (flag)
			{
				CharacterStats attacker = (castPartyMemberAI ? castPartyMemberAI.GetComponent<CharacterStats>() : null);
				if ((bool)castAttack)
				{
					if (castAttack.IsValidPrimaryTarget(GameCursor.CharacterUnderCursor))
					{
						m_AoeTargets.Add(GameCursor.CharacterUnderCursor);
					}
				}
				else if ((bool)castAbility)
				{
					m_AoeTargets.Add(castPartyMemberAI.gameObject);
				}
				for (int k = 0; k < m_AoeTargets.Count; k++)
				{
					GameObject gameObject = m_AoeTargets[k];
					if ((bool)gameObject)
					{
						HighlightCharacter component5 = gameObject.GetComponent<HighlightCharacter>();
						if ((bool)component5)
						{
							component5.Targeted = true;
						}
						UISuccessChanceManager.Instance.Show(attacker, gameObject, castAttack, castAbility);
					}
				}
				for (int l = 0; l < m_AoeOldTargets.Count; l++)
				{
					GameObject gameObject2 = m_AoeOldTargets[l];
					if ((bool)gameObject2 && !m_AoeTargets.Contains(gameObject2))
					{
						HighlightCharacter component6 = gameObject2.GetComponent<HighlightCharacter>();
						if ((bool)component6)
						{
							component6.Targeted = false;
						}
					}
				}
				UISuccessChanceManager.Instance.HideDead();
			}
			End();
		}
	}

	private void GetCastAoeHostility(AttackBase castAttack, out bool hostile, out ColorScheme colorScheme)
	{
		hostile = (bool)castAttack && castAttack.IsHostile(null, castAttack.DamageData);
		colorScheme = ((!hostile) ? ColorScheme.Friendly : ColorScheme.Hostile);
		if (colorScheme == ColorScheme.Hostile && castAttack.ValidTargetHostile() && !castAttack.ValidTargetAlly())
		{
			colorScheme = ColorScheme.HostileFoeOnly;
		}
	}

	private void GetCastAoeData(AttackBase castAttack, bool isExtra, out Vector3 hitPoint, out Vector3 forward, out float radius, out float baseRadius, out float angle)
	{
		AttackAOE attackAOE = castAttack as AttackAOE;
		HazardAttack hazardAttack = castAttack as HazardAttack;
		if ((bool)attackAOE || (bool)hazardAttack)
		{
			radius = 0f;
			baseRadius = 0f;
			angle = 0f;
			hitPoint = ((GameCursor.OverrideCharacterUnderCursor != null) ? GameCursor.OverrideCharacterUnderCursor.transform.position : GameInput.WorldMousePosition);
			Vector3 vector = hitPoint;
			if ((bool)attackAOE)
			{
				radius = Mathf.Max(radius, attackAOE.AdjustedBlastRadius);
				baseRadius = Mathf.Max(baseRadius, attackAOE.BlastRadius);
				angle = Mathf.Max(angle, attackAOE.DamageAngleDegrees);
			}
			else if ((bool)hazardAttack)
			{
				radius = Mathf.Max(radius, hazardAttack.LargestAuraRadius);
				baseRadius = Mathf.Max(baseRadius, hazardAttack.LargestAuraRadius);
				angle = 360f;
			}
			GameObject forcedTarget = castAttack.ForcedTarget;
			if ((bool)forcedTarget)
			{
				hitPoint = forcedTarget.transform.position;
				if (attackAOE.RequiresHitObject)
				{
					vector = GameCursor.CharacterUnderCursor.transform.position;
				}
			}
			else if ((isExtra || castAttack.RequiresHitObject) && (bool)GameCursor.CharacterUnderCursor)
			{
				hitPoint = GameCursor.CharacterUnderCursor.transform.position;
				vector = hitPoint;
			}
			if (angle > 0f && angle < 360f)
			{
				hitPoint = castAttack.Owner.transform.position;
			}
			if ((bool)castAttack && (bool)castAttack.Owner)
			{
				forward = vector - castAttack.Owner.transform.position;
				forward.y = 0f;
				if (forward.sqrMagnitude < float.Epsilon)
				{
					forward = castAttack.Owner.transform.forward;
				}
				forward.Normalize();
			}
			else
			{
				forward = Vector3.zero;
			}
		}
		else
		{
			hitPoint = Vector2.zero;
			forward = Vector2.zero;
			radius = 0f;
			baseRadius = 0f;
			angle = 0f;
		}
	}
}
