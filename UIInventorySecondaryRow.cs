using UnityEngine;

public class UIInventorySecondaryRow : MonoBehaviour
{
	private bool m_ShowStash;

	public UISprite Background;

	public UIInventoryItemGrid ItemGrid;

	public bool Visible => m_ShowStash;

	public float Height
	{
		get
		{
			if (!Visible)
			{
				return 0f;
			}
			return Background.transform.localScale.y;
		}
	}

	public void AllocateGrid()
	{
		ItemGrid.Allocate();
		UIWindowManager.IncreaseSpriteDepthRecursive(base.gameObject, 10);
	}

	public void Hide()
	{
		m_ShowStash = false;
		Reload();
	}

	public void ToggleStash()
	{
		if (m_ShowStash)
		{
			Hide();
		}
		else
		{
			ShowStash();
		}
	}

	public void ShowStash()
	{
		m_ShowStash = true;
		Reload();
	}

	public void Reload()
	{
		ItemGrid.IsExternalContainer = false;
		ItemGrid.AllowOrder = true;
		if (m_ShowStash && (bool)GameState.s_playerCharacter)
		{
			ItemGrid.AllowOrder = false;
			ItemGrid.LoadInventory(GameState.s_playerCharacter.GetComponent<StashInventory>());
		}
		if (Visible)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
