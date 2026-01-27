namespace Wiped.Shared.IoC;

internal enum IoCLifecycle : byte
{
    Constructing,   // bindings allowed, no resolution
    Resolving,      // resolution + injection allowed
    Frozen          // fully locked, runtime-only
}
