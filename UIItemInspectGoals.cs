using UnityEngine;

public class UIItemInspectGoals : MonoBehaviour
{
	public UILabel GoalLabel;

	public GameObject LastGoalParent;

	public UILabel LastGoalLabel;

	public GameObject DegenerateGoalParent;

	public UILabel DegenerateGoalLabel;

	public GameObject[] Children;

	private UITable m_Table;

	private void Awake()
	{
		m_Table = GetComponent<UITable>();
	}

	public void Set(EquipmentSoulbind soulbind, bool isLevelUpNotification)
	{
		for (int i = 0; i < Children.Length; i++)
		{
			Children[i].SetActive(soulbind);
		}
		if ((bool)soulbind)
		{
			GoalLabel.text = soulbind.GetNextRequirementString(degenerate: false) + " " + soulbind.GetProgressString(degenerate: false);
			DegenerateGoalLabel.text = soulbind.GetNextRequirementString(degenerate: true) + " " + soulbind.GetProgressString(degenerate: true);
			DegenerateGoalParent.SetActive(soulbind.CurrentLevelCanBeUnlockedDegenerately);
			if (isLevelUpNotification)
			{
				LastGoalLabel.text = soulbind.GetLastRequirementString();
				LastGoalParent.SetActive(!string.IsNullOrEmpty(LastGoalLabel.text));
			}
			else
			{
				LastGoalParent.SetActive(value: false);
			}
		}
		if ((bool)m_Table)
		{
			m_Table.Reposition();
		}
	}
}
