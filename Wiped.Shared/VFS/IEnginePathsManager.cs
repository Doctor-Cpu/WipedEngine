
namespace Wiped.Shared.VFS;

internal interface IEnginePathsManager
{
    string ProcessRoot { get; }
    string EngineRoot { get; }
    string ProjectRoot { get; }
    string UserDataRoot { get; }
}
