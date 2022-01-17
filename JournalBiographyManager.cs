using UnityEngine;

public class JournalBiographyManager : MonoBehaviour
{
	public JournalBiographyData BiographyData;

	public JournalStoryData PastStoryData;

	public JournalStoryData PresentStoryData;

	[Persistent]
	private bool[] PastStoryUnlocked;

	[Persistent]
	private int[] PresentStoryUnlockedTime;

	public static JournalBiographyManager Instance { get; private set; }

	public int[] PresentStoryUnlockedTimes => PresentStoryUnlockedTime;

	public bool IsPastUnlocked(int key)
	{
		if (PastStoryUnlocked == null || key >= PastStoryUnlocked.Length)
		{
			return false;
		}
		return PastStoryUnlocked[key];
	}

	public bool IsPresentUnlocked(int key)
	{
		if (PresentStoryUnlockedTimes == null || key >= PresentStoryUnlockedTime.Length)
		{
			return false;
		}
		return PresentStoryUnlockedTime[key] > 0;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'JournalBiographyManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
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

	public void UnlockPastStory(string keynm)
	{
		Allocate();
		int num = int.MaxValue;
		for (int i = 0; i < PastStoryData.Items.Length; i++)
		{
			if (PastStoryData.Items[i].Key.ToLower().Equals(keynm.ToLower()))
			{
				num = i;
				break;
			}
		}
		if (num < PastStoryUnlocked.Length)
		{
			PastStoryUnlocked[num] = true;
		}
		else
		{
			Debug.LogError("Tried to unlock Past Story item of key '" + keynm + "' but that key doesn't exist.");
		}
	}

	public void UnlockPresentStory(string keynm)
	{
		Allocate();
		int num = int.MaxValue;
		for (int i = 0; i < PresentStoryData.Items.Length; i++)
		{
			if (PresentStoryData.Items[i].Key.ToLower().Equals(keynm.ToLower()))
			{
				num = i;
				break;
			}
		}
		if (num < PresentStoryUnlockedTime.Length)
		{
			PresentStoryUnlockedTime[num] = WorldTime.Instance.CurrentTime.TotalSeconds;
		}
		else
		{
			Debug.LogError("Tried to unlock Present Story item of key '" + keynm + "' but that key doesn't exist.");
		}
	}

	private void Allocate()
	{
		if (PresentStoryUnlockedTime == null)
		{
			PresentStoryUnlockedTime = new int[PresentStoryData.Items.Length];
		}
		else if (PresentStoryUnlockedTime.Length < PresentStoryData.Items.Length)
		{
			int[] array = new int[PresentStoryData.Items.Length];
			PresentStoryUnlockedTime.CopyTo(array, 0);
			PresentStoryUnlockedTime = array;
		}
		if (PastStoryUnlocked == null)
		{
			PastStoryUnlocked = new bool[PastStoryData.Items.Length];
		}
		else if (PastStoryUnlocked.Length < PastStoryData.Items.Length)
		{
			bool[] array2 = new bool[PastStoryData.Items.Length];
			PastStoryUnlocked.CopyTo(array2, 0);
			PastStoryUnlocked = array2;
		}
	}

	public void UnlockAll()
	{
		Allocate();
		for (int i = 0; i < PastStoryUnlocked.Length; i++)
		{
			PastStoryUnlocked[i] = true;
		}
		for (int j = 0; j < PresentStoryUnlockedTime.Length; j++)
		{
			if (PresentStoryUnlockedTime[j] <= 0)
			{
				PresentStoryUnlockedTime[j] = WorldTime.Instance.CurrentTime.TotalSeconds;
			}
		}
	}
}
