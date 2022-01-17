using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class Console
{
	public enum ConsoleState
	{
		Combat,
		Dialogue,
		Count,
		DialogueBig,
		Both
	}

	public class BatchedAttackData
	{
		public AttackBase Attack;

		public GenericAbility Ability;

		public DamageInfo DamageInfo;

		public object Key
		{
			get
			{
				if ((bool)Ability)
				{
					return Ability;
				}
				if (DamageInfo != null && (bool)DamageInfo.Ability)
				{
					return DamageInfo.Ability;
				}
				if ((bool)Attack)
				{
					return Attack;
				}
				if (DamageInfo != null && (bool)DamageInfo.Attack)
				{
					return DamageInfo.Attack;
				}
				return null;
			}
		}

		public BatchedAttackData(AttackBase attack)
		{
			Attack = attack;
		}

		public BatchedAttackData(GenericAbility ability)
		{
			Ability = ability;
		}

		public BatchedAttackData(DamageInfo damage)
		{
			DamageInfo = damage;
		}

		public static implicit operator BatchedAttackData(AttackBase attack)
		{
			return new BatchedAttackData(attack);
		}

		public static implicit operator BatchedAttackData(GenericAbility abil)
		{
			return new BatchedAttackData(abil);
		}

		public static implicit operator BatchedAttackData(DamageInfo damage)
		{
			return new BatchedAttackData(damage);
		}
	}

	public class ConsoleMessage
	{
		public readonly ConsoleState m_mode;

		public readonly string m_message;

		public readonly string m_verbosemessage;

		public readonly Color m_color;

		public BatchedAttackData UserData;

		public IList<ConsoleMessage> Children;

		public Action OnClickCallback;

		public bool IsEmpty => string.IsNullOrEmpty(m_message);

		public bool IsVerboseEmpty => string.IsNullOrEmpty(m_verbosemessage);

		public bool ForMode(ConsoleState mode)
		{
			if (m_mode == ConsoleState.Both || mode == ConsoleState.Both)
			{
				return true;
			}
			if (m_mode == ConsoleState.DialogueBig && mode == ConsoleState.Dialogue)
			{
				return true;
			}
			return mode == m_mode;
		}

		public ConsoleMessage(string message, ConsoleState mode)
			: this(message, mode, Color.white)
		{
		}

		public ConsoleMessage(string message, ConsoleState mode, Color color)
			: this(message, string.Empty, mode, color)
		{
		}

		public ConsoleMessage(string message, string verbosemessage, ConsoleState mode)
			: this(message, verbosemessage, mode, Color.white)
		{
		}

		public ConsoleMessage(string message, string verbosemessage, ConsoleState mode, Color color)
		{
			m_message = message;
			m_mode = mode;
			m_verbosemessage = verbosemessage;
			m_color = color;
			OnClickCallback = null;
			UserData = null;
			Children = null;
		}
	}

	private static List<ConsoleMessage> m_MessageBuffer;

	private static List<ConsoleMessage> m_DialogueMessageBuffer;

	private static int s_LoggedMessages;

	private static StringBuilder m_Builder;

	private object m_Lock = new object();

	public static Console Instance { get; private set; }

	public int MessageDelta => s_LoggedMessages;

	public void ResetMessageDelta()
	{
		s_LoggedMessages = 0;
	}

	static Console()
	{
		s_LoggedMessages = 0;
		m_Builder = new StringBuilder();
		Instance = new Console();
	}

	public static void ProcessBatched()
	{
		if (m_MessageBuffer == null)
		{
			return;
		}
		IEnumerable<IGrouping<object, ConsoleMessage>> enumerable = from cm in m_MessageBuffer
			where cm.UserData != null
			group cm by cm.UserData.Key;
		List<ConsoleMessage> list = new List<ConsoleMessage>();
		foreach (IGrouping<object, ConsoleMessage> item in enumerable)
		{
			if (item.Count((ConsoleMessage cm) => cm.UserData.DamageInfo != null) <= 2)
			{
				continue;
			}
			int[] array = new int[4];
			GameObject gameObject = null;
			GenericAbility genericAbility = null;
			GameObject gameObject2 = null;
			bool flag = false;
			float num = 0f;
			foreach (ConsoleMessage item2 in item)
			{
				DamageInfo damageInfo = item2.UserData.DamageInfo;
				if (damageInfo != null)
				{
					gameObject = gameObject ?? damageInfo.Owner;
					genericAbility = genericAbility ?? damageInfo.Ability;
					array[(int)damageInfo.HitType]++;
					if (!flag)
					{
						flag = true;
						gameObject2 = damageInfo.Target;
					}
					else if (gameObject2 != damageInfo.Target)
					{
						gameObject2 = null;
					}
					num += damageInfo.FinalAdjustedDamage;
				}
			}
			m_Builder.Remove(0, m_Builder.Length);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] > 0)
				{
					m_Builder.Append(array[i]);
					m_Builder.Append(' ');
					m_Builder.Append(DamageInfo.GetHitTypeString((HitType)i));
					m_Builder.Append(GUIUtils.Comma());
				}
			}
			if (m_Builder.Length >= GUIUtils.Comma().Length)
			{
				m_Builder.Remove(m_Builder.Length - GUIUtils.Comma().Length);
			}
			string text = m_Builder.ToString();
			m_Builder.Remove(0, m_Builder.Length);
			if ((bool)gameObject2)
			{
				m_Builder.Append('[');
				m_Builder.Append(NGUITools.EncodeColor(AttackBase.GetMessageColor(gameObject, gameObject2)));
				m_Builder.Append(']');
				m_Builder.Append(GUIUtils.Format(123, CharacterStats.NameColored(gameObject2), num.ToString("#0.0"), CharacterStats.NameColored(gameObject)));
				m_Builder.Append("[-]");
				if (!string.IsNullOrEmpty(text))
				{
					m_Builder.AppendGuiFormat(1731, text);
				}
			}
			else
			{
				m_Builder.Append(CharacterStats.NameColored(gameObject));
				if ((bool)genericAbility)
				{
					m_Builder.Append(GUIUtils.Format(1731, GenericAbility.Name(genericAbility)));
				}
				m_Builder.Append(": ");
				m_Builder.Append(text);
			}
			ConsoleMessage consoleMessage = new ConsoleMessage(m_Builder.ToString(), ConsoleState.Combat);
			consoleMessage.Children = item.ToArray();
			list.AddRange(item);
			Instance.AddMessage(consoleMessage);
		}
		foreach (ConsoleMessage item3 in list)
		{
			m_MessageBuffer.Remove(item3);
		}
	}

	public static void AddBatchedMessage(string msg, Color color, BatchedAttackData userdata)
	{
		AddBatchedMessage(msg, string.Empty, color, userdata);
	}

	public static void AddBatchedMessage(string msg, string verbose, BatchedAttackData userdata)
	{
		AddBatchedMessage(msg, verbose, Color.white, userdata);
	}

	public static void AddBatchedMessage(string msg, string verbose, Color color, BatchedAttackData userdata)
	{
		ConsoleMessage consoleMessage = new ConsoleMessage(msg, verbose, ConsoleState.Combat, color);
		consoleMessage.UserData = userdata;
		Instance.AddMessage(consoleMessage);
	}

	public static void InsertBatchedMessage(string msg, string verbose, Color color, int atMinusIndex, BatchedAttackData userdata)
	{
		ConsoleMessage consoleMessage = new ConsoleMessage(msg, verbose, ConsoleState.Combat, color);
		consoleMessage.UserData = userdata;
		Instance.InsertMessage(consoleMessage, atMinusIndex);
	}

	public static void AddMessage(string msg, ConsoleState mode)
	{
		Instance.AddMessage(new ConsoleMessage(msg, mode));
	}

	public static void AddMessage(string msg, Color color, ConsoleState mode)
	{
		Instance.AddMessage(new ConsoleMessage(msg, mode, color));
	}

	public static void AddMessage(string msg)
	{
		Instance.AddMessage(new ConsoleMessage(msg, ConsoleState.Combat));
	}

	public static void AddMessage(string msg, string verbose)
	{
		Instance.AddMessage(new ConsoleMessage(msg, verbose, ConsoleState.Combat));
	}

	public static void AddMessage(string msg, Color color)
	{
		Instance.AddMessage(new ConsoleMessage(msg, ConsoleState.Combat, color));
	}

	public static void AddMessage(string msg, string verbose, Color color)
	{
		Instance.AddMessage(new ConsoleMessage(msg, verbose, ConsoleState.Combat, color));
	}

	public void AddMessage(ConsoleMessage message)
	{
		Interlocked.Increment(ref s_LoggedMessages);
		lock (m_Lock)
		{
			if (m_MessageBuffer == null)
			{
				m_MessageBuffer = new List<ConsoleMessage>();
			}
			m_MessageBuffer.Add(message);
			if (message.m_mode == ConsoleState.Both)
			{
				if (m_DialogueMessageBuffer == null)
				{
					m_DialogueMessageBuffer = new List<ConsoleMessage>();
				}
				m_DialogueMessageBuffer.Add(message);
			}
		}
	}

	public static string Format(string fstring, params object[] parameters)
	{
		try
		{
			return string.Format(fstring, parameters);
		}
		catch (FormatException ex)
		{
			Debug.LogException(ex);
			Debug.LogError("Format Exception. fstring: '" + fstring + "'");
			return "[FF0000]FormatException: " + ex.Message + "[-]";
		}
	}

	public List<ConsoleMessage> FetchConsoleMessages()
	{
		lock (m_Lock)
		{
			ProcessBatched();
			List<ConsoleMessage> messageBuffer = m_MessageBuffer;
			m_MessageBuffer = null;
			return messageBuffer;
		}
	}

	public List<ConsoleMessage> FetchDialogueMessages()
	{
		return Interlocked.Exchange(ref m_DialogueMessageBuffer, null);
	}

	public void ClearDialogueMessages()
	{
		lock (m_Lock)
		{
			if (m_DialogueMessageBuffer != null)
			{
				m_DialogueMessageBuffer.Clear();
			}
		}
	}

	public void InsertMessage(string msg, string verbose, Color color, int atMinusIndex)
	{
		InsertMessage(new ConsoleMessage(msg, verbose, ConsoleState.Combat, color), atMinusIndex);
	}

	public void InsertMessage(ConsoleMessage message, int atMinusIndex)
	{
		lock (m_Lock)
		{
			if (m_MessageBuffer == null)
			{
				m_MessageBuffer = new List<ConsoleMessage>();
			}
			m_MessageBuffer.Insert(m_MessageBuffer.Count - atMinusIndex, message);
		}
	}
}
