using UnityEngine;

[AddComponentMenu("Toolbox/AutoLootContainer")]
public class AutoLootContainer : OCL
{
	public bool FadeOnLoot = true;

	public RegeneratingItem[] ItemList;

	public override bool IsUsable
	{
		get
		{
			if (!GameState.InCombat)
			{
				return base.IsUsable;
			}
			return false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (GameCursor.GenericUnderCursor == base.gameObject && GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
		{
			HandleMouseUp();
		}
	}

	public override bool Open(GameObject user, bool ignoreLock)
	{
		if (!base.Open(user, ignoreLock))
		{
			return false;
		}
		PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
		int num = 0;
		if (inventory != null && ItemList != null)
		{
			RegeneratingItem[] itemList = ItemList;
			foreach (RegeneratingItem regeneratingItem in itemList)
			{
				if (OEIRandom.FloatValue() < regeneratingItem.Chance)
				{
					int num2 = OEIRandom.Range(regeneratingItem.stackMin, regeneratingItem.stackMax);
					UILootTooltipManager.Instance.ShowTip(regeneratingItem.baseItem, base.gameObject, num2, num++);
					inventory.AddItemAndLog(regeneratingItem.baseItem, num2, user);
				}
			}
		}
		Persistence component = GetComponent<Persistence>();
		if ((bool)component)
		{
			component.SetForDestroy();
		}
		if (FadeOnLoot)
		{
			base.gameObject.AddComponent<AlphaControl>().FadeOut(0.5f);
		}
		GameUtilities.Destroy(base.gameObject, 0.6f);
		return true;
	}

	private void HandleMouseUp()
	{
		GameInput.HandleAllClicks();
		GameState.s_playerCharacter.ObjectClicked(this);
	}

	private void OnMouseOver()
	{
		if (!GetComponent<PE_Collider2D>() && FogOfWar.PointVisibleInFog(base.transform.position))
		{
			GameCursor.GenericUnderCursor = base.gameObject;
		}
	}

	private void OnMouseExit()
	{
		if (!GetComponent<PE_Collider2D>() && GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(base.transform.position, UsableRadius);
	}
}
