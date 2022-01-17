using System;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
	public Team PlayerTeamPrefab;

	private Reputation[] FactionListInternal;

	public Reputation[] FactionList = new Reputation[2];

	[Persistent]
	public Disposition PlayerDisposition = new Disposition();

	public TitleStringSet DefaultTitles = new TitleStringSet();

	[Persistent]
	[HideInInspector]
	public Reputation[] Factions
	{
		get
		{
			return FactionListInternal;
		}
		set
		{
			if (value.Length < FactionList.Length)
			{
				int num = value.Length;
				Array.Resize(ref value, FactionList.Length);
				for (int i = num; i < FactionList.Length; i++)
				{
					value[i] = FactionList[i].Clone();
				}
			}
			FactionListInternal = value;
		}
	}

	public static ReputationManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'ReputationManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
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

	private void Start()
	{
		if (FactionListInternal == null)
		{
			FactionListInternal = new Reputation[FactionList.Length];
			for (int i = 0; i < FactionList.Length; i++)
			{
				FactionListInternal[i] = FactionList[i].Clone();
			}
		}
		PlayerDisposition.Start();
		for (int j = 0; j < Factions.Length; j++)
		{
			Factions[j].OnStart();
		}
	}

	public void Restored()
	{
		for (int i = 0; i < Factions.Length; i++)
		{
			Factions[i].OnStart();
		}
	}

	public Reputation GetReputation(FactionName id)
	{
		for (int i = 0; i < Factions.Length; i++)
		{
			if (Factions[i].Equals(id))
			{
				return Factions[i];
			}
		}
		return null;
	}

	public bool AddReputation(GameObject target, Reputation.Axis axis, Reputation.ChangeStrength strength)
	{
		Faction component = target.GetComponent<Faction>();
		if (component != null)
		{
			return AddReputation(component.Reputation, axis, strength);
		}
		return false;
	}

	public bool AddReputation(FactionName id, Reputation.Axis axis, Reputation.ChangeStrength strength)
	{
		AddReputation(GetReputation(id), axis, strength);
		return true;
	}

	public bool AddReputation(Reputation rep, Reputation.Axis axis, Reputation.ChangeStrength strength)
	{
		if (rep == null)
		{
			return false;
		}
		rep.AddReputation(axis, strength);
		return true;
	}

	public string GetFactionName(FactionName id)
	{
		for (int i = 0; i < Factions.Length; i++)
		{
			if (Factions[i].Equals(id))
			{
				return Factions[i].Name.GetText();
			}
		}
		return "";
	}
}
