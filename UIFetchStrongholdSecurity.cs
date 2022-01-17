using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIFetchStrongholdSecurity : MonoBehaviour
{
	private UILabel m_Label;

	private Stronghold m_Stronghold;

	private void Start()
	{
		m_Label = GetComponent<UILabel>();
		m_Stronghold = GameState.Stronghold;
	}

	private void Update()
	{
		if ((bool)m_Label && (bool)m_Stronghold)
		{
			m_Label.text = m_Stronghold.GetSecurity().ToString();
		}
	}
}
