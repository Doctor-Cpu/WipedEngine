using Wiped.Shared;

namespace Wiped.Client;

internal class Program
{
	static void Main(string[] args)
	{
		SharedEntryPoint.Start();
		EntryPoint.Start();
		SharedEntryPoint.Initialize();
	}
}
