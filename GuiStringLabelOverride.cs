using UnityEngine;

[RequireComponent(typeof(GUIStringLabel))]
public class GuiStringLabelOverride : MonoBehaviour
{
	public int OverrideStringID;

	private int m_originalStringID;

	public bool StartOverriden;

	private bool m_overrideSet;

	private void Start()
	{
		GUIStringLabel component = GetComponent<GUIStringLabel>();
		if ((bool)component)
		{
			m_originalStringID = component.DatabaseString.StringID;
		}
		if (StartOverriden)
		{
			BeginOverride();
		}
	}

	public void BeginOverride()
	{
		if (!m_overrideSet)
		{
			GUIStringLabel component = GetComponent<GUIStringLabel>();
			if ((bool)component)
			{
				component.SetString(OverrideStringID);
				m_overrideSet = true;
			}
		}
	}

	public void EndOverride()
	{
		if (m_overrideSet)
		{
			GUIStringLabel component = GetComponent<GUIStringLabel>();
			if ((bool)component)
			{
				component.SetString(m_originalStringID);
				m_overrideSet = false;
			}
		}
	}
}
