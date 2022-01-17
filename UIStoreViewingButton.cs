using UnityEngine;

public class UIStoreViewingButton : MonoBehaviour
{
	public PartyInventoryType Action;

	public UIWidget Highlight;

	private UIMultiSpriteImageButton m_ImageButton;

	private void Awake()
	{
		m_ImageButton = GetComponent<UIMultiSpriteImageButton>();
		UIStoreManager.Instance.OnPageChanged += OnPageChanged;
		UIStoreManager.Instance.StorePage.OnViewingChanged += OnViewingChanged;
		OnViewingChanged(UIStoreManager.Instance.StorePage.ViewingPage);
	}

	private void OnPageChanged(UIStorePageType page)
	{
		base.gameObject.SetActive(page == UIStorePageType.Store);
	}

	private void OnClick()
	{
		UIStoreManager.Instance.StorePage.ChangePlayerViewing(Action);
	}

	private void OnViewingChanged(PartyInventoryType tab)
	{
		if ((bool)m_ImageButton)
		{
			m_ImageButton.ForceDown(tab == Action);
		}
		if ((bool)Highlight)
		{
			Highlight.alpha = ((tab == Action) ? 1f : 0f);
		}
	}
}
