using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class AssetByNameAttribute : PropertyAttribute
{
	public readonly Type AllowedType;

	public AssetByNameAttribute(Type type)
	{
		AllowedType = type;
	}
}
