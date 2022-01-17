using System;
using UnityEngine;

[Serializable]
public class LootItem
{
	public int Weight = 1;

	public int Count = 1;

	public bool Always;

	public UnityEngine.Object Item;
}
