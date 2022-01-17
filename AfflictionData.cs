using System.Collections.Generic;
using UnityEngine;

public class AfflictionData : MonoBehaviour
{
	public Affliction FlankedPrefab;

	public Affliction MaimedPrefab;

	public Affliction PronePrefab;

	public Affliction MinorFatiguePrefab;

	public Affliction MajorFatiguePrefab;

	public Affliction CriticalFatiguePrefab;

	public Affliction CharmedPrefab;

	public Affliction DominatedPrefab;

	public Affliction ParalyzedPrefab;

	public Affliction PetrifiedPrefab;

	public Affliction StunnedPrefab;

	public Affliction UnconsciousPrefab;

	public float MinorFatigueHours;

	public float MajorFatigueHours;

	public float CriticalFatigueHours;

	public float CombatFatigueFormulaBase;

	public float CombatFatigueFormulaMult;

	public float ExtendedFatigueFormulaBase;

	public float ExtendedFatigueFormulaMult;

	[HideInInspector]
	public float TravelFatigueSoundTimer;

	public List<Affliction> AfflictionsForScripts = new List<Affliction>();

	public Affliction[] NormalInjuries;

	public Affliction BurnInjury;

	public Affliction ShockInjury;

	public Affliction FreezeInjury;

	public Affliction RawInjury;

	public CampEffectList SurvivalCampEffects;

	public static AfflictionData Instance { get; private set; }

	public static Affliction Flanked => Instance.FlankedPrefab;

	public static Affliction Maimed => Instance.MaimedPrefab;

	public static Affliction Prone => Instance.PronePrefab;

	public static Affliction Charmed => Instance.CharmedPrefab;

	public static Affliction Dominated => Instance.DominatedPrefab;

	public static Affliction Paralyzed => Instance.ParalyzedPrefab;

	public static Affliction Petrified => Instance.PetrifiedPrefab;

	public static Affliction Stunned => Instance.StunnedPrefab;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'AfflictionData' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Update()
	{
		if (TravelFatigueSoundTimer > 0f)
		{
			TravelFatigueSoundTimer -= Time.unscaledDeltaTime;
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

	public static Affliction GetFatigueAffliction(CharacterStats.FatigueLevel level)
	{
		return level switch
		{
			CharacterStats.FatigueLevel.Minor => Instance.MinorFatiguePrefab, 
			CharacterStats.FatigueLevel.Major => Instance.MajorFatiguePrefab, 
			CharacterStats.FatigueLevel.Critical => Instance.CriticalFatiguePrefab, 
			_ => null, 
		};
	}

	public static Affliction FindAfflictionForScript(string tag)
	{
		if (tag != null)
		{
			foreach (Affliction afflictionsForScript in Instance.AfflictionsForScripts)
			{
				if (afflictionsForScript.Tag == tag)
				{
					return afflictionsForScript;
				}
			}
		}
		return null;
	}
}
