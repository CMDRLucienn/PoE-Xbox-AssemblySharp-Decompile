using System.Collections.Generic;
using UnityEngine;

public class UICharacterCreationAbilityLevelGrid : MonoBehaviour
{
	public UIGrid AbilityLevelButtonGrid;

	public GameObject AbilityLevelButton;

	private List<GameObject> AddedChildren;

	private void Awake()
	{
		if (AbilityLevelButtonGrid == null)
		{
			return;
		}
		int num = 8;
		if (AddedChildren == null)
		{
			AddedChildren = new List<GameObject>(num);
		}
		else
		{
			for (int i = 0; i < AddedChildren.Count; i++)
			{
				GameUtilities.Destroy(AddedChildren[i]);
			}
			AddedChildren.Clear();
		}
		for (int j = 0; j < num; j++)
		{
			GameObject gameObject = Object.Instantiate(AbilityLevelButton);
			gameObject.SetActive(value: true);
			gameObject.transform.parent = AbilityLevelButtonGrid.gameObject.transform;
			gameObject.transform.localScale = AbilityLevelButton.gameObject.transform.localScale;
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, AbilityLevelButton.transform.localPosition.z);
			AddedChildren.Add(gameObject);
			UICharacterCreationAbilityLevelButton[] componentsInChildren = gameObject.GetComponentsInChildren<UICharacterCreationAbilityLevelButton>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				componentsInChildren[0].AbilityLevel = j + 1;
			}
			UILabel[] componentsInChildren2 = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
			{
				componentsInChildren2[0].text = RomanNumeral.Convert(j + 1);
			}
		}
		AbilityLevelButtonGrid.Reposition();
	}
}
