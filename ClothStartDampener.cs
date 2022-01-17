using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Cloth))]
public class ClothStartDampener : MonoBehaviour
{
	private Cloth m_Cloth;

	private void Awake()
	{
		m_Cloth = GetComponent<Cloth>();
	}

	private void OnEnable()
	{
		StartCoroutine(DampenClothCoroutine());
	}

	private IEnumerator DampenClothCoroutine()
	{
		float damp = m_Cloth.damping;
		m_Cloth.damping = 1f;
		for (int i = 0; i < 10; i++)
		{
			m_Cloth.ClearTransformMotion();
			yield return null;
		}
		m_Cloth.damping = damp;
	}
}
