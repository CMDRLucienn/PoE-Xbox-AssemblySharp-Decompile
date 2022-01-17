using UnityEngine;

public class UIAlignHalfPixels : MonoBehaviour
{
	public bool AlignOnUpdate;

	public bool WholePixel;

	private Vector3 m_LastPosition;

	private void Start()
	{
		Align(base.transform, WholePixel);
		m_LastPosition = base.transform.position;
	}

	private void Update()
	{
		if (AlignOnUpdate && m_LastPosition != base.transform.position)
		{
			m_LastPosition = base.transform.position;
			Align(base.transform, WholePixel);
		}
	}

	public static void Align(Transform t)
	{
		Align(t, whole: false);
	}

	public static void Align(Transform transform, bool whole)
	{
		if (Application.isPlaying)
		{
			float num = 0f;
			float num2 = 0f;
			Transform parent = transform.parent;
			while (parent != null)
			{
				num += parent.localPosition.x - Mathf.Floor(parent.localPosition.x);
				num2 += parent.localPosition.y - Mathf.Floor(parent.localPosition.y);
				parent = parent.parent;
			}
			num -= Mathf.Floor(num);
			num2 -= Mathf.Floor(num2);
			transform.localPosition = new Vector3(Mathf.Floor(transform.localPosition.x) + (whole ? 0.5f : 0f) - num, Mathf.Floor(transform.localPosition.y) + (whole ? 0.5f : 0f) - num2, transform.localPosition.z);
		}
	}
}
