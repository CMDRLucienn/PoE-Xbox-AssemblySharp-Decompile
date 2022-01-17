using UnityEngine;

public class UiOdNuaExternalIcon : MonoBehaviour
{
	public UILabel Label;

	public UIWidget Icon;

	public UIWorldMapIcon Link;

	public int Level = 1;

	public MapsDatabaseString OverrideName;

	[GlobalVariableString]
	public string GlobalVariable = "";

	private bool m_GlobalValid = true;

	private void OnEnable()
	{
		m_GlobalValid = string.IsNullOrEmpty(GlobalVariable) || GlobalVariables.Instance.GetVariable(GlobalVariable) != 0;
	}

	private void Start()
	{
		if (!Label)
		{
			Label = GetComponentInChildren<UILabel>();
		}
		if (!Icon)
		{
			Icon = GetComponentInChildren<UISprite>();
		}
		if (OverrideName.IsValidString)
		{
			Label.text = OverrideName.GetText();
			return;
		}
		Label.text = GUIUtils.Format(1763, Level);
	}

	private void Update()
	{
		bool flag = Link.gameObject.activeInHierarchy && m_GlobalValid;
		if ((bool)Label)
		{
			Label.enabled = flag;
		}
		if ((bool)Icon)
		{
			Icon.enabled = flag;
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}
