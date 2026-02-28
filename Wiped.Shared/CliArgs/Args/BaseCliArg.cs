using Wiped.Shared.Reflection;

namespace Wiped.Shared.CliArgs.Args;

[Reflectable]
internal abstract class BaseCliArg
{
	public abstract string Name();
	public abstract string Desc();

	public virtual int MinArgs() => 0;
	public virtual int MaxArgs() => 0;

	public abstract int Run(string[] args);

	public string Usage() => $"{Name()} - {Desc()}";
}
