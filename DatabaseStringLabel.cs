using UnityEngine;

[RequireComponent(typeof(UILabel))]
[ExecuteInEditMode]
public class DatabaseStringLabel : MonoBehaviour
{
	public DatabaseString DatabaseString = new DatabaseString(DatabaseString.StringTableType.Gui);

	public DatabaseString.StringTableType StringTable = DatabaseString.StringTableType.Gui;

	public bool AllCaps;

	public string FormatString;

	public static DatabaseStringLabel Get(MonoBehaviour mb)
	{
		return Get(mb.gameObject);
	}

	public static DatabaseStringLabel Get(GameObject go)
	{
		DatabaseStringLabel databaseStringLabel = go.GetComponent<DatabaseStringLabel>();
		if (!databaseStringLabel)
		{
			databaseStringLabel = go.AddComponent<DatabaseStringLabel>();
		}
		return databaseStringLabel;
	}

	private void Start()
	{
		RefreshText();
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLanguageChanged(Language lang)
	{
		RefreshText();
	}

	public void SetString(DatabaseString.StringTableType table, int stringId)
	{
		DatabaseString.StringID = stringId;
		DatabaseString.StringTable = table;
		RefreshText();
	}

	public void SetString(DatabaseString str)
	{
		if (str != null)
		{
			DatabaseString.StringID = str.StringID;
			DatabaseString.StringTable = str.StringTable;
			RefreshText();
		}
	}

	public void RefreshText()
	{
		UILabel component = GetComponent<UILabel>();
		if (component != null)
		{
			if (DatabaseString == null)
			{
				component.text = "";
			}
			else if (AllCaps)
			{
				component.text = DatabaseString.GetText().ToUpper();
			}
			else
			{
				component.text = DatabaseString.GetText();
			}
			if (!string.IsNullOrEmpty(FormatString))
			{
				component.text = StringUtility.Format(FormatString, component.text);
			}
		}
	}
}
