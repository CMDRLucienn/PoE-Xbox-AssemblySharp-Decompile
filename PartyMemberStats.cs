using UnityEngine;

public class PartyMemberStats : MonoBehaviour
{
	[Persistent]
	public EternityTimeInterval TimeInParty = new EternityTimeInterval();

	[Persistent]
	public EternityTimeInterval TimeInCombat = new EternityTimeInterval();

	[Persistent]
	public float TotalDamageDone;

	[Persistent]
	public float MaxSingleTargetDamage;

	[Persistent]
	public int EnemiesDefeated;

	[Persistent]
	public float MaxGroupDamage;

	[Persistent]
	public int CriticalHits;

	[Persistent]
	public int TotalHits;

	[Persistent]
	public float DamageTaken;

	[Persistent]
	public int TimesKOed;

	[Persistent]
	public int MostPowerfulEnemyLevel;

	[Persistent]
	public Gender MostPowerfulEnemyGender = Gender.Neuter;

	[Persistent]
	public DatabaseString MostPowerfulEnemyName;

	public void Restored()
	{
		if (TimeInParty == null)
		{
			TimeInParty = new EternityTimeInterval();
		}
		if (TimeInCombat == null)
		{
			TimeInCombat = new EternityTimeInterval();
		}
		TotalDamageDone = Mathf.Max(0f, TotalDamageDone);
		MaxSingleTargetDamage = Mathf.Max(0f, MaxSingleTargetDamage);
		MaxGroupDamage = Mathf.Max(0f, MaxGroupDamage);
		DamageTaken = Mathf.Max(0f, DamageTaken);
	}

	public void NotifyHit(GameObject source, CombatEventArgs args)
	{
		TotalDamageDone += args.Damage.FinalAdjustedDamage;
		MaxSingleTargetDamage = Mathf.Max(args.Damage.FinalAdjustedDamage, MaxSingleTargetDamage);
		if (args.Damage.IsCriticalHit)
		{
			CriticalHits++;
		}
		else if (args.Damage.IsPlainHit)
		{
			TotalHits++;
		}
	}

	public void NotifyKill(GameObject me, GameEventArgs args)
	{
		EnemiesDefeated++;
		CharacterStats component = args.GameObjectData[0].GetComponent<CharacterStats>();
		if ((bool)component && component.ScaledLevel > MostPowerfulEnemyLevel)
		{
			MostPowerfulEnemyLevel = component.ScaledLevel;
			MostPowerfulEnemyGender = component.Gender;
			MostPowerfulEnemyName = component.DisplayName;
		}
	}

	public void NotifyUnconscious(GameObject me, GameEventArgs args)
	{
		TimesKOed++;
	}

	public void NotifyDamaged(GameObject me, GameEventArgs args)
	{
		DamageTaken += args.FloatData[0];
	}

	private void Update()
	{
		if ((bool)WorldTime.Instance)
		{
			if (TimeInParty != null)
			{
				TimeInParty.AddSeconds(WorldTime.Instance.FrameWorldSeconds);
			}
			if (GameState.InCombat)
			{
				TimeInCombat.AddSeconds(WorldTime.Instance.FrameWorldSeconds);
			}
		}
	}
}
