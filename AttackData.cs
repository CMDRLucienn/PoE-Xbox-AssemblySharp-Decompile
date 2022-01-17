using UnityEngine;

public class AttackData : MonoBehaviour
{
	public enum InterruptScale
	{
		None,
		Weakest,
		Weaker,
		Weak,
		Average,
		Strong,
		Stronger,
		Strongest
	}

	public float WeakestInterrupt;

	public float Weaker;

	public float Weak;

	public float Average;

	public float Strong;

	public float Stronger;

	public float StrongestInterrupt;

	public float BaseConcentration;

	[Range(0.1f, 2f)]
	public float RecoveryFactor = 1f;

	[Range(0f, 1f)]
	public float MovingRecoveryMult;

	public float InstantAttackSpeed;

	public float ShortAttackSpeed = 3f;

	public float LongAttackSpeed = 6f;

	public float Single1HWeapRecovFactor = 2f;

	public int Single1HWeapNoShieldAccuracyBonus = 15;

	public float MinDamagePercent = 40f;

	public float MinCrushingDamagePercent = 50f;

	public float DamageIntervalRate = 1f;

	public float HazardIntervalRate = 1f;

	public float PulseAoeIntervalRate = 3f;

	public float TrapIntervalRate = 1f;

	[Tooltip("The length of time in seconds that a character must wait after issueing a disengagement attack, before he can engage the same target again.")]
	public float ReengagementWaitPeriod = 2f;

	[Tooltip("Minimum time in seconds for NPCs to wait before reevaluating a new target while moving.")]
	public float MinTargetReevaluationTime = 1f;

	[Tooltip("Maximum time in seconds for NPCs to wait before reevaluating a new target while moving.")]
	public float MaxTargetReevaluationTime = 1.5f;

	public float SwitchWeaponRecoveryTime = 2f;

	public float GrimoireCooldownTime = 10f;

	public Affliction[] SneakAttackAfflictions;

	public Affliction[] InterruptBlockingAfflictions;

	public Affliction FrightenedAffliction;

	public Affliction TerrifiedAffliction;

	public Affliction SickenedAffliction;

	public Affliction WeakenedAffliction;

	public Affliction HobbledAffliction;

	public Affliction StuckAffliction;

	public Affliction DazedAffliction;

	public Affliction ConfusedAffliction;

	public Affliction BondedGriefAffliction;

	public GameObject DefaultBlood;

	public GameObject DefaultFireFx;

	public GameObject DefaultFreezeFx;

	public GameObject DefaultShockFx;

	public GameObject DefaultCorrodeFx;

	public GameObject DefaultBloodGib;

	public GameObject DefaultFireGib;

	public GameObject DefaultFreezeGib;

	public GameObject DefaultShockGib;

	public GameObject DefaultCorrodeGib;

	[Tooltip("The default gib list for the following races : Humans, Elf, Dwarf, Godlike, Orlans, Aumaua")]
	public ObjectList DefaultGibList;

	[Tooltip("Percent value")]
	public int BaseSuspicionRate = 20;

	[Tooltip("Percent value")]
	public int StealthDecayRate = 15;

	[Tooltip("Delay in seconds before stealth meter begins to decay")]
	public float StealthDecayDelay = 0.75f;

	[Tooltip("The max acceleration rate for gaining suspicion during stealth")]
	public float MaxSuspicionStealthAccelerationMultiplier = 4f;

	[Tooltip("The max deceleration rate for gaining suspicion during stealth")]
	public float MaxSuspicionStealthDecelerationMultiplier = 0.05f;

	[Tooltip("The max penalty for stealth distance (target is close)")]
	public float MinDistanceMultiplier = 2f;

	[Tooltip("The min bonus for stealth distance (target is far)")]
	public float MaxDistanceMultiplier = 0.5f;

	[Tooltip("The distance Supernatural characters will see enemies in stealth mode.")]
	public float Supernatural = 9f;

	[Tooltip("The distance Keen characters will see enemies in stealth mode.")]
	public float Keen = 6f;

	[Tooltip("The distance Normal characters will see enemies in stealth mode.")]
	public float Normal = 5f;

