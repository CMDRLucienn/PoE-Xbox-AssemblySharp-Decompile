using System;
using UnityEngine;

public class UICharacterCreationAbilityIcon : MonoBehaviour
{
	public GameObject Collider;

	private GenericAbility m_Ability;

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider);
		uIEventListener.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnChildRightClick(GameObject sender)
	{
		if ((bool)m_Ability)
		{
			UIItemInspectManager.Examine(m_Ability, UICharacterCreationManager.Instance.TargetCharacter);
		}
	}

	public void Set(GenericAbility ability)
	{
		m_Ability = ability;
	}
}
