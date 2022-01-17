using UnityEngine;

public abstract class UIParentSelectorListener : MonoBehaviour
{
	protected ISelectACharacter ParentSelector;

	protected virtual void Start()
	{
		FindParent();
	}

	protected void FindParent()
	{
		if (ParentSelector == null)
		{
			ParentSelector = UIWindowManager.FindParentISelectACharacter(base.transform);
			if (ParentSelector != null)
			{
				ParentSelector.OnSelectedCharacterChanged += NotifySelectionChanged;
				NotifySelectionChanged(ParentSelector.SelectedCharacter);
			}
			else
			{
				Debug.LogError("UIParentSelectorListener '" + base.name + "' does not have an ISelectACharacter in its parents.");
				NotifySelectionChanged(null);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (ParentSelector != null)
		{
			ParentSelector.OnSelectedCharacterChanged -= NotifySelectionChanged;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public virtual void NotifySelectionChanged(CharacterStats selection)
	{
	}
}
