using System;
using System.Collections.Generic;
using AnimationOrTween;
using OEIFormats.FlowCharts.Quests;
using UnityEngine;

public class UIQuestNotifications : MonoBehaviour
{
	private interface Notification
	{
	}

	private class QuestNotification : Notification
	{
		public QuestNotificationType Type;

		public Quest Quest;

		public ObjectiveNode Data;

		public string DataString;

		public QuestNotification(QuestNotificationType type, Quest quest)
		{
			Type = type;
			Quest = quest;
			Data = null;
			DataString = null;
		}

		public QuestNotification(QuestNotificationType type, Quest quest, ObjectiveNode data)
			: this(type, quest)
		{
			Data = data;
			DataString = null;
		}

		public QuestNotification(QuestNotificationType type, Quest quest, string data)
			: this(type, quest)
		{
			Data = null;
			DataString = data;
		}
	}

	private class StrongholdNotification : Notification
	{
		public string Message;

		public StrongholdNotification(string message)
		{
			Message = message;
		}
	}

	public enum QuestNotificationType
	{
		QUEST_ADDED,
		QUEST_UPDATED,
		QUEST_COMPLETED,
		QUEST_FAILED,
		QUEST_ADDENDUM
	}

	public GameObject QuestContent;

	public UILabel TypeLabel;

	public UILabel TitleLabel;

	public UILabel ObjectiveLabel;

	public UISprite IconTexture;

	public GameObject StrongholdContent;

	public UILabel StrongholdText;

	public UIPanel ParchmentPanel;

	public UIPanel ObjectivesPanel;

	public UIPanel ParchmentSubPanel;

	public GameObject BackgroundCollider;

	public UITweenerAggregator ParchmentTween;

	public UITweenerAggregator ObjectiveTween;

	public UITweener ParchmentTweenOnly;

	public TweenScale BackgroundTweener;

	public GameObject CenterOffsetter;

	public float ShowTime = 5f;

	private float m_ShowTime;

	public UIQuestNotificationObjective RootObjective;

	private List<UIQuestNotificationObjective> m_Objectives;

	private Notification m_Current;

	private static GUIDatabaseString[] m_TypeStrings = new GUIDatabaseString[5]
	{
		new GUIDatabaseString(889),
		new GUIDatabaseString(892),
		new GUIDatabaseString(890),
		new GUIDatabaseString(891),
		new GUIDatabaseString(1028)
	};

	private static GUIDatabaseString[] m_TypeStringsTask = new GUIDatabaseString[5]
	{
		new GUIDatabaseString(1479),
		new GUIDatabaseString(1482),
		new GUIDatabaseString(1480),
		new GUIDatabaseString(1481),
		new GUIDatabaseString(1028)
	};

	private List<Notification> m_Queue = new List<Notification>();