	[Tooltip("The distance Poor characters will see enemies in stealth mode.")]
	public float Poor = 3f;

	[Tooltip("The distance Oblivious characters will see enemies in stealth mode.")]
	public float Oblivious = 1f;

	[Tooltip("The amount of focus a cipher gains per point of weapon damage dealt")]
	public float FocusPerWeaponDamageDealt = 0.25f;

	public float ChanterPhraseRadius = 4f;

	public GameObject DefaultRiposteFx;

	public GameObject DefaultDisengagementFx;

	public float StoryTimeMinimumRollToCrit = 126f;

	public float MinimumRollToCrit = 94f;

	public float MinimumRollToGraze = 25f;

	public float CritDamageMult = 1.5f;

	public float GrazeDamageMult = 0.5f;

	public int AccuracyPerLevel = 3;

	public int DefensePerLevel = 3;

	public float Slope => MaxDistanceMultiplier - MinDistanceMultiplier;

	public static AttackData Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'AttackData' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static float InterruptDuration(InterruptScale scale)
	{
		return scale switch
		{
			InterruptScale.Weakest => Instance.WeakestInterrupt, 
			InterruptScale.Weaker => Instance.Weaker, 
			InterruptScale.Weak => Instance.Weak, 
			InterruptScale.Average => Instance.Average, 
			InterruptScale.Strong => Instance.Strong, 
			InterruptScale.Stronger => Instance.Stronger, 
			InterruptScale.Strongest => Instance.StrongestInterrupt, 
			_ => 0f, 
		};
	}

	public static GameObject GetDefaultEffect(DamagePacket.DamageType type)
	{
		GameObject result = null;
		switch (type)
		{
		case DamagePacket.DamageType.Burn:
			result = Instance.DefaultFireFx;
			break;
		case DamagePacket.DamageType.Freeze:
			result = Instance.DefaultFreezeFx;
			break;
		case DamagePacket.DamageType.Shock:
			result = Instance.DefaultShockFx;
			break;
		case DamagePacket.DamageType.Corrode:
			result = Instance.DefaultCorrodeFx;
			break;
		case DamagePacket.DamageType.Slash:
		case DamagePacket.DamageType.Crush:
		case DamagePacket.DamageType.Pierce:
			result = Instance.DefaultBlood;
			break;
		}
		return result;
	}

	public static GameObject GetDefaultGibEffect(DamagePacket.DamageType type)
	{
		GameObject result = null;
		switch (type)
		{
		case DamagePacket.DamageType.Burn:
			result = Instance.DefaultFireGib;
			break;
		case DamagePacket.DamageType.Freeze:
			result = Instance.DefaultFreezeGib;
			break;
		case DamagePacket.DamageType.Shock:
			result = Instance.DefaultShockGib;
			break;
		case DamagePacket.DamageType.Corrode:
			result = Instance.DefaultCorrodeGib;
			break;
		case DamagePacket.DamageType.Slash:
		case DamagePacket.DamageType.Crush:
		case DamagePacket.DamageType.Pierce:
			result = Instance.DefaultBloodGib;
			break;
		}
		return result;
	}

	public static float GetPerceptionDistance(CharacterStats.PerceptionAdjustment perception)
	{
		if (Instance == null)
		{
			return 1f;
		}
		return perception switch
		{
			CharacterStats.PerceptionAdjustment.Supernatural => Instance.Supernatural, 
			CharacterStats.PerceptionAdjustment.Keen => Instance.Keen, 
			CharacterStats.PerceptionAdjustment.Normal => Instance.Normal, 
			CharacterStats.PerceptionAdjustment.Poor => Instance.Poor, 
			CharacterStats.PerceptionAdjustment.Oblivious => Instance.Oblivious, 
			_ => 1f, 
		};
	}

	public static ObjectList GetDefaultGibList(CharacterStats.Race race)
	{
		if ((uint)(race - 1) <= 4u || race == CharacterStats.Race.Aumaua)
		{
			if ((bool)Instance)
			{
				return Instance.DefaultGibList;
			}
			return null;
		}
		return null;
	}
}
