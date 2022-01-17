using System;
using UnityEngine;

[Serializable]
public class EncounterData
{
	public GameObject Creature;

	public EncounterSpawnPoint SpawnPoint;

	[Tooltip("Prefab for a visual effect to play when the creature is spawned or activated.")]
	public GameObject SpawnVfx;

	[Tooltip("Prefab for a visual effect to play when the creature is deactivated.")]
	public GameObject DespawnVfx;

	public DifficultySettings AppearsInLevelOfDifficulty;

	[HideInInspector]
	public bool IsPrefab;
}
