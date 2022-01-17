using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ArmorDtRangeAttribute : PropertyAttribute
{
	public DamagePacket.DamageType DamageType { get; set; }

	public ArmorDtRangeAttribute(DamagePacket.DamageType damage)
	{
		DamageType = damage;
	}
}
