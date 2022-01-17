using System;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterCreationAbilityGetter : UICharacterCreationElement
{
	private class AbilityObject
	{
		public GameObject Ability;

		public GameObject AbilityUIObject;
	}

	public GameObject BaseObject;

	public UICharacterCreationEnumSetter.EnumType Enum;

	private List<AbilityObject> m_TempChildren = new List<AbilityObject>();

	public int ScaleAfterNumItems;

	public float ScaleValue;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnAbilityTooltip(GameObject sender, bool over)
	{
		if (over)
		{
			foreach (AbilityObject tempChild in m_TempChildren)
			{
				if (tempChild.AbilityUIObject.GetComponentInChildren<UITexture>().gameObject == sender)
				{
					UIAbilityTooltip.GlobalShow(sender.GetComponentInChildren<UITexture>(), UIWidget.Pivot.BottomLeft, UICharacterCreationManager.Instance.TargetCharacter, AbilityProgressionTable.GetToolTipContent(tempChild.Ability));
				}
			}
			return;
		}
		UIAbilityTooltip.GlobalHide();
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (type != ValueType.Ability && type != ValueType.Talent && type != ValueType.Class && type != ValueType.All)
		{
			return;
		}
		int num = 0;
		GameObject gameObject = null;
		BaseObject.SetActive(value: false);
		BaseObject.transform.localPosition = Vector3.zero;
		foreach (AbilityObject tempChild in m_TempChildren)
		{
			tempChild.AbilityUIObject.SetActive(value: false);
			GameUtilities.Destroy(tempChild.AbilityUIObject);
		}
		m_TempChildren.Clear();
		List<GameObject> list = null;
		if (Enum == UICharacterCreationEnumSetter.EnumType.ABILITY)
		{
			list = UICharacterCreationManager.Instance.GetSelectedAndGrantedAbilities();
		}
		else if (Enum == UICharacterCreationEnumSetter.EnumType.TALENTS)
		{
			list = UICharacterCreationManager.Instance.GetSelectedTalents();
		}
		else if (Enum == UICharacterCreationEnumSetter.EnumType.ABILITY_MASTERY)
		{
			list = UICharacterCreationManager.Instance.GetMasteredAbilities();
		}
		List<GameObject> masteredAbilities = UICharacterCreationManager.Instance.GetMasteredAbilities();
		foreach (GameObject item in list)
		{
			GenericAbility genericAbility = AbilityProgressionTable.GetGenericAbility(item);
			if (genericAbility != null && genericAbility.HideFromUi)
			{
				continue;
			}
			if (!gameObject)
			{
				gameObject = UnityEngine.Object.Instantiate(BaseObject);
				gameObject.transform.parent = BaseObject.transform.parent;
				gameObject.transform.localScale = BaseObject.transform.localScale;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				UICharacterCreationAbilityIcon component = gameObject.GetComponent<UICharacterCreationAbilityIcon>();
				if ((bool)component)
				{
					component.Set(genericAbility);
				}
				AbilityObject abilityObject = new AbilityObject();
				abilityObject.Ability = item;
				abilityObject.AbilityUIObject = gameObject;
				m_TempChildren.Add(abilityObject);
			}
			gameObject.SetActive(value: true);
			num++;
			UITexture componentInChildren = gameObject.GetComponentInChildren<UITexture>();
			if ((bool)componentInChildren)
			{
				UIEventListener uIEventListener = UIEventListener.Get(componentInChildren.gameObject);
				uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnAbilityTooltip));
				Shader shader = Shader.Find("Unlit/Transparent Colored");
				if (shader != null)
				{
					componentInChildren.material = new Material(shader);
				}
				componentInChildren.color = Color.white;
				componentInChildren.material.mainTexture = AbilityProgressionTable.GetAbilityIcon(item);
			}
			if ((bool)genericAbility)
			{
				Transform transform = gameObject.transform.Find("MasteryTexture");
				if ((bool)transform)
				{
					if (masteredAbilities.Contains(genericAbility.gameObject))
					{
						transform.gameObject.SetActive(value: true);
					}
					else
					{
						transform.gameObject.SetActive(value: false);
					}
				}
			}
			gameObject = null;
		}
		UIGrid component2 = GetComponent<UIGrid>();
		if ((bool)component2)
		{
			component2.Reposition();
		}
		if (ScaleAfterNumItems > 0)
		{
			if (num > ScaleAfterNumItems)
			{
				float num2 = 1f - (float)(num - ScaleAfterNumItems) * ScaleValue;
				component2.transform.localScale = new Vector3(num2, num2, 1f);
			}
			else
			{
				component2.transform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
	}
}
