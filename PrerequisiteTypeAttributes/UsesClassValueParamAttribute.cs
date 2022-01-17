using System;

namespace PrerequisiteTypeAttributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class UsesClassValueParamAttribute : Attribute
{
}
