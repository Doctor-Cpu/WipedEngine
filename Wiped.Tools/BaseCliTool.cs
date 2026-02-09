namespace Wiped.Tools;

internal abstract class BaseCliTool : BaseTool
{
	private readonly TextWriter _output;

	public virtual int ArgMin => int.MinValue;
	public virtual int ArgMax => int.MaxValue;

	public override int Start(string[] args)
	{
		if (args.Length > ArgMax)
			return ToolExitCodes.WrongArgCount;

		if (args.Length < ArgMin)
			return ToolExitCodes.WrongArgCount;

		return Run(args);
	}

	protected abstract int Run(string[] args);

	protected void PrintText(string text)
	{
		_output.WriteLine(text);
	}

	internal BaseCliTool(TextWriter? output = null)
	{
		_output = output ?? Console.Out;
	}
}
