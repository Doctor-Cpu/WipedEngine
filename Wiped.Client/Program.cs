using Wiped.Client.IoC;
using Wiped.Shared;

namespace Wiped.Client;

internal class Program
{
	static void Main(string[] args)
	{
		ClientEngineIoC.Register();
		SharedEntryPoint.Start();
		EntryPoint.Start();
		SharedEntryPoint.Initialize();
	}
}
