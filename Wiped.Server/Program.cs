using Wiped.Server.IoC;
using Wiped.Shared;

namespace Wiped.Server;

internal class Program
{
	static void Main(string[] args)
	{
		ServerEngineIoC.Register();
		SharedEntryPoint.Start();
		EntryPoint.Start();
		SharedEntryPoint.Initialize();
	}
}
