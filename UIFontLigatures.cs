using System;
using UnityEngine;

public class UIFontLigatures : MonoBehaviour
{
	[Serializable]
	public class Mapping
	{
		public string From;

		public int To;

		public bool Discretionary;

		private string m_ToString;

		public string ToCharAsString
		{
			get
			{
				if (m_ToString == null)
				{
					m_ToString = ((char)To).ToString();
				}
				return m_ToString;
			}
		}
	}

	public Mapping[] Mappings;

	public static bool LigaturesEnabled
	{
		get
		{
			if (GameState.Mode != null && GameState.Mode.Option != null && !GameState.Mode.Option.GetOption(GameOption.BoolOption.LIGATURES))
			{
				return !Application.isPlaying;
			}
			return true;
		}
	}
}
