public class ConsoleUser
{
	public readonly string UID;

	public readonly int index;

	public readonly bool isPrimary;

	public readonly string onlineID;

	public readonly string hashedUID;

	public ConsoleUser(string UID, int index, string onlineID, bool isPrimary, string hashedUID)
	{
		this.hashedUID = hashedUID;
		this.UID = UID;
		this.index = index;
		this.onlineID = onlineID;
		this.isPrimary = isPrimary;
	}
}
