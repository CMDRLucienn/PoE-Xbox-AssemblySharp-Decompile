using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UIDropdownMenu))]
public class UIDropdownResolution : MonoBehaviour
{
	[Serializable]
	public class MyResolution
	{
		public int width;

		public int height;

		public int refreshRate;

		public MyResolution(Resolution r)
			: this(r.width, r.height, r.refreshRate)
		{
			width = r.width;
			height = r.height;
			refreshRate = r.refreshRate;
		}

		public MyResolution(int width, int height)
		{
			this.width = width;
			this.height = height;
			refreshRate = 60;
		}

		public MyResolution(int width, int height, int refreshRate)
			: this(width, height)
		{
			this.refreshRate = refreshRate;
		}

		public MyResolution(MyResolution other)
			: this(other.width, other.height, other.refreshRate)
		{
		}

		public MyResolution(string str)
		{
			string[] array = str.Split('&');
			if (array.Length >= 1)
			{
				IntUtils.TryParseInvariant(array[0], out width);
			}
			if (array.Length >= 2)
			{
				IntUtils.TryParseInvariant(array[1], out height);
			}
			if (array.Length >= 3)
			{
				IntUtils.TryParseInvariant(array[2], out refreshRate);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is MyResolution myResolution)
			{
				if (myResolution.width == width && myResolution.height == height)
				{
					return myResolution.refreshRate == refreshRate;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return width + height * 13 + refreshRate * 17;
		}

		public override string ToString()
		{
			return width + "x" + height + " @ " + GUIUtils.Format(1420, refreshRate);
		}

		public string SerialString()
		{
			return width.ToStringInvariant() + "&" + height.ToStringInvariant() + "&" + refreshRate.ToStringInvariant();
		}
	}

	private UIDropdownMenu m_Dropdown;

	private void Awake()
	{
		m_Dropdown = GetComponent<UIDropdownMenu>();
		IEnumerable<Resolution> source = Screen.resolutions.Where((Resolution r) => r.width >= 1280 && r.height >= 720 && r.refreshRate > 24);
		m_Dropdown.Options = source.Select((Resolution res) => new MyResolution(res)).Cast<object>().ToArray();
	}
}
