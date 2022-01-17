using System;
using UnityEngine;

public class StrongholdContainer : Container
{
	public enum ContentType
	{
		Ingredients,
		AdventureAwards,
		VisitorItems,
		Everything
	}

	public ContentType Contents;

	public override bool IsEmpty
	{
		get
		{
			Regenerate();
			return base.IsEmpty;
		}
	}

	public override bool Open(GameObject user, bool ignoreLock)
	{
		Regenerate();
		if (!base.Open(user, ignoreLock))
		{
			return false;
		}
		return true;
	}

	public void Regenerate()
	{
		SetSeed();
		if (Contents == ContentType.VisitorItems || Contents == ContentType.Everything)
		{
			GameState.Stronghold.CreateVisitorItems(m_inventory);
		}
		if (Contents == ContentType.AdventureAwards || Contents == ContentType.Everything)
		{
			GameState.Stronghold.CreateAdventureRewards(m_inventory);
		}
		if (Contents == ContentType.Ingredients || Contents == ContentType.Everything)
		{
			GameState.Stronghold.CreateStrongholdSpawnedIngredients(m_inventory);
		}
		ResetSeed();
	}

	private void SetSeed()
	{
		UnityEngine.Random.InitState((int)(base.transform.position.x + base.transform.position.z) * GameState.s_playerCharacter.name.GetHashCode() + (WorldTime.Instance.CurrentHour + WorldTime.Instance.CurrentDay));
	}

	private void ResetSeed()
	{
		UnityEngine.Random.InitState(Environment.TickCount);
	}
}
