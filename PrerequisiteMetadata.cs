using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrerequisiteTypeAttributes;

public static class PrerequisiteMetadata
{
	private struct Metadata
	{
		public bool UsesValue;

		public bool UsesTag;

		public bool UsesClassValue;

		public bool UsesRaceValue;

		public bool UsesSkillValue;

		public bool UsesAfflictionPrefab;
	}

	private static Dictionary<PrerequisiteType, Metadata> m_Metadata;

	static PrerequisiteMetadata()
	{
		m_Metadata = new Dictionary<PrerequisiteType, Metadata>();
		Type typeFromHandle = typeof(PrerequisiteType);
		Array values = Enum.GetValues(typeFromHandle);
		for (int i = 0; i < values.Length; i++)
		{
			MemberInfo[] member = typeFromHandle.GetMember(values.GetValue(i).ToString());
			Metadata value = new Metadata
			{
				UsesValue = member.First().GetCustomAttributes(typeof(UsesValueParamAttribute), inherit: false).Any(),
				UsesTag = member.First().GetCustomAttributes(typeof(UsesTagParamAttribute), inherit: false).Any(),
				UsesRaceValue = member.First().GetCustomAttributes(typeof(UsesRaceValueParamAttribute), inherit: false).Any(),
				UsesClassValue = member.First().GetCustomAttributes(typeof(UsesClassValueParamAttribute), inherit: false).Any(),
				UsesSkillValue = member.First().GetCustomAttributes(typeof(UsesSkillValueParamAttribute), inherit: false).Any(),
				UsesAfflictionPrefab = member.First().GetCustomAttributes(typeof(UsesAfflictionParamAttribute), inherit: false).Any()
			};
			m_Metadata[(PrerequisiteType)i] = value;
		}
	}

	public static bool UsesValueParam(PrerequisiteType type)
	{
		return m_Metadata[type].UsesValue;
	}

	public static bool UsesTagParam(PrerequisiteType type)
	{
		return m_Metadata[type].UsesTag;
	}

	public static bool UsesRaceValueParam(PrerequisiteType type)
	{
		return m_Metadata[type].UsesRaceValue;
	}

	public static bool UsesClassValueParam(PrerequisiteType type)
	{
		return m_Metadata[type].UsesClassValue;
	}

	public static bool UsesSkillValueParam(PrerequisiteType type)
	{
		return m_Metadata[type].UsesSkillValue;
	}

	public static bool UsesAfflictionParam(PrerequisiteType type)
	{
		return m_Metadata[type].UsesAfflictionPrefab;
	}
}
