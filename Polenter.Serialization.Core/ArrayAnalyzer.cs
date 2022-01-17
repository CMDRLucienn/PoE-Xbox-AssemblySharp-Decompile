using System;
using System.Collections.Generic;

namespace Polenter.Serialization.Core;

public class ArrayAnalyzer
{
	private readonly object _array;

	private readonly ArrayInfo _arrayInfo;

	private IList<int[]> _indexes;

	public ArrayInfo ArrayInfo => _arrayInfo;

	public ArrayAnalyzer(object array)
	{
		_array = array;
		Type type = array.GetType();
		_arrayInfo = getArrayInfo(type);
	}

	private int getRank(Type arrayType)
	{
		return arrayType.GetArrayRank();
	}

	private int getLength(int dimension, Type arrayType)
	{
		return (int)arrayType.GetMethod("GetLength").Invoke(_array, new object[1] { dimension });
	}

	private int getLowerBound(int dimension, Type arrayType)
	{
		return getBound("GetLowerBound", dimension, arrayType);
	}

	private int getBound(string methodName, int dimension, Type arrayType)
	{
		return (int)arrayType.GetMethod(methodName).Invoke(_array, new object[1] { dimension });
	}

	private ArrayInfo getArrayInfo(Type arrayType)
	{
		ArrayInfo arrayInfo = new ArrayInfo();
		for (int i = 0; i < getRank(arrayType); i++)
		{
			DimensionInfo dimensionInfo = new DimensionInfo();
			dimensionInfo.Length = getLength(i, arrayType);
			dimensionInfo.LowerBound = getLowerBound(i, arrayType);
			arrayInfo.DimensionInfos.Add(dimensionInfo);
		}
		return arrayInfo;
	}

	public IEnumerable<int[]> GetIndexes()
	{
		if (_indexes == null)
		{
			_indexes = new List<int[]>();
			ForEach(addIndexes);
		}
		foreach (int[] index in _indexes)
		{
			yield return index;
		}
	}

	public IEnumerable<object> GetValues()
	{
		foreach (int[] index in GetIndexes())
		{
			yield return ((Array)_array).GetValue(index);
		}
	}

	private void addIndexes(int[] obj)
	{
		_indexes.Add(obj);
	}

	public void ForEach(Action<int[]> action)
	{
		DimensionInfo dimensionInfo = _arrayInfo.DimensionInfos[0];
		for (int i = dimensionInfo.LowerBound; i < dimensionInfo.LowerBound + dimensionInfo.Length; i++)
		{
			List<int> list = new List<int>();
			list.Add(i);
			if (_arrayInfo.DimensionInfos.Count < 2)
			{
				action(list.ToArray());
			}
			else
			{
				forEach(_arrayInfo.DimensionInfos, 1, list, action);
			}
		}
	}

	private void forEach(IList<DimensionInfo> dimensionInfos, int dimension, IEnumerable<int> coordinates, Action<int[]> action)
	{
		DimensionInfo dimensionInfo = dimensionInfos[dimension];
		for (int i = dimensionInfo.LowerBound; i < dimensionInfo.LowerBound + dimensionInfo.Length; i++)
		{
			List<int> list = new List<int>(coordinates);
			list.Add(i);
			if (dimension == _arrayInfo.DimensionInfos.Count - 1)
			{
				action(list.ToArray());
			}
			else
			{
				forEach(_arrayInfo.DimensionInfos, dimension + 1, list, action);
			}
		}
	}
}
