using Wiped.Shared.IoC;

namespace Wiped.Tools;

internal interface IProgramManager : IManager
{
	int Load<T>(string[] args) where T : BaseTool;
	int Load(string name, string[] args);
	IEnumerable<BaseTool> GetAll();
}
