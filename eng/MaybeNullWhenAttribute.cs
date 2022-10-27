using System.ComponentModel;

#if !NET6_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class MaybeNullWhenAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified return value condition.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter may be null.
    /// </param>
    public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }
}
#endif
