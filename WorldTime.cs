using UnityEngine;

public class WorldTime : MonoBehaviour
{
	private EternityTimeInterval m_TimeInCombat = new EternityTimeInterval();

	private EternityTimeInterval m_TimeSpentTraveling = new EternityTimeInterval();

	private float m_realWorldPlayTimeStartTimestamp;

	public int GameSecondsPerRealSecond = 24;

	public int StartSecond;

	public int StartMinute = 30;

	public int StartHour = 8;

	public int StartDay = 18;

	public int StartMonth = 4;

	public int StartYear = 2823;

	public int SecondsPerMinute = 60;

	public int MinutesPerHour = 60;

	public int HoursPerDay = 24;

	public int DaysPerMonth = 20;

	public int MonthsPerYear = 16;

	public int DaytimeStartHour = 6;

	public int NighttimeStartHour = 20;

	private int m_DaysPerYear;

	public GUIDatabaseString[] TransitionNames;

	public GUIDatabaseString[] MonthNames;

	public GUIDatabaseString[] PlainMonthNames;

	public GUIDatabaseString[] DayNames;

	public GUIDatabaseString YearName;

	public int[] MonthLayout = new int[23]
	{
		1, 20, 20, 3, 20, 20, 20, 20, 3, 20,
		20, 1, 20, 20, 3, 20, 20, 20, 20, 3,
		20, 20, 1
	};

	private float SecondsTimer;

	private PE_DayNightRender DayNight;

	public static WorldTime Instance { get; private set; }

	[Persistent]
	public EternityDateTime CurrentTime { get; set; }

	[Persistent]
	public EternityDateTime AdventureStart { get; set; }

	[Persistent]
	public EternityTimeInterval TimeInCombat
	{
		get
		{
			return m_TimeInCombat;
		}
		set
		{
			m_TimeInCombat = new EternityTimeInterval(value);
		}
	}

	[Persistent]
	public EternityTimeInterval TimeSpentTravelling
	{
		get
		{
			return m_TimeSpentTraveling;
		}
		set
		{
			m_TimeSpentTraveling = new EternityTimeInterval(value);
		}
	}

	[Persistent]
	public float RealWorldPlayTime { get; set; }

	public int CurrentSecond => CurrentTime.Second;

	public int CurrentMinute => CurrentTime.Minute;

	public int CurrentHour => CurrentTime.Hour;

	public int CurrentDay => CurrentTime.Day;

	public int CurrentMonth => CurrentTime.Month;

	public int CurrentYear => CurrentTime.Year;

	public int TotalSecondsToday => CurrentHour * MinutesPerHour * SecondsPerMinute + CurrentMinute * SecondsPerMinute + CurrentSecond;

	public int DaysPerYear
	{
		get
		{
			if (m_DaysPerYear <= 0)
			{
				m_DaysPerYear = 0;
				int[] monthLayout = MonthLayout;
				foreach (int num in monthLayout)
				{
					m_DaysPerYear += num;
				}
			}
			return m_DaysPerYear;
		}
	}

	public int SecondsPerYear => DaysPerYear * SecondsPerDay;

	public int SecondsPerDay => HoursPerDay * MinutesPerHour * SecondsPerMinute;

	public int FrameWorldSeconds { get; private set; }

