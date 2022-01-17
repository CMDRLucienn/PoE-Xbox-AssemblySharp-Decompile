using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class EternityDateTime : IComparable<EternityDateTime>
{
	[NonSerialized]
	private int m_TotalSeconds;

	[NonSerialized]
	private int m_Second;

	[NonSerialized]
	private int m_Minute;

	[NonSerialized]
	private int m_Hour;

	[NonSerialized]
	private int m_Day;

	[NonSerialized]
	private int m_Month;

	[NonSerialized]
	private int m_Year;

	[NonSerialized]
	private int m_InternalMonth;

	[NonSerialized]
	private string m_TimeString;

	[NonSerialized]
	private string m_DateString;

	public int AdventureDay => (this - WorldTime.Instance.AdventureStart).TotalDays() + 1;

	public int TotalSeconds
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

	[XmlIgnore]
	public int Second
	{
		get
		{
			return m_Second + 1;
		}
		set
		{
			m_Second = value - 1;
		}
	}

	[XmlIgnore]
	public int Minute
	{
		get
		{
			return m_Minute + 1;
		}
		set
		{
			m_Minute = value - 1;
		}
	}

	[XmlIgnore]
	public int Hour
	{
		get
		{
			return m_Hour + 1;
		}
		set
		{
			m_Hour = value - 1;
		}
	}

	[XmlIgnore]
	public int Day
	{
		get
		{
			return m_Day + 1;
		}
		set
		{
			m_Day = value - 1;
		}
	}

	[XmlIgnore]
	public int Month
	{
		get
		{
			return m_Month + 1;
		}
		set
		{
			m_Month = value - 1;
		}
	}

	[XmlIgnore]
	public int Year
	{
		get
		{
			return m_Year + WT.StartYear;
		}
		set
		{
			m_Year = value - 1;
		}
	}

	[XmlIgnore]
	public int InternalMonth
	{
		get
		{
			return m_InternalMonth;
		}
		set
		{
			m_InternalMonth = value;
		}
	}

	private static WorldTime WT => WorldTime.Instance;

	public string GetDayOfWeekName()
	{
		return WT.DayNames[m_Day % WT.DayNames.Length].GetText();
	}

	public bool IsRealMonth()
	{
		return m_Month >= 0;
	}

	public bool IsTransitionMonth()
	{
		return m_Month < 0;
	}

	public string GetMonthOrTransitionName()
	{
		if (IsRealMonth())
		{
			return WT.MonthNames[m_Month].GetText();
		}
		int num = 0;
		for (int i = 0; i < m_InternalMonth; i++)
		{
			if (WT.MonthLayout[i] != WT.DaysPerMonth)
			{
				num += WT.MonthLayout[i];
			}
		}
		return WT.TransitionNames[num + m_Day].GetText();
	}

	public EternityDateTime()
		: this(0)
	{
	}

	public EternityDateTime(EternityDateTime other)
		: this(other.m_TotalSeconds)
	{
	}

	public EternityDateTime(int year, int month, int day, int hour, int minute, int second)
	{
		m_TotalSeconds = second - 1 + (minute - 1) * WT.SecondsPerMinute + (hour - 1) * WT.SecondsPerMinute * WT.MinutesPerHour + (day - 1) * WT.SecondsPerDay + (year - WT.StartYear) * WT.SecondsPerYear;
		month--;
		if (month >= WT.MonthLayout.Length)
		{
			Debug.LogError("Tried to construct an EternityDateTime with an invalid month.");
		}
		int num = 0;
		for (int i = 0; i < WT.MonthLayout.Length; i++)
		{
			if (month <= 0)
			{
				break;
			}
			num += WT.MonthLayout[i];
			if (WT.MonthLayout[i] == WT.DaysPerMonth)
			{
				month--;
			}
		}
		AddDays(num);
	}

	public EternityDateTime(int totalseconds)
	{
		m_TotalSeconds = totalseconds;
		Recalculate();
	}

	public void Add(EternityTimeInterval interval)
	{
		AddSeconds(interval.TotalSeconds());
	}

	public void AddSeconds(int seconds)
	{
		if (seconds > 0)
		{
			m_TotalSeconds += seconds;
			Recalculate();
		}
	}

	public void AddMinutes(int minutes)
	{
		AddSeconds(minutes * WT.SecondsPerMinute);
	}

	public void AddHours(int hours)
	{
		AddMinutes(hours * WT.MinutesPerHour);
	}

	public void AddDays(int days)
	{
		AddSeconds(days * WT.SecondsPerDay);
	}

	public void AddYears(int years)
	{
		AddSeconds(years * WT.SecondsPerYear);
	}

	private void Recalculate()
	{
		if (WT == null)
		{
			return;
		}
		m_Second = m_TotalSeconds;
		if (WT.SecondsPerYear == 0)
		{
			Debug.LogError("Error in EternityTime::Recalculate - SecondsPerYear is zero.");
			return;
		}
		m_Year = m_Second / WT.SecondsPerYear;
		m_Second -= m_Year * WT.SecondsPerYear;
		m_Day = m_Second / WT.SecondsPerDay;
		m_Second -= m_Day * WT.SecondsPerDay;
		m_Month = 0;
		m_InternalMonth = -1;
		for (int i = 0; i < WT.MonthLayout.Length; i++)
		{
			if (WT.MonthLayout[i] > m_Day)
			{
				m_InternalMonth = i;
				if (WT.MonthLayout[i] != WT.DaysPerMonth)
				{
					m_Month = -1;
				}
				break;
			}
			m_Day -= WT.MonthLayout[i];
			if (WT.MonthLayout[i] == WT.DaysPerMonth)
			{
				m_Month++;
			}
		}
		if (m_InternalMonth < 0)
		{
			Debug.LogError("There must be a bug in EternityDateTime::Recalculate.");
		}
		m_Hour = m_Second / (WT.MinutesPerHour * WT.SecondsPerMinute);
		m_Second -= m_Hour * WT.MinutesPerHour * WT.SecondsPerMinute;
		m_Minute = m_Second / WT.SecondsPerMinute;
		m_Second -= m_Minute * WT.SecondsPerMinute;
		m_TimeString = null;
		m_DateString = null;
	}

	public static EternityTimeInterval operator -(EternityDateTime a, EternityDateTime b)
	{
		return new EternityTimeInterval(a.m_TotalSeconds - b.m_TotalSeconds);
	}

	public static EternityDateTime operator +(EternityDateTime a, EternityTimeInterval b)
	{
		return new EternityDateTime(a.TotalSeconds + b.TotalSeconds());
	}

	public override bool Equals(object obj)
	{
		if (obj is EternityDateTime)
		{
			return ((EternityDateTime)obj).m_TotalSeconds == m_TotalSeconds;
		}
		return false;
	}

	public string Format(string format)
	{
		return StringUtility.Format(format, Second, Minute, Hour, Day, Year, GetDayOfWeekName(), GetMonthOrTransitionName(), AdventureDay);
	}

	public string GetTime()
	{
		if (m_TimeString == null)
		{
			m_TimeString = Format("{2}:{1,0:D2}");
		}
		return m_TimeString;
	}

	public string GetDate()
	{
		if (m_DateString == null)
		{
			if (IsRealMonth())
			{
				m_DateString = Format("{3} {6}, {4} " + WT.YearName.GetText());
			}
			else
			{
				m_DateString = Format("{6}, {4} " + WT.YearName.GetText());
			}
		}
		return m_DateString;
	}

	public override string ToString()
	{
		return GetTime() + ", " + GetDate();
	}

	public override int GetHashCode()
	{
		return m_TotalSeconds;
	}

	public int CompareTo(EternityDateTime other)
	{
		if (other == null)
		{
			return -1;
		}
		return m_TotalSeconds.CompareTo(other.m_TotalSeconds);
	}
}
