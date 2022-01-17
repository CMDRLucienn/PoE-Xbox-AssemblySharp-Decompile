using UnityEngine;

public class SelectionCircleMaterials
{
	public class ColorblindSet
	{
		public class SelectedSet
		{
			public class StealthedSet
			{
				private Material m_Standard;

				private Material m_Stealthed;

				public StealthedSet(Material standard)
				{
					m_Standard = standard;
					m_Stealthed = new Material(standard);
					m_Stealthed.SetFloat("_DashWidth", 1f);
					m_Stealthed.SetFloat("_DashOffAlpha", 0.3f);
				}

				public Material Get(bool stealthed)
				{
					if (stealthed)
					{
						return m_Stealthed;
					}
					return m_Standard;
				}
			}

			private StealthedSet m_Standard;

			private StealthedSet m_Selected;

			public SelectedSet(Material standard, Material selected)
			{
				m_Standard = new StealthedSet(standard);
				m_Selected = new StealthedSet(selected);
			}

			public Material Get(bool selected, bool stealthed)
			{
				if (selected)
				{
					return m_Selected.Get(stealthed);
				}
				return m_Standard.Get(stealthed);
			}
		}

		private SelectedSet m_Standard;

		private SelectedSet m_Colorblind;

		public ColorblindSet(Material standard, Material standardColorblind, Material standardSelected, Material standardSelectedColorblind)
		{
			m_Standard = new SelectedSet(standard, standardSelected);
			m_Colorblind = new SelectedSet(standardColorblind, standardSelectedColorblind);
		}

		public Material Get(bool colorblind, bool selected, bool stealthed)
		{
			if (colorblind)
			{
				return m_Colorblind.Get(selected, stealthed);
			}
			return m_Standard.Get(selected, stealthed);
		}
	}

	private ColorblindSet Friendly;

	private ColorblindSet Foe;

	private ColorblindSet FriendlyDominated;

	public SelectionCircleMaterials(Material friendly, Material friendlyColorblind, Material foe, Material foeColorblind, Material friendlySelected, Material friendlySelectedColorblind, Material friendlyDominated, Material friendlyDominatedColorblind)
	{
		Friendly = new ColorblindSet(friendly, friendlyColorblind, friendlySelected, friendlySelectedColorblind);
		Foe = new ColorblindSet(foe, foeColorblind, foe, foeColorblind);
		FriendlyDominated = new ColorblindSet(friendlyDominated, friendlyDominatedColorblind, friendlyDominated, friendlyDominatedColorblind);
	}

	public Material Get(bool friendly, bool colorblind, bool selected, bool stealthed, bool dominated)
	{
		if (friendly)
		{
			return Friendly.Get(colorblind, selected, stealthed);
		}
		if (dominated)
		{
			return FriendlyDominated.Get(colorblind, selected, stealthed);
		}
		return Foe.Get(colorblind, selected, stealthed);
	}
}
