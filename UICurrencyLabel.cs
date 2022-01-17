using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UICurrencyLabel : MonoBehaviour
{
	private UILabel m_Label;

	private CurrencyValue m_TrackValue;

	private float m_LastValue = -1f;

	private void Start()
	{
		m_Label = GetComponent<UILabel>();
	}

	private void Update()
	{
		float num = ((m_TrackValue == null) ? GameState.s_playerCharacter.Inventory.currencyTotalValue.v : m_TrackValue.v);
		if (num != m_LastValue)
		{
			m_Label.text = num.ToString();
			m_LastValue = num;
		}
	}

	public void SetPlayer()
	{
		m_TrackValue = null;
	}

	public void SetStore(Store store)
	{
		m_TrackValue = store.currencyStoreBank;
	}
}
