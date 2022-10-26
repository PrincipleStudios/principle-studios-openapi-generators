using System.ComponentModel;

#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class IsExternalInit { }
#endif
