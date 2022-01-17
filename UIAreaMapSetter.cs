using UnityEngine;

public class UIAreaMapSetter : MonoBehaviour
{
	private static UIAreaMapSetter m_instance;

	public static UIAreaMapSetter Instance => m_instance;

	private void Awake()
	{
		m_instance = this;
	}

	private void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
	}

	public void SetName()
	{
		GetComponent<UILabel>().text = GameState.Instance.CurrentMap.DisplayName.GetText();
	}
}
