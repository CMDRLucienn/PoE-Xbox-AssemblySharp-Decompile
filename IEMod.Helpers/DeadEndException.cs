/// <summary>
/// Indicates that the code should be unreachable.
/// </summary>
public class DeadEndException : IEModException
{
	public DeadEndException(string location)
	: base(string.Format("Code should be unreachable. Location: {0}", location))
	{
	}
}
