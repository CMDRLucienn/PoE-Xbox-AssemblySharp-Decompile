using UnityEngine;

[RequireComponent(typeof(UITable))]
public class UIReverseTableByContext : MonoBehaviour
{
	public UILikeContext Context;

	private UITable m_Table;

	private void Start()
	{
		m_Table = GetComponent<UITable>();
		if ((bool)Context)
		{
			Context.OnFlipped += OnFlipped;
		}
	}

	private void OnFlipped(bool dir)
	{
		m_Table.reverseSort = dir;
	}
}
