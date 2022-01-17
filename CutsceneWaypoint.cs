using System;
using UnityEngine;

[Serializable]
public class CutsceneWaypoint
{
	public enum CutsceneMoveType
	{
		None,
		Walk,
		Run,
		Teleport
	}

	public Transform Location;

	public CutsceneMoveType MoveType;

	public GameObject TeleportVFX;

	[HideInInspector]
	public GameObject owner;
}
