using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EternityTimeInterval : IComparable<EternityTimeInterval>
{
	[SerializeField]
	private int m_TotalSeconds;

	private int m_Second;

	private int m_Minute;

	private int m_Hour;

	private int m_Day;

	private int m_Month;

	private int m_Year;

	public int SerializedSeconds
	{
		get
		{
			return m_TotalSeconds;
		}
		set
		{
			m_TotalSeconds = value;
			Recalculate();
		}
	}

	private static WorldTime WT => WorldTime.Instance;

	public EternityTimeInterval()
	{
		Recalculate();
	}

	public EternityTimeInterval(int seconds)
	{
		m_TotalSeconds = seconds;
		Recalculate();
	}

	public EternityTimeInterval(EternityTimeInterval other)
	{
		m_TotalSeconds = other.TotalSeconds();
		Recalculate();
	}

	public void AddSeconds(int seconds)
	{
		if (seconds != 0)
		{
			m_TotalSeconds += seconds;
			Recalculate();
		}
	}

	public void Add(EternityTimeInterval other)
	{
		if (other.m_TotalSeconds != 0)
		{
			m_TotalSeconds += other.m_TotalSeconds;
			Recalculate();
		}
	}

	private void Recalculate()
	{
		if (!(WT == null))
		{
			m_Second = m_TotalSeconds;
			if (WT.SecondsPerYear == 0)
			{
				Debug.LogError("Error in EternityTime::Recalculate - SecondsPerYear is zero.");
				return;
			}
			m_Year = m_Second / WT.SecondsPerYear;
			m_Second -= m_Year * WT.SecondsPerYear;
			m_Month = m_Second / (WT.SecondsPerDay * WT.DaysPerMonth);
			m_Second -= m_Month * WT.SecondsPerDay * WT.DaysPerMonth;
			m_Day = m_Second / WT.SecondsPerDay;
			m_Second -= m_Day * WT.SecondsPerDay;
			m_Hour = m_Second / (WT.MinutesPerHour * WT.SecondsPerMinute);
			m_Second -= m_Hour * WT.MinutesPerHour * WT.SecondsPerMinute;
			m_Minute = m_Second / WT.SecondsPerMinute;
			m_Second -= m_Minute * WT.SecondsPerMinute;
		}
	}

	public static EternityTimeInterval operator +(EternityTimeInterval a, EternityTimeInterval b)
	{
		return new EternityTimeInterval(a.m_TotalSeconds + b.m_TotalSeconds);
	}

	public static EternityTimeInterval operator -(EternityTimeInterval a, EternityTimeInterval b)
	{
		return new EternityTimeInterval(a.m_TotalSeconds - b.m_TotalSeconds);
	}

	public override bool Equals(object obj)
	{
		if (obj is EternityTimeInterval)
		{
			return ((EternityTimeInterval)obj).m_TotalSeconds == m_TotalSeconds;
		}
		return false;
	}

	public int TotalSeconds()
	{
		return m_TotalSeconds;
	}

	public int TotalMinutes()
	{
		return (int)Mathf.Floor((float)m_TotalSeconds / (float)WorldTime.Instance.SecondsPerMinute);
	}

	public int TotalHours()
	{
		return (int)Mathf.Floor((float)m_TotalSeconds / (float)(WorldTime.Instance.SecondsPerMinute * WorldTime.Instance.MinutesPerHour));
	}

	public int TotalDays()
	{
		return (int)Mathf.Floor((float)m_TotalSeconds / (float)WorldTime.Instance.SecondsPerDay);
	}

	public int TotalYears()
	{
		return (int)Mathf.Floor((float)m_TotalSeconds / (float)WorldTime.Instance.SecondsPerYear);
	}

	public string Format(string format)
	{
		return StringUtility.Format(format, m_Second, m_Minute, m_Hour, m_Day, m_Year);
	}

	public string FormatNonZero(int minElement)
	{
		object[] param = new object[6] { m_Second, m_Minute, m_Hour, m_Day, m_Month, m_Year };
		List<string> list = new List<string>();
		if (m_Year > 0 && minElement <= 5)
		{
			if (m_Year == 1)
			{
				list.Add(GUIUtils.GetText(346));
			}
			else
			{
				list.Add(GUIUtils.GetText(265));
			}
		}
		if (m_Month > 0 && minElement <= 4)
		{
			if (m_Month == 1)
			{
				list.Add(GUIUtils.GetText(347));
			}
			else
			{
				list.Add(GUIUtils.GetText(266));
			}
		}
		if (m_Day > 0 && minElement <= 3)
		{
			if (m_Day == 1)
			{
				list.Add(GUIUtils.GetText(348));
			}
			else
			{
				list.Add(GUIUtils.GetText(267));
			}
		}
		if (m_Hour > 0 && minElement <= 2)
		{
			if (m_Hour == 1)
			{
				list.Add(GUIUtils.GetText(349));
			}
			else
			{
				list.Add(GUIUtils.GetText(268));
			}
		}
		if (m_Minute > 0 && minElement <= 1)
		{
			if (m_Minute == 1)
			{
				list.Add(GUIUtils.GetText(350));
			}
			else
			{
				list.Add(GUIUtils.GetText(269));
			}
		}
		if ((m_Second > 0 || list.Count == 0) && minElement <= 0)
		{
			if (m_Second == 1)
			{
				list.Add(GUIUtils.GetText(351));
			}
			else
			{
				list.Add(GUIUtils.GetText(270));
			}
		}
		if (list.Count == 0)
		{
			return FormatNonZero(minElement - 1);
		}
		return StringUtility.Format(string.Join(", ", list.ToArray()), param);
	}

	public override string ToString()
	{
		return FormatNonZero(0);
	}

	public int CompareTo(EternityTimeInterval obj)
	{
		return m_TotalSeconds.CompareTo(obj.m_TotalSeconds);
	}

	public override int GetHashCode()
	{
		return m_TotalSeconds;
	}
}
