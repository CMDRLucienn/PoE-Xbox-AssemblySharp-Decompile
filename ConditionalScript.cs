using System;
using System.Collections.Generic;
using OEIFormats.FlowCharts;

[Serializable]
public class ConditionalScript
{
	public string Function;

	public List<string> Parameters = new List<string>();

	public LogicalOperator Op = LogicalOperator.Or;

	public bool Not;
}