	public static UIQuestNotifications Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameResources.OnLoadedSave -= OnLoadedSave;
		if ((bool)Stronghold.Instance)
		{
			Stronghold instance = Stronghold.Instance;
			instance.OnLogMessage = (Stronghold.LogMessageDelegate)Delegate.Remove(instance.OnLogMessage, new Stronghold.LogMessageDelegate(OnStrongholdLog));
		}
		if ((bool)QuestManager.Instance)
		{
			QuestManager instance2 = QuestManager.Instance;
			instance2.OnQuestStarted = (QuestManager.QuestDelegate)Delegate.Remove(instance2.OnQuestStarted, new QuestManager.QuestDelegate(QuestStarted));
			QuestManager instance3 = QuestManager.Instance;
			instance3.OnQuestUpdated = (QuestManager.QuestObjectiveDelegate)Delegate.Remove(instance3.OnQuestUpdated, new QuestManager.QuestObjectiveDelegate(QuestUpdated));
			QuestManager instance4 = QuestManager.Instance;
			instance4.OnQuestCompleted = (QuestManager.QuestDelegate)Delegate.Remove(instance4.OnQuestCompleted, new QuestManager.QuestDelegate(QuestCompleted));
			QuestManager instance5 = QuestManager.Instance;
			instance5.OnQuestFailed = (QuestManager.QuestDelegate)Delegate.Remove(instance5.OnQuestFailed, new QuestManager.QuestDelegate(QuestFailed));
			QuestManager instance6 = QuestManager.Instance;
			instance6.OnQuestAddendum = (QuestManager.QuestAddendumDelegate)Delegate.Remove(instance6.OnQuestAddendum, new QuestManager.QuestAddendumDelegate(QuestAddendum));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		QuestManager instance = QuestManager.Instance;
		instance.OnQuestStarted = (QuestManager.QuestDelegate)Delegate.Combine(instance.OnQuestStarted, new QuestManager.QuestDelegate(QuestStarted));
		QuestManager instance2 = QuestManager.Instance;
		instance2.OnQuestUpdated = (QuestManager.QuestObjectiveDelegate)Delegate.Combine(instance2.OnQuestUpdated, new QuestManager.QuestObjectiveDelegate(QuestUpdated));
		QuestManager instance3 = QuestManager.Instance;
		instance3.OnQuestCompleted = (QuestManager.QuestDelegate)Delegate.Combine(instance3.OnQuestCompleted, new QuestManager.QuestDelegate(QuestCompleted));
		QuestManager instance4 = QuestManager.Instance;
		instance4.OnQuestFailed = (QuestManager.QuestDelegate)Delegate.Combine(instance4.OnQuestFailed, new QuestManager.QuestDelegate(QuestFailed));
		QuestManager instance5 = QuestManager.Instance;
		instance5.OnQuestAddendum = (QuestManager.QuestAddendumDelegate)Delegate.Combine(instance5.OnQuestAddendum, new QuestManager.QuestAddendumDelegate(QuestAddendum));
		GameResources.OnLoadedSave += OnLoadedSave;
		Stronghold instance6 = Stronghold.Instance;
		instance6.OnLogMessage = (Stronghold.LogMessageDelegate)Delegate.Combine(instance6.OnLogMessage, new Stronghold.LogMessageDelegate(OnStrongholdLog));
		float num3 = (ParchmentPanel.alpha = (ParchmentSubPanel.alpha = 0f));
		ObjectivesPanel.alpha = 0f;
		UIEventListener uIEventListener = UIEventListener.Get(BackgroundCollider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
		Init();
	}

	private void OnLoadedSave()
	{
		m_Queue.Clear();
		m_ShowTime = 0f;
		ObjectiveTween.Play(forward: false);
		ParchmentTween.Play(forward: false);
		float num3 = (ParchmentPanel.alpha = (ParchmentSubPanel.alpha = 0f));
	}

	private void OnClick(GameObject sender)
	{
		if (m_Current is QuestNotification)
		{
			QuestNotification questNotification = m_Current as QuestNotification;
			UIJournalManager.Instance.ChangeScreen(UIJournalManager.JournalScreen.QUESTS);
			UIJournalManager.Instance.ShowWindow();
			UIJournalManager.Instance.SetSelectedItem(questNotification.Quest, UIJournalManager.JournalScreen.QUESTS);
		}
		else if (m_Current is StrongholdNotification)
		{
			UIStrongholdManager.Instance.ShowForPane = Stronghold.WindowPane.Actions;
			UIStrongholdManager.Instance.ShowWindow();
			ClearStronghold();
			EndShowCurrent();
		}
	}

	private void Init()
	{
		if (m_Objectives == null)
		{
			m_Objectives = new List<UIQuestNotificationObjective>();
			m_Objectives.Add(RootObjective);
			RootObjective.gameObject.SetActive(value: false);
			RootObjective.gameObject.name = "0000";
		}
	}

	private void OnStrongholdLog(Stronghold.NotificationType type, string timestamp, string message)
	{
		if (type != 0)
		{
			PushStronghold(message);
		}
	}

	private void QuestStarted(Quest quest)
	{
		PushQuest(QuestNotificationType.QUEST_ADDED, quest);
	}

	private void QuestUpdated(Quest quest, ObjectiveNode node)
	{
		PushQuest(QuestNotificationType.QUEST_UPDATED, quest, node);
	}

	private void QuestCompleted(Quest quest)
	{
		PushQuest(QuestNotificationType.QUEST_COMPLETED, quest);
	}

	private void QuestFailed(Quest quest)
	{
		PushQuest(QuestNotificationType.QUEST_FAILED, quest);
	}

	private void QuestAddendum(Quest quest, int addendumId)
	{
		PushQuest(QuestNotificationType.QUEST_ADDENDUM, quest, addendumId);
	}

	private void ParchmentTweenFinished(UITweener tween)
	{
		if (tween.direction == Direction.Reverse)
		{
			bool flag = true;
			UITweener[] tweeners = ParchmentTween.Tweeners;
			foreach (UITweener uITweener in tweeners)
			{
				flag &= uITweener.tweenFactor == 0f;
			}
			if (flag)
			{
				m_Current = null;
				float num3 = (ParchmentPanel.alpha = (ParchmentSubPanel.alpha = 0f));
				Pull();
			}
		}
	}

	private void ObjectiveTweenFinished(UITweener tween)
	{
		if (tween.direction == Direction.Reverse)
		{
			bool flag = true;
			UITweener[] tweeners = ObjectiveTween.Tweeners;
			foreach (UITweener uITweener in tweeners)
			{
				flag &= uITweener.tweenFactor == 0f;
			}
			if (flag)
			{
				m_Current = null;
				ObjectivesPanel.alpha = 0f;
			}
		}
	}

	public void ClearStronghold()
	{
		for (int num = m_Queue.Count - 1; num >= 0; num--)
		{
			if (m_Queue[num] is StrongholdNotification)
			{
				m_Queue.RemoveAt(num);
			}
		}
	}

	public void PushStronghold(string text)
	{
		m_Queue.Add(new StrongholdNotification(text));
	}

	public void PushQuest(QuestNotificationType type, Quest quest)
	{
		if (type == QuestNotificationType.QUEST_ADDED || type == QuestNotificationType.QUEST_FAILED)
		{
			bool flag = false;
			for (int num = m_Queue.Count - 1; num >= 0; num--)
			{
				if (m_Queue[num] is QuestNotification questNotification && questNotification.Quest != quest)
				{
					m_Queue.Insert(num + 1, new QuestNotification(type, quest));
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_Queue.Insert(0, new QuestNotification(type, quest));
			}
		}
		else
		{
			m_Queue.Add(new QuestNotification(type, quest));
		}
	}

	public void PushQuest(QuestNotificationType type, Quest quest, ObjectiveNode node)
	{
		if (node != null && GameState.Option.DisplayQuestObjectiveTitles && GameState.Option.GetOption(GameOption.BoolOption.QUEST_UPDATES_UI))
		{
			m_Queue.Add(new QuestNotification(type, quest, node));
		}
	}

	public void PushQuest(QuestNotificationType type, Quest quest, int addendumID)
	{
		string addendumDescription = quest.GetAddendumDescription(quest.GetNode(quest.GetAddendumNode(addendumID)) as ObjectiveNode, addendumID);
		if (GameState.Option.DisplayQuestObjectiveTitles && GameState.Option.GetOption(GameOption.BoolOption.QUEST_UPDATES_UI))
		{
			m_Queue.Add(new QuestNotification(type, quest, addendumDescription));
		}
	}

	private void Pull(bool force = false)
	{
		if (GameState.IsLoading)
		{
			return;
		}
		if (m_Queue.Count > 0)
		{
			if (force || !UIWindowManager.Instance.AnyWindowShowing())
			{
				m_Current = m_Queue[0];
				m_Queue.RemoveAt(0);
				if (m_Current is StrongholdNotification)
				{
					ShowStrongholdNotification();
				}
				else if (m_Current is QuestNotification)
				{
					ShowQuestNotification();
				}
				ParchmentTween.Play(forward: true);
				float num3 = (ParchmentPanel.alpha = (ParchmentSubPanel.alpha = 1f));
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(CenterOffsetter.transform);
				BackgroundTweener.to.x = bounds.size.x + 40f;
				BackgroundTweener.Play(forward: true);
				m_ShowTime = ShowTime;
			}
		}
		else if (force)
		{
			m_ShowTime = 0.001f;
		}
	}

	private void ShowQuestNotification()
	{
		QuestContent.gameObject.SetActive(value: true);
		StrongholdContent.gameObject.SetActive(value: false);
		IconTexture.spriteName = "parch_ico_journal";
		int i = 0;
		QuestNotification questNotification = m_Current as QuestNotification;
		if (GlobalAudioPlayer.Instance != null)
		{
			switch (questNotification.Type)
			{
			case QuestNotificationType.QUEST_ADDED:
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.QuestRecieved);
				break;
			case QuestNotificationType.QUEST_COMPLETED:
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.QuestComplete);
				break;
			case QuestNotificationType.QUEST_FAILED:
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.QuestFailed);
				break;
			default:
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.QuestUpdated);
				break;
			}
		}
		if (questNotification.Type == QuestNotificationType.QUEST_UPDATED)
		{
			GetObjective(i).Set(questNotification.Quest, questNotification.Data);
			i++;
		}
		TitleLabel.text = questNotification.Quest.GetQuestTitle();
		if ((bool)questNotification.Quest && questNotification.Quest.GetQuestType() == QuestType.Task)
		{
			TypeLabel.text = m_TypeStringsTask[(int)questNotification.Type].GetText() + ":";
		}
		else
		{
			TypeLabel.text = m_TypeStrings[(int)questNotification.Type].GetText() + ":";
		}
		if (questNotification.Type == QuestNotificationType.QUEST_UPDATED || questNotification.Type == QuestNotificationType.QUEST_ADDED)
		{
			List<ObjectiveNode> list = new List<ObjectiveNode>(m_Queue.Count);
			for (int num = m_Queue.Count - 1; num >= 0; num--)
			{
				if (m_Queue[num] is QuestNotification questNotification2 && questNotification2.Quest == questNotification.Quest && questNotification2.Type == QuestNotificationType.QUEST_UPDATED && questNotification2.Data != null && !list.Contains(questNotification2.Data))
				{
					GetObjective(i).Set(questNotification2.Quest, questNotification2.Data);
					i++;
					list.Add(questNotification2.Data);
					m_Queue.RemoveAt(num);
				}
			}
		}
		for (int num2 = m_Queue.Count - 1; num2 >= 0; num2--)
		{
			if (m_Queue[num2] is QuestNotification questNotification3 && questNotification3.Quest == questNotification.Quest && questNotification3.Type == QuestNotificationType.QUEST_ADDENDUM)
			{
				m_Queue.RemoveAt(num2);
			}
		}
		if (i > 0)
		{
			ObjectiveTween.Play(forward: true);
		}
		for (; i < m_Objectives.Count; i++)
		{
			m_Objectives[i].gameObject.SetActive(value: false);
		}
		float num3 = 0f;
		for (i = 0; i < m_Objectives.Count; i++)
		{
			UIQuestNotificationObjective uIQuestNotificationObjective = m_Objectives[i];
			if (uIQuestNotificationObjective.gameObject.activeSelf)
			{
				uIQuestNotificationObjective.transform.localPosition = new Vector3(uIQuestNotificationObjective.transform.localPosition.x, num3, uIQuestNotificationObjective.transform.localPosition.z);
				num3 -= uIQuestNotificationObjective.Height;
				continue;
			}
			break;
		}
	}

	private void ShowStrongholdNotification()
	{
		QuestContent.gameObject.SetActive(value: false);
		StrongholdContent.gameObject.SetActive(value: true);
		IconTexture.spriteName = "parch_ico_stronghold";
		StrongholdNotification strongholdNotification = m_Current as StrongholdNotification;
		StrongholdText.text = strongholdNotification.Message;
		for (int i = 0; i < m_Objectives.Count; i++)
		{
			m_Objectives[i].gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!GameState.Option.GetOption(GameOption.BoolOption.QUEST_UPDATES_UI))
		{
			for (int num = m_Queue.Count - 1; num >= 0; num--)
			{
				if (m_Queue[num] is QuestNotification)
				{
					m_Queue.RemoveAt(num);
				}
			}
			if (m_Current is QuestNotification)
			{
				EndShowCurrent();
			}
		}
		if (m_ShowTime > 0f)
		{
			m_ShowTime -= TimeController.sUnscaledDelta;
			if (m_ShowTime <= 0f)
			{
				EndShowCurrent();
			}
		}
		else if (!ParchmentTweenOnly.enabled)
		{
			Pull();
		}
	}

	private void EndShowCurrent()
	{
		if (m_Queue.Count == 0)
		{
			ObjectiveTween.Play(forward: false);
			ParchmentTween.Play(forward: false);
		}
		else
		{
			ParchmentTweenOnly.Play(forward: false);
		}
	}

	public void ForceShow()
	{
		m_ShowTime = ShowTime;
		ParchmentTween.Play(forward: true);
	}

	private UIQuestNotificationObjective GetObjective(int index)
	{
		if (index < m_Objectives.Count)
		{
			m_Objectives[index].gameObject.SetActive(value: true);
			return m_Objectives[index];
		}
		UIQuestNotificationObjective component = NGUITools.AddChild(RootObjective.transform.parent.gameObject, RootObjective.gameObject).GetComponent<UIQuestNotificationObjective>();
		m_Objectives.Add(component);
		component.gameObject.name = index.ToString("0000");
		return component;
	}
}
