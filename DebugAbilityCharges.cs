using UnityEngine;

public class DebugAbilityCharges : MonoBehaviour
{
	private GenericAbility m_Ability;

	private int m_AbilityLastKnownCharges;

	private void Start()
	{
		m_Ability = GetComponent<GenericAbility>();
		if ((bool)m_Ability)
		{
			m_AbilityLastKnownCharges = m_Ability.UsesLeft();
		}
	}

	private void Update()
	{
		if (!m_Ability)
		{
			return;
		}
		int num = m_Ability.UsesLeft();
		if (num != m_AbilityLastKnownCharges)
		{
			if (num > m_AbilityLastKnownCharges)
			{
				UIDebug.Instance.LogOnScreenWarning("Ability '" + m_Ability.name + "' charges went up!", UIDebug.Department.Programming, 5f);
			}
			m_AbilityLastKnownCharges = num;
		}
	}
}
