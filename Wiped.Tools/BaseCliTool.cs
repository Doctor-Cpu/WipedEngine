namespace Wiped.Tools;

public abstract class BaseCliTool : BaseTool
{
	private readonly TextWriter _output;

	public virtual int ArgMin => int.MinValue;
	public virtual int ArgMax => int.MaxValue;

	public override void Start(string[] args)
	{
		if (args.Length > ArgMax)
			return;

		if (args.Length < ArgMin)
			return;

		Run(args);
	}

	protected abstract void Run(string[] args);

	protected void PrintText(string text)
	{
		_output.WriteLine(text);
	}

	internal BaseCliTool(TextWriter? output = null)
	{
		_output = output ?? Console.Out;
	}
}
