using System;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalToggleManager : MonoBehaviour
{
	public float UpdateFrequency = 1f;

	public List<ConditionalToggle> ActiveList = new List<ConditionalToggle>();

	public List<ConditionalToggle> InactiveList = new List<ConditionalToggle>();

	public List<ConditionalToggle> ScriptInactiveList = new List<ConditionalToggle>();

	private float ActiveTimer;

	private float InactiveTimer = 1f;

	public static ConditionalToggleManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'ConditionalToggleManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		InactiveTimer = UpdateFrequency / 2f;
		GameResources.OnPreloadGame += GameResources_OnPreloadGame;
		GameState.OnLevelLoaded += GameState_OnLevelLoaded;
	}

	private void GameResources_OnPreloadGame()
	{
		ActiveList.Clear();
		InactiveList.Clear();
		ScriptInactiveList.Clear();
	}

	private void GameState_OnLevelLoaded(object sender, EventArgs e)
	{
		if (ScriptInactiveList == null)
		{
			return;
		}
		foreach (ConditionalToggle scriptInactive in ScriptInactiveList)
		{
			if (!(scriptInactive == null))
			{
				Persistence component = scriptInactive.GetComponent<Persistence>();
				if ((bool)component)
				{
					component.Load();
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameResources.OnPreloadGame -= GameResources_OnPreloadGame;
		GameState.OnLevelLoaded -= GameState_OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		ActiveTimer -= Time.deltaTime;
		InactiveTimer -= Time.deltaTime;
		if (InactiveTimer < 0f)
		{
			for (int num = InactiveList.Count - 1; num >= 0; num--)
			{
				if (!(InactiveList[num] == null) && InactiveList[num].Evaluate())
				{
					InactiveList[num].gameObject.SetActive(value: true);
					AddToActiveList(InactiveList[num]);
					InactiveList.RemoveAt(num);
				}
			}
			InactiveTimer = UpdateFrequency;
		}
		if (!(ActiveTimer < 0f))
		{
			return;
		}
		for (int num2 = ActiveList.Count - 1; num2 >= 0; num2--)
		{
			if (!(ActiveList[num2] == null) && !ActiveList[num2].Evaluate())
			{
				ActiveList[num2].gameObject.SetActive(value: false);
				AddToInactiveList(ActiveList[num2]);
				ActiveList.RemoveAt(num2);
			}
		}
		ActiveTimer = UpdateFrequency;
	}

	public void AddToActiveList(ConditionalToggle toggle)
	{
		ActiveList.Add(toggle);
	}

	public void AddToInactiveList(ConditionalToggle toggle)
	{
		InactiveList.Add(toggle);
	}

	public void AddToScriptInactiveList(ConditionalToggle toggle)
	{
		ScriptInactiveList.Add(toggle);
	}

	public void ResetBetweenSceneLoads()
	{
		InactiveList.Clear();
		ScriptInactiveList.Clear();
	}

	public GameObject GetObjectByID(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if ((bool)objectByID)
		{
			return objectByID;
		}
		foreach (ConditionalToggle scriptInactive in ScriptInactiveList)
		{
			if (!(scriptInactive == null))
			{
				InstanceID component = scriptInactive.GetComponent<InstanceID>();
				if ((bool)component && component.Guid == objectGuid)
				{
					return component.gameObject;
				}
			}
		}
		return null;
	}
}
