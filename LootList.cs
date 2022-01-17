using System.Collections;
using UnityEngine;

public class LootList : ScriptableObject
{
	public LootItem[] items;

	public int TotalWeight
	{
		get
		{
			int num = 0;
			for (int i = 0; i < items.Length; i++)
			{
				if (!items[i].Always)
				{
					num += items[i].Weight;
				}
			}
			return num;
		}
	}

	public object[] Evaluate()
	{
		ArrayList arrayList = new ArrayList();
		int totalWeight = TotalWeight;
		int num = Random.Range(0, totalWeight);
		totalWeight = 0;
		bool flag = false;
		for (int i = 0; i < items.Length; i++)
		{
			bool flag2 = false;
			if (!items[i].Always)
			{
				totalWeight += items[i].Weight;
				if (num < totalWeight && !flag)
				{
					flag2 = true;
					flag = true;
				}
			}
			else
			{
				flag2 = true;
			}
			if (items[i].Item == null || !flag2)
			{
				continue;
			}
			for (int j = 0; j < items[i].Count; j++)
			{
				if (items[i].Item is LootList)
				{
					arrayList.AddRange((items[i].Item as LootList).Evaluate());
				}
				else
				{
					arrayList.Add(items[i].Item);
				}
			}
		}
		return arrayList.ToArray();
	}
}
