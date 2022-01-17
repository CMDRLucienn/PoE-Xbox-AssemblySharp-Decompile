using System.Collections.Generic;
using UnityEngine;

public static class Clustering
{
	private class Cluster
	{
		public List<IClusterable> dataPoints = new List<IClusterable>();

		public Vector2 mean = Vector2.zero;

		public bool Empty => dataPoints.Count == 0;

		public void Add(IClusterable point)
		{
			mean = (point.GetPosition() + mean * dataPoints.Count) / (dataPoints.Count + 1);
			dataPoints.Add(point);
		}

		public void Remove(IClusterable point)
		{
			if (dataPoints.Contains(point))
			{
				mean = (mean * dataPoints.Count - point.GetPosition()) / (dataPoints.Count - 1);
				dataPoints.Remove(point);
			}
		}
	}

	public static T[][] DoCluster<T>(IList<T> dataPoints, float splitThreshold) where T : IClusterable
	{
		float num = splitThreshold * splitThreshold;
		if (dataPoints.Count == 0)
		{
			return new T[0][];
		}
		if (dataPoints.Count == 1)
		{
			T[][] obj = new T[1][] { new T[1] };
			obj[0][0] = dataPoints[0];
			return obj;
		}
		List<Cluster> list = new List<Cluster>();
		Cluster[] array = new Cluster[dataPoints.Count];
		Cluster cluster = new Cluster();
		cluster.Add(dataPoints[0]);
		list.Add(cluster);
		array[0] = cluster;
		bool flag = false;
		while (!flag)
		{
			flag = true;
			for (int i = 0; i < dataPoints.Count; i++)
			{
				float num2 = float.MaxValue;
				Cluster cluster2 = null;
				foreach (Cluster item in list)
				{
					float sqrMagnitude = (item.mean - dataPoints[i].GetPosition()).sqrMagnitude;
					if (sqrMagnitude < num2 && sqrMagnitude < num)
					{
						num2 = sqrMagnitude;
						cluster2 = item;
					}
				}
				if (cluster2 != null)
				{
					if (cluster2 == array[i])
					{
						continue;
					}
					flag = false;
					cluster2.Add(dataPoints[i]);
					if (array[i] != null)
					{
						array[i].Remove(dataPoints[i]);
						if (array[i].Empty)
						{
							list.Remove(array[i]);
						}
					}
					array[i] = cluster2;
					continue;
				}
				flag = false;
				Cluster cluster3 = new Cluster();
				cluster3.Add(dataPoints[i]);
				list.Add(cluster3);
				if (array[i] != null)
				{
					array[i].Remove(dataPoints[i]);
					if (array[i].Empty)
					{
						list.Remove(array[i]);
					}
				}
				array[i] = cluster3;
			}
		}
		T[][] array2 = new T[list.Count][];
		for (int j = 0; j < list.Count; j++)
		{
			T[] array3 = (array2[j] = new T[list[j].dataPoints.Count]);
			for (int k = 0; k < array3.Length; k++)
			{
				array2[j][k] = (T)list[j].dataPoints[k];
			}
		}
		return array2;
	}
}
