using Wiped.Server.IoC;
using Wiped.Shared;

namespace Wiped.Server;

internal class Program
{
	static void Main(string[] args)
	{
		ServerEngineIoC.Register();
		EntryPoint.Start();
	}
}
