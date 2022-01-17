using UnityEngine;

public class UIFormationsGrid : MonoBehaviour
{
	public int Width = 6;

	public int Height = 6;

	private const float m_FormationScale = 1.5f;

	public GameObject RootObject;

	private UIFormationsSlot[][] m_Slots;

	private float GridMinX => (float)Width * 1.5f / 2f;

	private float GridMinY => (float)Height * 1.5f;

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void LoadFormation(int index)
	{
		Vector3[] formation = FormationData.Instance.GetFormation(index);
		Init();
		for (int i = 0; i < m_Slots.Length; i++)
		{
			for (int j = 0; j < m_Slots[i].Length; j++)
			{
				m_Slots[i][j].SetPartyMember(-1);
			}
		}
		Vector3[] array = new Vector3[formation.Length];
		formation.CopyTo(array, 0);
		for (int k = 0; k < formation.Length; k++)
		{
			array[k].x += GridMinX;
			array[k].z = 0f - array[k].z;
			int value = Mathf.FloorToInt(array[k].x / 1.5f);
			int value2 = Mathf.FloorToInt(array[k].z / 1.5f);
			value2 = Mathf.Clamp(value2, 0, m_Slots.Length - 1);
			value = Mathf.Clamp(value, 0, m_Slots[value2].Length - 1);
			while (m_Slots[value2][value].CurrentPartyMember >= 0)
			{
				value++;
				if (value >= Width)
				{
					value = 0;
					value2++;
					if (value2 >= Height)
					{
						value2 = 0;
					}
				}
			}
			m_Slots[value2][value].SetPartyMember(k);
		}
	}

	public void SetFormation(int index)
	{
		Vector3[] formation = FormationData.Instance.GetFormation(index);
		Vector3[] array = new Vector3[formation.Length];
		for (int i = 0; i < m_Slots.Length; i++)
		{
			for (int j = 0; j < m_Slots[i].Length; j++)
			{
				if (m_Slots[i][j].CurrentPartyMember >= 0)
				{
					array[m_Slots[i][j].CurrentPartyMember] = new Vector3(((float)j + 0.5f) * 1.5f - GridMinX, 0f, (float)(-i) * 1.5f);
				}
			}
		}
		array.CopyTo(formation, 0);
	}

	private void Init()
	{
		if (m_Slots != null)
		{
			return;
		}
		RootObject.SetActive(value: false);
		m_Slots = new UIFormationsSlot[Height][];
		for (int i = 0; i < Height; i++)
		{
			m_Slots[i] = new UIFormationsSlot[Width];
			for (int j = 0; j < Width; j++)
			{
				GameObject obj = NGUITools.AddChild(RootObject.transform.parent.gameObject, RootObject);
				UIFormationsSlot uIFormationsSlot = obj.GetComponentsInChildren<UIFormationsSlot>(includeInactive: true)[0];
				obj.SetActive(value: true);
				m_Slots[i][j] = uIFormationsSlot;
			}
		}
		GetComponent<UIGrid>().Reposition();
	}
}
