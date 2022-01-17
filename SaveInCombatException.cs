using System;

[Serializable]
public class SaveInCombatException : Exception
{
	public SaveInCombatException()
		: base("Tried to save the game while in combat.")
	{
	}
}
