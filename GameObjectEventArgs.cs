using System;
using UnityEngine;

public class GameObjectEventArgs : EventArgs
{
	public GameObject Object;

	public GameObjectEventArgs(GameObject obj)
	{
		Object = obj;
	}
}