	public event WorldTimeEventHandler OnTimeJump;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'WorldTime' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		if (Conditionals.CommandLineArg("bb"))
		{
			StartHour = 8;
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Reset()
	{
		CurrentTime = new EternityDateTime(StartYear, StartMonth, StartDay, StartHour, StartMinute, StartSecond);
		AdventureStart = new EternityDateTime(CurrentTime);
		TimeInCombat = new EternityTimeInterval();
		TimeSpentTravelling = new EternityTimeInterval();
		SecondsTimer = 0f;
		FrameWorldSeconds = 0;
		RealWorldPlayTime = 0f;
		m_realWorldPlayTimeStartTimestamp = Time.realtimeSinceStartup;
	}

	private void Start()
	{
		if (CurrentTime == null)
		{
			CurrentTime = new EternityDateTime(StartYear, StartMonth, StartDay, StartHour, StartMinute, StartSecond);
		}
		if (AdventureStart == null)
		{
			AdventureStart = new EternityDateTime(CurrentTime);
		}
		if (TimeInCombat == null)
		{
			TimeInCombat = new EternityTimeInterval();
		}
		if (TimeSpentTravelling == null)
		{
			TimeSpentTravelling = new EternityTimeInterval();
		}
		m_realWorldPlayTimeStartTimestamp = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		SecondsTimer += Time.deltaTime * (float)GameSecondsPerRealSecond;
		if (Time.realtimeSinceStartup - m_realWorldPlayTimeStartTimestamp >= 1f)
		{
			RealWorldPlayTime += Time.realtimeSinceStartup - m_realWorldPlayTimeStartTimestamp;
			m_realWorldPlayTimeStartTimestamp = Time.realtimeSinceStartup;
		}
		FrameWorldSeconds = Mathf.FloorToInt(SecondsTimer);
		CurrentTime.AddSeconds(FrameWorldSeconds);
		SecondsTimer -= FrameWorldSeconds;
		if (GameState.InCombat)
		{
			TimeInCombat.AddSeconds(FrameWorldSeconds);
		}
		if (FrameWorldSeconds > 0)
		{
			UpdateDayNightCycle();
		}
	}

	public void AdvanceTimeByHours(int hoursToAdvance, bool isResting)
	{
		HandleCurrentTimeJump(hoursToAdvance * MinutesPerHour * SecondsPerMinute, isMapTravel: false, isResting);
		CurrentTime.AddHours(hoursToAdvance);
		Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(360), hoursToAdvance));
	}

	public void AdvanceTime(EternityTimeInterval interval, bool isTravel, bool isResting)
	{
		HandleCurrentTimeJump(interval.TotalSeconds(), isTravel, isResting);
		CurrentTime.Add(interval);
		if (isTravel)
		{
			TimeSpentTravelling.Add(interval);
		}
		Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(360), interval.TotalHours()));
	}

	public void AdvanceTimeToHour(int hour)
	{
		int num = 0;
		num = ((CurrentTime.Hour >= hour) ? (HoursPerDay - CurrentTime.Hour + hour) : (hour - CurrentTime.Hour));
		AdvanceTimeByHours(num, isResting: false);
	}

	public bool IsCurrentlyDaytime()
	{
		if (CurrentTime == null)
		{
			CurrentTime = new EternityDateTime(StartYear, StartMonth, StartDay, StartHour, StartMinute, StartSecond);
		}
		if (CurrentTime.Hour < NighttimeStartHour && CurrentTime.Hour >= DaytimeStartHour)
		{
			return true;
		}
		return false;
	}

	public bool IsCurrentlyNighttime()
	{
		if (CurrentTime == null)
		{
			CurrentTime = new EternityDateTime(StartYear, StartMonth, StartDay, StartHour, StartMinute, StartSecond);
		}
		if (CurrentTime.Hour >= NighttimeStartHour || CurrentTime.Hour < DaytimeStartHour)
		{
			return true;
		}
		return false;
	}

	private void UpdateDayNightCycle()
	{
		if ((bool)DayNight)
		{
			DayNight.twentyFourHourTime = (float)TotalSecondsToday / (float)SecondsPerDay * 24f;
		}
	}

	public void SetDayNightCycle(PE_DayNightRender dayNight)
	{
		DayNight = dayNight;
		UpdateDayNightCycle();
	}

	public void UnsetDayNightCycle(PE_DayNightRender dayNight)
	{
		if (DayNight == dayNight)
		{
			DayNight = null;
		}
	}

	public string GetTimeDisplayString()
	{
		return CurrentTime.GetTime();
	}

	public string GetDateDisplayString()
	{
		return CurrentTime.GetDate();
	}

	public void HandleCurrentTimeJump(int gameSeconds, bool isMapTravel, bool isResting)
	{
		if (this.OnTimeJump != null)
		{
			this.OnTimeJump(gameSeconds, isMapTravel, isResting);
		}
	}
}
