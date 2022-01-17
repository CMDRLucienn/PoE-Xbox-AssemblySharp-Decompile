using System.Collections.Generic;
using AI.Achievement;
using AI.Pet;
using AI.Plan;
using UnityEngine;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("AI/Package Controller")]
public class AIPackageController : AIController
{
	public enum PackageType
	{
		None,
		DefaultAI,
		TownGuard,
		SpellCaster,
		Pet
	}

	[Persistent]
	public bool Patroller;

	public PackageType AIPackage = PackageType.DefaultAI;

	[Tooltip("If this is set, the character will always update even when far from the party.")]
	public bool OptimizeUpdates = true;

	public SpellList InstructionSet;

	public float CooldownBetweenSpells;

	public GameObject PreferredPatrolPoint;

	[Tooltip("When true, the character must die for combat to end.")]
	public bool CombatEndsOnDeath;

	[Tooltip("If true, the character will try to stay near his original start position when not patrolling.")]
	public bool Tethered = true;

	[Tooltip("NPCs who are tethered will try to stay within this distance of their start location.")]
	public float TetherDistance = 25f;

	protected float m_timeSinceUpdate;

	protected float m_updateInterval = 0.1f;

	protected List<SpellCastData> m_instructions = new List<SpellCastData>();

	protected SpellList.InstructionSelectionType m_instructionSelectionType;

	protected TargetPreference m_defaultTargetPreference;

	protected bool m_is_onscreen_or_nearby;

	protected bool m_isFogVisible;

	protected AlphaControl m_alphaControl;

	public bool IsFogVisible => m_isFogVisible;

	public override List<SpellCastData> Instructions => m_instructions;

	public override SpellList.InstructionSelectionType InstructionSelectionType => m_instructionSelectionType;

	public override TargetPreference DefaultTargetPreference => m_defaultTargetPreference;

	public float CooldownTime => m_instructionTimer;

	[Persistent]
	public bool IsActive
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public override bool IsPet => AIPackage == PackageType.Pet;

