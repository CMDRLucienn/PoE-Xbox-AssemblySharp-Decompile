using UnityEngine;

[RequireComponent(typeof(UIStretch))]
[ExecuteInEditMode]
public class UIStretchRelSize : MonoBehaviour
{
	public UIStretch Stretch;

	public GameObject Source;

	private void Start()
	{
		if (!Stretch)
		{
			Stretch = GetComponent<UIStretch>();
		}
	}

	private void Update()
	{
		if ((bool)Source)
		{
			Stretch.relativeSize = Source.transform.localScale;
		}
	}
}
