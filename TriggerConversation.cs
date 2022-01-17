using UnityEngine;

public class TriggerConversation : TriggerLink
{
	public string Conversation;

	public int Node;

	public GameObject Owner;

	public override void OnTriggerEnter(Collider dude)
	{
		if (CanTrigger(dude.gameObject))
		{
			ConversationManager.Instance.StartConversation(Conversation, 0, Owner, FlowChartPlayer.DisplayMode.Standard);
			m_triggeredCount++;
		}
	}
}
