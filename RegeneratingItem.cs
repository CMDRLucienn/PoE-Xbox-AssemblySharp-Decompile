using System;
using Polenter.Serialization;

[Serializable]
public class RegeneratingItem
{
	[ExcludeFromSerialization]
	public Item baseItem;

	public int stackMin = 1;

	public int stackMax = 1;

	public float Chance = 0.5f;
}
