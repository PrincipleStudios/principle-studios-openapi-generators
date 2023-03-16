using System.ComponentModel;

#if !NET6_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[System.AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
sealed class NotNullWhenAttribute : Attribute
{
    // This is a positional argument
    public NotNullWhenAttribute(bool result)
    {
    }
}
#endif
