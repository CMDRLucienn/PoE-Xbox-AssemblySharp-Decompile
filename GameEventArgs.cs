using System;
using UnityEngine;

public class GameEventArgs : EventArgs
{
	public GameEventType Type;

	public int[] IntData;

	public float[] FloatData;

	public GameObject[] GameObjectData;

	public object[] GenericData;

	public Vector3[] VectorData;
}
