using OEIFormats.FlowCharts.Quests;
using UnityEngine;

public class UIQuestNotificationObjective : MonoBehaviour
{
	public UIWidget Check;

	public UIWidget CheckBack;

	public UILabel TextLabel;

	public float Height => TextLabel.relativeSize.y * TextLabel.transform.localScale.y;

	public void Set(Quest quest, ObjectiveNode node)
	{
		Check.alpha = (quest.IsQuestStateEnded(node.NodeID) ? 1 : 0);
		CheckBack.alpha = 1f;
		TextLabel.text = quest.GetObjectiveTitle(node);
	}

	public void Set(Quest quest, string text)
	{
		Check.alpha = 0f;
		CheckBack.alpha = 0f;
		TextLabel.text = text;
	}
}
