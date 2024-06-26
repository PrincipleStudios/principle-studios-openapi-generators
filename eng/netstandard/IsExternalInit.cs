using System.ComponentModel;

#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices;

#pragma warning disable CA1812 // Internal class never instantiated
[EditorBrowsable(EditorBrowsableState.Never)]
internal class IsExternalInit { }
#pragma warning restore CA1812 // Internal class never instantiated
#endif
