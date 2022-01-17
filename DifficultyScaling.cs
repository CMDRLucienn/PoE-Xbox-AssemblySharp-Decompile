using System;
using UnityEngine;

public class DifficultyScaling : MonoBehaviour
{
	[Flags]
	public enum Scaler
	{
		PX1_HIGH_LEVEL = 1,
		ACT4_HIGH_LEVEL = 2,
		ELMSHORE_HIGH_LEVEL = 4,
		PX2_HIGH_LEVEL = 8
	}

	[Serializable]
	public class ScaleData
	{
		public string DesignNote;

		public Scaler Type;

		[Tooltip("A bonus to each attribute of any creature marked to scale.")]
		public int CreatureAttributeBonus;

		[Tooltip("On marked creatures, level is multiplier by this and rounded down.")]
		public float CreatureLevelMult = 1f;

		[Tooltip("All designer trap and object detect difficulties are multiplied by this and rounded up.")]
		public float DetectableDifficultyMult = 1f;

		[Tooltip("All designer trap disarm difficulties are multiplied by this and rounded up.")]
		public float DisarmDifficultyMult = 1f;

		[Tooltip("All designer traps gain this much bonus accuracy.")]
		public int TrapAccuracyBonus;

		[Tooltip("All designer traps' effect durations are multiplied by this.")]
		public float TrapEffectMult = 1f;

		[Tooltip("All designer traps' damage is multiplied by this.")]
		public float TrapDamageMult = 1f;

		[Tooltip("Skill check DCs are multiplied by this.")]
		public float SkillCheckMult = 1f;
	}

	[Tooltip("List of data for possible scalers.")]
	public ScaleData[] Data;

	public static DifficultyScaling Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'DifficultyScaling' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
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

	public float GetScaleMultiplicative(Scaler types, Func<ScaleData, float> attributeMap)
	{
		float num = 1f;
		for (int i = 0; i < Data.Length; i++)
		{
			Scaler scaler = Data[i].Type & types;
			if (scaler != 0 && Instance.IsAnyScalerActive(scaler))
			{
				num *= attributeMap(Data[i]);
			}
		}
		return num;
	}

	public int GetScaleAdditive(Scaler types, Func<ScaleData, int> attributeMap)
	{
		int num = 0;
		for (int i = 0; i < Data.Length; i++)
		{
			Scaler scaler = Data[i].Type & types;
			if (scaler != 0 && Instance.IsAnyScalerActive(scaler))
			{
				num += attributeMap(Data[i]);
			}
		}
		return num;
	}

	public bool IsAnyScalerActive(Scaler scaler)
	{
		if ((bool)GameState.Instance)
		{
			return (GameState.Instance.ActiveScalers & scaler) != 0;
		}
		return false;
	}

	public void ToggleScaler(Scaler scaler)
	{
		SetScalerActive(scaler, !IsAnyScalerActive(scaler));
	}

	public void SetScalerActive(Scaler scaler, bool active)
	{
		if (active)
		{
			GameState.Instance.ActiveScalers |= scaler;
		}
		else
		{
			GameState.Instance.ActiveScalers &= ~scaler;
		}
	}
}
