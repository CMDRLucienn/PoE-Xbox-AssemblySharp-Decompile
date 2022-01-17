using System;
using UnityEngine;

public class EnvironmentalEffectsManager : MonoBehaviour
{
	public WeatherTemperatureNPCEffect[] WeatherTemperatureNPCEffects;

	private WeatherTemperature m_CurrentTemperature = WeatherTemperature.Neutral;

	public CharacterStats.Class[] GlobalClassExcludeListForTemperatureEffects;

	public CharacterStats.Race[] GlobalRaceExcludeListForTemperatureEffects;

	public GroundMaterialFootstepEffect[] GroundMaterialFootstepEffects;

	private void Start()
	{
		GameState.OnLevelLoaded += OnLevelLoaded;
		GameState.OnLevelUnload += OnLevelUnload;
		CharacterStats.s_OnCharacterStatsStart += OnCharacterStatsStart;
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
		GameState.OnLevelUnload -= OnLevelUnload;
		CharacterStats.s_OnCharacterStatsStart -= OnCharacterStatsStart;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!GameState.IsLoading)
		{
			WeatherTemperature currentMapTemperature = GetCurrentMapTemperature();
			if (currentMapTemperature != m_CurrentTemperature)
			{
				RemoveWeatherEffectsFromNPCs(m_CurrentTemperature);
				ApplyWeatherEffectsToNPCs(currentMapTemperature);
				m_CurrentTemperature = currentMapTemperature;
			}
		}
	}

	private WeatherTemperature GetCurrentMapTemperature()
	{
		if ((bool)GameState.Instance && GameState.Instance.CurrentMap != null && (bool)WorldTime.Instance)
		{
			if (WorldTime.Instance.IsCurrentlyDaytime())
			{
				return GameState.Instance.CurrentMap.DaytimeWeatherTemperature;
			}
			return GameState.Instance.CurrentMap.NighttimeWeatherTemperature;
		}
		return WeatherTemperature.Neutral;
	}

	private StatusEffectParams[] GetStatusEffectsForTemperature(WeatherTemperature temperature)
	{
		if (WeatherTemperatureNPCEffects == null)
		{
			return null;
		}
		for (int i = 0; i < WeatherTemperatureNPCEffects.Length; i++)
		{
			if (WeatherTemperatureNPCEffects[i].Temperature == temperature)
			{
				return WeatherTemperatureNPCEffects[i].StatusEffects;
			}
		}
		return null;
	}

	private GameObject[] GetEffectsForGroundMaterialFootstep(GroundMaterial material)
	{
		if (GroundMaterialFootstepEffects == null)
		{
			return null;
		}
		for (int i = 0; i < GroundMaterialFootstepEffects.Length; i++)
		{
			if (GroundMaterialFootstepEffects[i].GroundPlaneMaterial == material)
			{
				return GroundMaterialFootstepEffects[i].GroundEffects;
			}
		}
		return null;
	}

	private bool CharacterValidForWeatherEffects(CharacterStats stats)
	{
		if (stats == null)
		{
			return false;
		}
		if (GlobalClassExcludeListForTemperatureEffects != null)
		{
			for (int i = 0; i < GlobalClassExcludeListForTemperatureEffects.Length; i++)
			{
				if (GlobalClassExcludeListForTemperatureEffects[i] == stats.CharacterClass)
				{
					return false;
				}
			}
		}
		if (GlobalRaceExcludeListForTemperatureEffects != null)
		{
			for (int j = 0; j < GlobalRaceExcludeListForTemperatureEffects.Length; j++)
			{
				if (GlobalRaceExcludeListForTemperatureEffects[j] == stats.CharacterRace)
				{
					return false;
				}
			}
		}
		Health component = stats.GetComponent<Health>();
		if (!component || component.Dead)
		{
			return false;
		}
		if ((bool)stats.GetComponent<Ghost>())
		{
			return false;
		}
		NPCAppearance component2 = stats.GetComponent<NPCAppearance>();
		if ((bool)component2 && !component2.enabled)
		{
			return false;
		}
		return true;
	}

	private void OnWeatherEffectsApplied(CharacterStats stats)
	{
		Health component = stats.GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath += OnNPCDeath;
		}
	}

	private void OnWeatherEffectsRemoved(CharacterStats stats)
	{
		Health component = stats.GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath -= OnNPCDeath;
		}
	}

	private void ApplyWeatherEffectsToNPCs(WeatherTemperature Temperature)
	{
		StatusEffectParams[] statusEffectsForTemperature = GetStatusEffectsForTemperature(Temperature);
		if (statusEffectsForTemperature == null)
		{
			return;
		}
		CharacterStats[] array = UnityEngine.Object.FindObjectsOfType<CharacterStats>();
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				ApplyWeatherEffectsToNPC(array[i], statusEffectsForTemperature);
			}
		}
	}

	public void ApplyWeatherEffectsToNPC(GameObject gameObject)
	{
		if ((bool)gameObject)
		{
			CharacterStats component = gameObject.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				ApplyWeatherEffectsToNPC(component);
			}
		}
	}

	public void ApplyWeatherEffectsToNPC(CharacterStats stats)
	{
		if ((bool)stats)
		{
			StatusEffectParams[] statusEffectsForTemperature = GetStatusEffectsForTemperature(m_CurrentTemperature);
			if (statusEffectsForTemperature != null)
			{
				ApplyWeatherEffectsToNPC(stats, statusEffectsForTemperature);
			}
		}
	}

	private void ApplyWeatherEffectsToNPC(CharacterStats stats, StatusEffectParams[] effects)
	{
		if (!CharacterValidForWeatherEffects(stats))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < effects.Length; i++)
		{
			if (stats.CountStatusEffects(effects[i].Tag) == 0)
			{
				GenericAbility origin = null;
				StatusEffect statusEffect = StatusEffect.Create(stats.gameObject, origin, effects[i], GenericAbility.AbilityType.Ability, null, deleteOnClear: true);
				statusEffect.Params.Persistent = false;
				stats.ApplyStatusEffect(statusEffect);
				flag = true;
			}
		}
		if (flag)
		{
			OnWeatherEffectsApplied(stats);
		}
	}

	private void RemoveWeatherEffectsFromNPC(CharacterStats stats, StatusEffectParams[] effects)
	{
		for (int i = 0; i < effects.Length; i++)
		{
			stats.ClearStatusEffects(effects[i].Tag);
		}
		OnWeatherEffectsRemoved(stats);
	}

	public void RemoveWeatherEffectsFromNPC(GameObject gameObject)
	{
		if ((bool)gameObject)
		{
			CharacterStats component = gameObject.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				RemoveWeatherEffectsFromNPC(component);
			}
		}
	}

	public void RemoveWeatherEffectsFromNPC(CharacterStats stats)
	{
		if (!(stats == null))
		{
			StatusEffectParams[] statusEffectsForTemperature = GetStatusEffectsForTemperature(m_CurrentTemperature);
			if (statusEffectsForTemperature != null)
			{
				RemoveWeatherEffectsFromNPC(stats, statusEffectsForTemperature);
			}
		}
	}

	public void RemoveWeatherEffectsFromNPCs(WeatherTemperature Temperature)
	{
		StatusEffectParams[] statusEffectsForTemperature = GetStatusEffectsForTemperature(Temperature);
		if (statusEffectsForTemperature == null)
		{
			return;
		}
		CharacterStats[] array = UnityEngine.Object.FindObjectsOfType<CharacterStats>();
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				RemoveWeatherEffectsFromNPC(array[i], statusEffectsForTemperature);
			}
		}
	}

	public void RemoveAllWeatherEffectsFromNPCs()
	{
		RemoveWeatherEffectsFromNPCs(m_CurrentTemperature);
	}

	public AttackBase.EffectAttachType GetFootstepEffectAttachmentType(GameObject obj)
	{
		if (!obj)
		{
			return AttackBase.EffectAttachType.LeftFoot;
		}
		AnimationBoneMapper component = obj.GetComponent<AnimationBoneMapper>();
		if (!component)
		{
			return AttackBase.EffectAttachType.LeftFoot;
		}
		Transform transform = component[obj, AttackBase.EffectAttachType.LeftFoot];
		Transform transform2 = component[obj, AttackBase.EffectAttachType.RightFoot];
		if ((bool)transform && (bool)transform2)
		{
			if (transform.position.y < transform2.position.y)
			{
				return AttackBase.EffectAttachType.LeftFoot;
			}
			return AttackBase.EffectAttachType.RightFoot;
		}
		return AttackBase.EffectAttachType.LeftFoot;
	}

	private void HandleCharacterFootstep(object sender, EventArgs e)
	{
		if (GameState.Option.Quality == 0)
		{
			return;
		}
		GameObject gameObject = sender as GameObject;
		if (!gameObject || !FogOfWar.PointVisibleInFog(gameObject.transform.position))
		{
			return;
		}
		GroundMaterial materialAtPoint = FootstepMapLoader.Instance.GetMaterialAtPoint(gameObject.transform.position);
		GameObject[] effectsForGroundMaterialFootstep = GetEffectsForGroundMaterialFootstep(materialAtPoint);
		if (effectsForGroundMaterialFootstep == null || effectsForGroundMaterialFootstep.Length == 0)
		{
			return;
		}
		for (int i = 0; i < effectsForGroundMaterialFootstep.Length; i++)
		{
			if (effectsForGroundMaterialFootstep != null)
			{
				AttackBase.EffectAttachType footstepEffectAttachmentType = GetFootstepEffectAttachmentType(gameObject);
				Transform transform = AttackBase.GetTransform(gameObject, footstepEffectAttachmentType);
				transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(gameObject.transform.forward).eulerAngles.y, 0f);
				GameUtilities.LaunchEffect(effectsForGroundMaterialFootstep[i], 1f, transform.position, transform.rotation, null);
			}
		}
	}

	private void OnCharacterStatsStart(object statsObject, EventArgs args)
	{
		if (statsObject == null)
		{
			return;
		}
		CharacterStats characterStats = statsObject as CharacterStats;
		if ((bool)characterStats)
		{
			AnimationController component = characterStats.GetComponent<AnimationController>();
			NPCAppearance component2 = characterStats.GetComponent<NPCAppearance>();
			if ((bool)component && (bool)component2 && component2.enabled)
			{
				component.OnEventFootstep -= HandleCharacterFootstep;
				component.OnEventFootstep += HandleCharacterFootstep;
			}
			if (!GameState.IsLoading)
			{
				ApplyWeatherEffectsToNPC(characterStats);
			}
		}
	}

	private void OnNPCDeath(GameObject myObject, GameEventArgs args)
	{
		if ((bool)myObject)
		{
			Health component = myObject.GetComponent<Health>();
			if ((bool)component)
			{
				component.OnDeath -= OnNPCDeath;
				component.OnRevived += OnNPCRevived;
			}
			RemoveWeatherEffectsFromNPC(myObject);
		}
	}

	private void OnNPCRevived(GameObject myObject, GameEventArgs args)
	{
		if ((bool)myObject)
		{
			Health component = myObject.GetComponent<Health>();
			if ((bool)component)
			{
				component.OnRevived -= OnNPCRevived;
			}
			ApplyWeatherEffectsToNPC(myObject);
		}
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		RemoveAllWeatherEffectsFromNPCs();
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		m_CurrentTemperature = GetCurrentMapTemperature();
		ApplyWeatherEffectsToNPCs(m_CurrentTemperature);
	}
}
