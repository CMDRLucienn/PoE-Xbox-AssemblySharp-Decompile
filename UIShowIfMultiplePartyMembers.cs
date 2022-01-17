using System.Linq;
using UnityEngine;

public class UIShowIfMultiplePartyMembers : MonoBehaviour
{
	private UIWidget m_Widget;

	private void Start()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		if ((bool)m_Widget)
		{
			m_Widget.alpha = ((PartyMemberAI.OnlyPrimaryPartyMembers.Count() > 1) ? 1 : 0);
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}
