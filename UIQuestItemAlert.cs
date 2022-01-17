using UnityEngine;

public class UIQuestItemAlert : MonoBehaviour
{
	private TweenColor m_Alerter;

	private void OnEnable()
	{
		if (!m_Alerter)
		{
			m_Alerter = base.gameObject.AddComponent<TweenColor>();
			m_Alerter.from = Color.white;
			m_Alerter.to = Color.black;
			m_Alerter.style = UITweener.Style.PingPong;
			m_Alerter.duration = 0.4f;
			m_Alerter.Reset();
			m_Alerter.enabled = false;
		}
		if ((bool)GameState.s_playerCharacter)
		{
			QuestInventory component = GameState.s_playerCharacter.GetComponent<QuestInventory>();
			if ((bool)component && component.HasNew)
			{
				m_Alerter.Play(forward: true);
				return;
			}
			m_Alerter.Reset();
			m_Alerter.enabled = false;
		}
	}

	private void OnClick()
	{
		m_Alerter.Reset();
		m_Alerter.enabled = false;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}
