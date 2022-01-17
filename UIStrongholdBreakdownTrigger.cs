using UnityEngine;

public class UIStrongholdBreakdownTrigger : MonoBehaviour
{
	public Stronghold.StatType Stat;

	private UIWidget m_Widget;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void OnTooltip(bool over)
	{
		if (over)
		{
			UIStrongholdManager.Instance.Breakdown.Show(Stat, m_Widget);
		}
		else
		{
			UIStrongholdManager.Instance.Breakdown.Hide();
		}
	}
}
