using UnityEngine;

public class BackerContent : MonoBehaviour
{
	public string BackerName;

	public DatabaseString BackerDescription = new DatabaseString(DatabaseString.StringTableType.BackerContent);

	private void Start()
	{
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}
