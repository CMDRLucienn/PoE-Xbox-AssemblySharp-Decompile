using System;
using System.Collections.Generic;

[Serializable]
public class RegenerationEntry
{
	public int GlobalVariableValue;

	public List<RegeneratingItem> RegeneratingItems = new List<RegeneratingItem>();
}
