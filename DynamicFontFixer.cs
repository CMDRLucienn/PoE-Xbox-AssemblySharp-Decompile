using System;
using UnityEngine;

[ExecuteInEditMode]
public class DynamicFontFixer : MonoBehaviour
{
	[Serializable]
	public class FontMap
	{
		public Font oldFont;

		public UIDynamicFontManager.FontClass newClass;
	}

	public bool Execute;

	public FontMap[] mappings;

	private void Update()
	{
		if (!Execute)
		{
			return;
		}
		Execute = false;
		UIDynamicFontSize[] componentsInChildren = GetComponentsInChildren<UIDynamicFontSize>(includeInactive: true);
		foreach (UIDynamicFontSize uIDynamicFontSize in componentsInChildren)
		{
			UILabel component = uIDynamicFontSize.GetComponent<UILabel>();
			if (!component)
			{
				continue;
			}
			Font dynamicFont = component.font.dynamicFont;
			UIDynamicFontManager.FontClass @class = UIDynamicFontManager.FontClass.ESP_REGULAR;
			int size = (int)component.transform.localScale.y;
			FontMap[] array = mappings;
			foreach (FontMap fontMap in array)
			{
				if (fontMap.oldFont == dynamicFont)
				{
					@class = fontMap.newClass;
					break;
				}
			}
			uIDynamicFontSize.Size = size;
			uIDynamicFontSize.Class = @class;
		}
	}
}
