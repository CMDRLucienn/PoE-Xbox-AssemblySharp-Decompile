using UnityEngine;

public class TriggerQuestObjective : TriggerLink
{
	public string QuestName;

	public int QuestStageCheck;

	public override void OnTriggerEnter(Collider dude)
	{
		if (CanTrigger(dude.gameObject) && !(QuestManager.Instance == null) && QuestManager.Instance.IsQuestStateActive(QuestName, QuestStageCheck))
		{
			if (dude.gameObject.GetComponent<Player>() != null)
			{
				QuestManager.Instance.AdvanceQuest(QuestName);
			}
			m_triggeredCount++;
		}
	}
}
