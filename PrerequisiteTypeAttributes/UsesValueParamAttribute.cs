using System;

namespace PrerequisiteTypeAttributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class UsesValueParamAttribute : Attribute
{
}
