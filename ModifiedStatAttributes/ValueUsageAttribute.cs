using System;

namespace ModifiedStatAttributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ValueUsageAttribute : Attribute
{
	public UsageType m_Usage;

	public UsageType Usage => m_Usage;

	public ValueUsageAttribute(UsageType usage)
	{
		m_Usage = usage;
	}
}
