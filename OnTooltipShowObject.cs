using UnityEngine;

public class OnTooltipShowObject : MonoBehaviour
{
	public GameObject[] Objects;

	private void OnTooltip()
	{
		GameObject[] objects = Objects;
		foreach (GameObject gameObject in objects)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
				UIWidgetUtils.UpdateDependents(gameObject, 1);
			}
		}
	}

	private void OnHover(bool over)
	{
		if (over)
		{
			return;
		}
		GameObject[] objects = Objects;
		foreach (GameObject gameObject in objects)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: false);
			}
		}
	}
}
