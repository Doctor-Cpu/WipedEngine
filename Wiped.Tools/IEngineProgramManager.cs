using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Tools;

internal interface IEngineProgramManager : IManager
{
	void SetProgram(string program);

	void Load<T>(string[] args) where T : BaseTool;

	void Load(string name, string[] args);

	bool TryGet<T>([NotNullWhen(true)] out T? tool) where T : BaseTool;

	bool TryGet(Type type, [NotNullWhen(true)] out BaseTool? tool);

	bool TryGet(string name, [NotNullWhen(true)] out BaseTool? tool);

	IEnumerable<BaseTool> GetAll();
}
