using System;
using UnityEngine;

[Serializable]
public class SceneActor
{
	public GameObject Actor;

	public CutsceneWaypoint SpawnLocation;

	public CutsceneWaypoint MoveLocation;

	public bool UseSpawnLocation;

	public bool UseMoveLocation;

	public bool ActivateAtStart = true;

	public bool DeactivateAtEnd;

	[Tooltip("If true, do not include this character in the cutscene if he is dead.")]
	public bool IgnoreIfDead = true;
}
