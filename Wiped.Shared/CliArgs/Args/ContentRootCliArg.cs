using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.CliArgs.Args;

internal sealed class ContentRootCliArg : BaseCliArg
{
	[Dependency] private readonly IoCDynamic<IEngineContentVFSManager> _vfs = default!;

	public override string Name() => "content-root";
    public override string Desc() => "Specify the root folder for content.";

	public override int MinArgs() => 1;
	public override int? MaxArgs() => null;

	public override int Run(string[] args)
	{
		_vfs.Value.Bootstrap(args);
		return args.Length;
	}
}
