using UnityEngine;

public class UIGrimoireLevelButtons : MonoBehaviour
{
	public delegate void OnLevelChanged(int level);

	public UILabel LevelLabel;

	public UIGrid Grid;

	public UIGrimoireLevelButton RootButton;

	private int m_CurrentLevel = 1;

	public OnLevelChanged LevelChanged;

	private UIGrimoireLevelButton[] m_Buttons;

	public int CurrentLevel => m_CurrentLevel;

	private void Start()
	{
		ChangeLevel(1);
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Init()
	{
		if (m_Buttons == null)
		{
			m_Buttons = new UIGrimoireLevelButton[10];
			m_Buttons[0] = RootButton;
			m_Buttons[0].gameObject.name = "Level01";
			m_Buttons[0].Level = 1;
			for (int i = 1; i < 10; i++)
			{
				m_Buttons[i] = NGUITools.AddChild(RootButton.transform.parent.gameObject, RootButton.gameObject).GetComponent<UIGrimoireLevelButton>();
				m_Buttons[i].gameObject.name = "Level" + (i + 1).ToString("00");
				m_Buttons[i].Level = i + 1;
			}
			Grid.Reposition();
		}
	}

	public void ChangeLevel(int level)
	{
		Init();
		if (m_CurrentLevel > 0)
		{
			m_Buttons[m_CurrentLevel - 1].ForceHighlight(setting: false);
		}
		m_CurrentLevel = level;
		m_Buttons[m_CurrentLevel - 1].ForceHighlight(setting: true);
		if (LevelChanged != null)
		{
			LevelChanged(level);
		}
		LevelLabel.text = GUIUtils.Format(410, Ordinal.Get(level));
	}

	public void IncLevel()
	{
		int num = m_CurrentLevel + 1;
		if (num > m_Buttons.Length)
		{
			num = 1;
		}
		ChangeLevel(num);
	}

	public void DecLevel()
	{
		int num = m_CurrentLevel - 1;
		if (num < 1)
		{
			num = m_Buttons.Length;
		}
		ChangeLevel(num);
	}
}
