using System;
using System.Collections.Generic;
using OEIFormats.FlowCharts.Quests;
using UnityEngine;

public class UIJournalQuestObjective : MonoBehaviour
{
	private class ChronologicalAddendum : IComparable<ChronologicalAddendum>
	{
		public int index;

		public Quest quest;

		public ChronologicalAddendum(int index, Quest quest)
		{
			this.index = index;
			this.quest = quest;
		}

		public int CompareTo(ChronologicalAddendum other)
		{
			EternityDateTime addendumTimestamp = QuestManager.Instance.GetAddendumTimestamp(quest, index);
			EternityDateTime addendumTimestamp2 = QuestManager.Instance.GetAddendumTimestamp(other.quest, other.index);
			if (addendumTimestamp2 == null)
			{
				return 1;
			}
			if (addendumTimestamp == null)
			{
				return -1;
			}
			int num = addendumTimestamp.CompareTo(addendumTimestamp2);
			if (num != 0)
			{
				return num;
			}
			if (index > other.index)
			{
				return 1;
			}
			return -1;
		}
	}

	private Quest m_Quest;

	private ObjectiveNode m_ObjectiveNode;

	private int m_NodeId;

	public UISprite ExpandSprite;

	public UILabel TitleLabel;

	public UILabel TimestampLabel;

	public UILabel DescriptionLabel;

	public int TitleTimestampMargin = 20;

	private BoxCollider m_ExpandCollider;

	private bool m_Expanded;

	private string m_ObjectiveTitleColor
	{
		get
		{
			if (m_Quest == null || m_Quest.IsQuestStateEnded(m_NodeId))
			{
				return "[" + NGUITools.EncodeColor(UIJournalManager.Instance.QuestObjectiveDisabledColor) + "]";
			}
			return "[" + NGUITools.EncodeColor(UIJournalManager.Instance.QuestObjectiveTitleColor) + "]";
		}
	}

	private string m_ObjectiveColor
	{
		get
		{
			if (m_Quest == null || m_Quest.IsQuestStateEnded(m_NodeId))
			{
				return "[" + NGUITools.EncodeColor(UIJournalManager.Instance.QuestObjectiveDisabledColor) + "]";
			}
			return "[" + NGUITools.EncodeColor(UIJournalManager.Instance.QuestObjectiveColor) + "]";
		}
	}

	public bool Expanded
	{
		get
		{
			return m_Expanded;
		}
		set
		{
			if (GameState.Option.DisplayQuestObjectiveTitles)
			{
				m_Expanded = value;
			}
			else
			{
				m_Expanded = true;
			}
			if (ExpandSprite != null)
			{
				if (!m_Expanded)
				{
					ExpandSprite.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
				}
				else
				{
					ExpandSprite.transform.localRotation = Quaternion.identity;
				}
			}
		}
	}

	public float ContentHeight
	{
		get
		{
			if (Expanded)
			{
				return NGUIMath.CalculateRelativeWidgetBounds(base.transform).size.y;
			}
			return Mathf.Max(TimestampLabel.transform.localScale.y * TitleLabel.relativeSize.y, TitleLabel.transform.localScale.y * TitleLabel.relativeSize.y) + 30f;
		}
	}

	private void Start()
	{
		Expanded = false;
		m_ExpandCollider = GetComponentInChildren<BoxCollider>();
		if (m_ExpandCollider != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(m_ExpandCollider.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnExpandClick));
		}
	}

	private void OnDestroy()
	{
		if (m_ExpandCollider != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(m_ExpandCollider.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnExpandClick));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnExpandClick(GameObject go)
	{
		if (ExpandSprite.gameObject.activeSelf)
		{
			Expanded = !Expanded;
			UIJournalManager.Instance.RefreshContent();
		}
	}

	public void SetContent(Quest quest, int nodeid)
	{
		m_Quest = quest;
		m_ObjectiveNode = m_Quest.GetNode(nodeid) as ObjectiveNode;
		m_NodeId = nodeid;
		RebuildContent();
	}

	public void RebuildContent()
	{
		Expanded = Expanded;
		if (m_Quest == null)
		{
			Debug.LogError("ERROR: m_Quest is null (UIJournalQuestObjective.RebuildContent ())");
			return;
		}
		if (m_ObjectiveNode == null)
		{
			Debug.LogError("ERROR: m_ObjectiveNode is null (UIJournalQuestObjective.RebuildContent ())");
			return;
		}
		if (TitleLabel != null)
		{
			string text = m_Quest.GetObjectiveTitle(m_ObjectiveNode);
			if (string.IsNullOrEmpty(text))
			{
				text = "[NO NAME]";
			}
			TitleLabel.text = m_ObjectiveTitleColor + text;
			TitleLabel.gameObject.SetActive(GameState.Option.DisplayQuestObjectiveTitles);
		}
		if (TimestampLabel != null)
		{
			EternityDateTime stateTimestamp = QuestManager.Instance.GetStateTimestamp(m_Quest, m_NodeId);
			if (stateTimestamp != null)
			{
				TimestampLabel.text = m_ObjectiveColor + stateTimestamp.Format(GUIUtils.GetText(264)) + "\n" + stateTimestamp.GetDate();
			}
			else
			{
				TimestampLabel.text = "";
			}
		}
		if (!(DescriptionLabel != null))
		{
			return;
		}
		DescriptionLabel.text = m_ObjectiveColor + m_Quest.GetObjectiveDescription(m_ObjectiveNode);
		List<ChronologicalAddendum> list = new List<ChronologicalAddendum>();
		foreach (int addendumID in m_ObjectiveNode.AddendumIDs)
		{
			if (QuestManager.Instance.IsAddendumTriggered(m_Quest, addendumID))
			{
				list.Add(new ChronologicalAddendum(addendumID, m_Quest));
			}
		}
		list.Sort();
		foreach (ChronologicalAddendum item in list)
		{
			UILabel descriptionLabel = DescriptionLabel;
			descriptionLabel.text = descriptionLabel.text + Environment.NewLine + Environment.NewLine + m_Quest.GetAddendumDescription(m_ObjectiveNode, item.index);
		}
		DescriptionLabel.gameObject.SetActive(m_Expanded);
		ExpandSprite.gameObject.SetActive(!string.IsNullOrEmpty(DescriptionLabel.text) && GameState.Option.DisplayQuestObjectiveTitles);
		if (!ExpandSprite.gameObject.activeSelf)
		{
			Expanded = false;
		}
	}
}
