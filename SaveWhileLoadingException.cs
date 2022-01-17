using System;

[Serializable]
public class SaveWhileLoadingException : Exception
{
	public SaveWhileLoadingException()
		: base("Tried to save the game while loading.")
	{
	}
}