	public override void InitAI()
	{
		m_updateInterval = OEIRandom.Range(0.5f, 1.25f);
		if (m_ai == null)
		{
			m_ai = AIStateManager.StateManagerPool.Allocate();
			m_ai.Owner = base.gameObject;
		}
		else
		{
			m_ai.AbortStateStack();
		}
		InitMover();
		switch (AIPackage)
		{
		case PackageType.None:
			m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<AI.Achievement.Idle>());
			break;
		case PackageType.DefaultAI:
		case PackageType.TownGuard:
		case PackageType.SpellCaster:
			if (InstructionSet == null || InstructionSet.Spells == null || InstructionSet.Spells.Length == 0)
			{
				m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<ScanForTarget>());
				break;
			}
			m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<CasterScanForTarget>());
			InitSpellCaster();
			break;
		case PackageType.Pet:
			m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<AI.Pet.Idle>());
			break;
		default:
			m_ai.SetDefaultState(AIStateManager.StatePool.Allocate<AI.Achievement.Idle>());
			return;
		}
		if (m_ai.Owner != null)
		{
			m_ai.AIController.RecordRetreatPosition(m_ai.Owner.transform.position);
		}
	}

	public override void OnEnable()
	{
		if (!PartyMemberAI.SafeEnableDisable)
		{
			base.OnEnable();
		}
	}

	public override void OnDisable()
	{
		if (!PartyMemberAI.SafeEnableDisable)
		{
			base.OnDisable();
		}
		if ((bool)m_alphaControl)
		{
			m_alphaControl.Alpha = 1f;
		}
	}

	public void ChangeBehavior(PackageType newPackage)
	{
		if (AIPackage != newPackage)
		{
			AIPackage = newPackage;
			InitAI();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Restored()
	{
		if (base.CurrentWaypoint != null && !(base.StateManager.CurrentState is Patrol))
		{
			Patrol patrol = AIStateManager.StatePool.Allocate<Patrol>();
			base.StateManager.PushState(patrol);
			patrol.StartPoint = base.CurrentWaypoint;
			patrol.TargetScanner = GetTargetScanner();
		}
	}

	public override void Update()
	{
		if (GameState.IsLoading)
		{
			return;
		}
		if (m_instructionTimer > 0f)
		{
			m_instructionTimer -= Time.deltaTime;
		}
		if (m_instructions != null)
		{
			for (int num = m_instructions.Count - 1; num >= 0; num--)
			{
				m_instructions[num].Update();
			}
		}
		if (FogOfWar.Instance != null)
		{
			bool flag = FogOfWar.Instance.PointVisible(base.transform.position);
			if (m_alphaControl == null)
			{
				m_alphaControl = base.gameObject.GetComponent<AlphaControl>();
				if (m_alphaControl == null)
				{
					m_alphaControl = base.gameObject.AddComponent<AlphaControl>();
				}
				m_alphaControl.Alpha = (flag ? 1f : 0f);
			}
			if (flag != m_isFogVisible || !m_alphaControl.IsFadeValid())
			{
				m_isFogVisible = flag;
				if (flag)
				{
					m_alphaControl.FadeIn(0.5f);
				}
				else
				{
					m_alphaControl.FadeOut(0.5f);
				}
			}
		}
		m_is_onscreen_or_nearby = m_isFogVisible;
		if (!m_is_onscreen_or_nearby && GameUtilities.NearestPlayerSquaredDist(base.gameObject.transform.position) < Mathf.Pow(PerceptionDistance * 3f, 2f))
		{
			m_is_onscreen_or_nearby = true;
		}
		if (!OptimizeUpdates || m_timeSinceUpdate > m_updateInterval || m_is_onscreen_or_nearby)
		{
			m_timeSinceUpdate = 0f;
			base.Update();
		}
		else
		{
			m_timeSinceUpdate += Time.deltaTime;
		}
	}

	public void InitSpellCaster()
	{
		CharacterStats component = GetComponent<CharacterStats>();
		if (InstructionSet == null)
		{
			ChangeBehavior(PackageType.DefaultAI);
			return;
		}
		int num = 0;
		bool flag = false;
		Persistence component2 = GetComponent<Persistence>();
		if ((bool)component2)
		{
			flag = component2.Mobile;
		}
		m_instructionSelectionType = InstructionSet.SelectionType;
		m_defaultTargetPreference = InstructionSet.DefaultTargetPreference.Clone() as TargetPreference;
		SpellCastData[] spells = InstructionSet.Spells;
		foreach (SpellCastData spellCastData in spells)
		{
			if (spellCastData == null)
			{
				continue;
			}
			SpellCastData spellCastData2 = spellCastData.Clone() as SpellCastData;
			if (spellCastData.Spell != null)
			{
				GenericAbility genericAbility = component.FindAbilityInstance(spellCastData.Spell);
				if ((bool)genericAbility)
				{
					spellCastData2.Spell = genericAbility;
				}
				else if (!GameState.LoadedGame || !flag)
				{
					spellCastData2.Spell = component.InstantiateAbility(spellCastData.Spell, GenericAbility.AbilityType.Ability);
				}
				if (spellCastData2.Spell is Chant)
				{
					ChanterTrait chanterTrait = component.GetChanterTrait();
					if ((bool)chanterTrait)
					{
						chanterTrait.DesiredChant = spellCastData2.Spell as Chant;
					}
				}
			}
			num += spellCastData.CastingPriority;
			m_instructions.Add(spellCastData2);
		}
		float num2 = num;
		foreach (SpellCastData instruction in m_instructions)
		{
			instruction.Odds = (float)instruction.CastingPriority / num2;
		}
		if (InstructionSet.SelectionType == SpellList.InstructionSelectionType.Random)
		{
			m_instructions.Sort();
		}
	}

	public override float GetCooldownBetweenSpells()
	{
		return CooldownBetweenSpells;
	}

	public override bool IsTethered()
	{
		if (!Tethered)
		{
			return false;
		}
		if (m_retreatPosition.sqrMagnitude < float.Epsilon)
		{
			return false;
		}
		if (Patroller && m_currentWaypoint != null && m_currentWaypoint.NextWaypoint != null)
		{
			return false;
		}
		return true;
	}

	public override float GetTetherDistanceSq()
	{
		return TetherDistance * TetherDistance;
	}

	public override bool ShouldCombatEndOnDeath()
	{
		return CombatEndsOnDeath;
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		CharacterStats component = GetComponent<CharacterStats>();
		if ((bool)component)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position, component.PerceptionDistance);
		}
	}
}
