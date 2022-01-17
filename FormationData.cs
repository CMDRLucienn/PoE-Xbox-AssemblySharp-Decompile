using UnityEngine;

public class FormationData : MonoBehaviour
{
	[Persistent]
	private Vector3[][] m_formations = new Vector3[5][]
	{
		new Vector3[24]
		{
			new Vector3(-1f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(-1f, 0f, -1.5f),
			new Vector3(1f, 0f, -1.5f),
			new Vector3(-1f, 0f, -3f),
			new Vector3(1f, 0f, -3f),
			new Vector3(-1f, 0f, -4.5f),
			new Vector3(1f, 0f, -4.5f),
			new Vector3(-1f, 0f, -6f),
			new Vector3(1f, 0f, -6f),
			new Vector3(-1f, 0f, -7.5f),
			new Vector3(1f, 0f, -7.5f),
			new Vector3(-1f, 0f, -9f),
			new Vector3(1f, 0f, -9f),
			new Vector3(-1f, 0f, -10.5f),
			new Vector3(1f, 0f, -10.5f),
			new Vector3(-1f, 0f, -13f),
			new Vector3(1f, 0f, -13f),
			new Vector3(-1f, 0f, -14.5f),
			new Vector3(1f, 0f, -14.5f),
			new Vector3(-1f, 0f, -16f),
			new Vector3(1f, 0f, -16f),
			new Vector3(-1f, 0f, -17.5f),
			new Vector3(1f, 0f, -17.5f)
		},
		new Vector3[24]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1.5f, 0f, -1.5f),
			new Vector3(-1.5f, 0f, -1.5f),
			new Vector3(3f, 0f, -3f),
			new Vector3(-3f, 0f, -3f),
			new Vector3(0f, 0f, -3f),
			new Vector3(3f, 0f, -4.5f),
			new Vector3(-3f, 0f, -4.5f),
			new Vector3(0f, 0f, -4.5f),
			new Vector3(3f, 0f, -6f),
			new Vector3(-3f, 0f, -6f),
			new Vector3(0f, 0f, -6f),
			new Vector3(3f, 0f, -7.5f),
			new Vector3(-3f, 0f, -7.5f),
			new Vector3(0f, 0f, -7.5f),
			new Vector3(3f, 0f, -9f),
			new Vector3(-3f, 0f, -9f),
			new Vector3(0f, 0f, -9f),
			new Vector3(3f, 0f, -10.5f),
			new Vector3(-3f, 0f, -10.5f),
			new Vector3(0f, 0f, -10.5f),
			new Vector3(3f, 0f, -12f),
			new Vector3(-3f, 0f, -12f),
			new Vector3(0f, 0f, -12f)
		},
		new Vector3[24]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, -1.5f),
			new Vector3(0f, 0f, -3f),
			new Vector3(0f, 0f, -4.5f),
			new Vector3(0f, 0f, -6f),
			new Vector3(0f, 0f, -7.5f),
			new Vector3(0f, 0f, -9f),
			new Vector3(0f, 0f, -10.5f),
			new Vector3(0f, 0f, -12f),
			new Vector3(0f, 0f, -13.5f),
			new Vector3(0f, 0f, -15f),
			new Vector3(0f, 0f, -16.5f),
			new Vector3(0f, 0f, -18f),
			new Vector3(0f, 0f, -19.5f),
			new Vector3(0f, 0f, -21f),
			new Vector3(0f, 0f, -22.5f),
			new Vector3(0f, 0f, -24f),
			new Vector3(0f, 0f, -26.5f),
			new Vector3(0f, 0f, -27f),
			new Vector3(0f, 0f, -28.5f),
			new Vector3(0f, 0f, -30f),
			new Vector3(0f, 0f, -31.5f),
			new Vector3(0f, 0f, -33f),
			new Vector3(0f, 0f, -34.5f)
		},
		new Vector3[24]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, -1.5f),
			new Vector3(0f, 0f, -3f),
			new Vector3(0f, 0f, -4.5f),
			new Vector3(0f, 0f, -6f),
			new Vector3(0f, 0f, -7.5f),
			new Vector3(0f, 0f, -9f),
			new Vector3(0f, 0f, -10.5f),
			new Vector3(0f, 0f, -12f),
			new Vector3(0f, 0f, -13.5f),
			new Vector3(0f, 0f, -15f),
			new Vector3(0f, 0f, -16.5f),
			new Vector3(0f, 0f, -18f),
			new Vector3(0f, 0f, -19.5f),
			new Vector3(0f, 0f, -21f),
			new Vector3(0f, 0f, -22.5f),
			new Vector3(0f, 0f, -24f),
			new Vector3(0f, 0f, -26.5f),
			new Vector3(0f, 0f, -27f),
			new Vector3(0f, 0f, -28.5f),
			new Vector3(0f, 0f, -30f),
			new Vector3(0f, 0f, -31.5f),
			new Vector3(0f, 0f, -33f),
			new Vector3(0f, 0f, -34.5f)
		},
		new Vector3[24]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, -1.5f),
			new Vector3(0f, 0f, -3f),
			new Vector3(0f, 0f, -4.5f),
			new Vector3(0f, 0f, -6f),
			new Vector3(0f, 0f, -7.5f),
			new Vector3(0f, 0f, -9f),
			new Vector3(0f, 0f, -10.5f),
			new Vector3(0f, 0f, -12f),
			new Vector3(0f, 0f, -13.5f),
			new Vector3(0f, 0f, -15f),
			new Vector3(0f, 0f, -16.5f),
			new Vector3(0f, 0f, -18f),
			new Vector3(0f, 0f, -19.5f),
			new Vector3(0f, 0f, -21f),
			new Vector3(0f, 0f, -22.5f),
			new Vector3(0f, 0f, -24f),
			new Vector3(0f, 0f, -26.5f),
			new Vector3(0f, 0f, -27f),
			new Vector3(0f, 0f, -28.5f),
			new Vector3(0f, 0f, -30f),
			new Vector3(0f, 0f, -31.5f),
			new Vector3(0f, 0f, -33f),
			new Vector3(0f, 0f, -34.5f)
		}
	};

	public static FormationData Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'FormationData' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public Vector3[] GetFormation(int index)
	{
		return m_formations[index];
	}
}
