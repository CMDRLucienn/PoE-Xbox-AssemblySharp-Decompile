using System;
using UnityEngine;

public class PairedAbility : MonoBehaviour
{
	public string Tag;

	private PairedAbility m_OtherPair;

	public PairedAbility OtherPair
	{
		get
		{
			return m_OtherPair;
		}
		private set
		{
			m_OtherPair = value;
			OtherAbility = (value ? value.GetComponent<GenericAbility>() : null);
		}
	}

	public GenericAbility OtherAbility { get; private set; }

	private void OnEnable()
	{
		LookForPair();
	}

	public void LookForPair()
	{
		OtherPair = null;
		PairedAbility[] array = UnityEngine.Object.FindObjectsOfType<PairedAbility>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != this && string.Compare(array[i].Tag, Tag, StringComparison.OrdinalIgnoreCase) == 0)
			{
				OtherPair = array[i];
				OtherPair.OtherPair = this;
				break;
			}
		}
	}
}
