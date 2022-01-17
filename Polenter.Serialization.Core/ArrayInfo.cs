using System.Collections.Generic;

namespace Polenter.Serialization.Core;

public sealed class ArrayInfo
{
	private IList<DimensionInfo> _dimensionInfos;

	public IList<DimensionInfo> DimensionInfos
	{
		get
		{
			if (_dimensionInfos == null)
			{
				_dimensionInfos = new List<DimensionInfo>();
			}
			return _dimensionInfos;
		}
		set
		{
			_dimensionInfos = value;
		}
	}
}
