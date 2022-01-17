using System;

namespace Polenter.Serialization;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public sealed class ExcludeFromSerializationAttribute : Attribute
{
}
