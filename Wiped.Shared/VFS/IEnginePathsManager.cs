
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

internal interface IEnginePathsManager : IManager
{
    string ProcessRoot { get; }
    string EngineRoot { get; }
    string ProjectRoot { get; }
    string UserDataRoot { get; }
}
