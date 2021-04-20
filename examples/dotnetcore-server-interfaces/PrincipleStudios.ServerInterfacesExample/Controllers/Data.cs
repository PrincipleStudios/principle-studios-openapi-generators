namespace PrincipleStudios.ServerInterfacesExample.Controllers
{
    internal static class Data
    {
        // Obviously, this is a demo. You should use a different class for storage, and only use your controller for mapping.
        internal readonly static System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string? tag)> pets = new System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string? tag)>();
        internal static long lastId = 0;
    }
}
