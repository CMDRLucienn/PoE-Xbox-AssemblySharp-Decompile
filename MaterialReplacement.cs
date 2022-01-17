using System;
using UnityEngine;

[Serializable]
public class MaterialReplacement
{
	public Material Material;

	public LayerMask Layer;

	public bool ReplaceColor;

	public bool ReplaceNormal;

	public bool ReplaceEmissive;

	public bool ReplaceTint;

	public bool Empty => Material == null;

	public static bool IsNullOrEmpty(MaterialReplacement replacement)
	{
		return replacement?.Empty ?? true;
	}

	public void Replace(GameObject obj)
	{
		MaterialCache.Get(obj).Replace(this);
	}

	public void Restore(GameObject obj)
	{
		MaterialCache materialCache = MaterialCache.Get(obj);
		if ((bool)materialCache)
		{
			materialCache.Restore(this);
			MaterialCache.Clear(obj);
		}
	}
}
