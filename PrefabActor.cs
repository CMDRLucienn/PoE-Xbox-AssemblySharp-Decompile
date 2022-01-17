using System;
using UnityEngine;

[Serializable]
public class PrefabActor
{
	public GameObject Prefab;

	public CutsceneWaypoint SpawnLocation;

	public CutsceneWaypoint MoveLocation;

	public bool UseMoveLocation;

	public bool DeactivateAtStart;

	public bool DeleteAtEnd = true;

	[HideInInspector]
	public GameObject SpawnedObject;
}
