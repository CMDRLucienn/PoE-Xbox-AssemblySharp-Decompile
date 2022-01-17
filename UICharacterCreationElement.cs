using UnityEngine;

public class UICharacterCreationElement : MonoBehaviour
{
	public enum ValueType
	{
		Gender,
		Class,
		Race,
		Subrace,
		Culture,
		Background,
		BodyType,
		Voice,
		Deity,
		Religion,
		Ability,
		Talent,
		Attribute,
		Color,
		BodyPart,
		Portrait,
		Name,
		Skill,
		All,
		Count
	}

	private UICharacterCreationController m_Owner;

	protected UICharacterCreationController Owner
	{
		get
		{
			if (!m_Owner)
			{
				Transform parent = base.transform;
				while (parent.parent != null && !(m_Owner = parent.GetComponent<UICharacterCreationController>()))
				{
					parent = parent.parent;
				}
			}
			return m_Owner;
		}
	}

	protected virtual void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected virtual void Start()
	{
	}

	public virtual void SignalValueChanged(ValueType type)
	{
	}
}
