using UnityEngine;

public class StringList : ScriptableObject
{
	public string[] Strings;

	public string this[int index]
	{
		get
		{
			return Strings[index];
		}
		set
		{
			Strings[index] = value;
		}
	}

	public StringList(int length)
	{
		Strings = new string[length];
	}
}
