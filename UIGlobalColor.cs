using System;
using UnityEngine;

[ExecuteInEditMode]
public class UIGlobalColor : MonoBehaviour
{
	public delegate void ColorChanged();

	[Serializable]
	public class LinkColors
	{
		public Color Buff;

		public Color BuffHighlight;

		public Color BuffColorblind;

		public Color BuffHighlightColorblind;

		public Color Debuff;

		public Color DebuffHighlight;

		public Color Neutral;

		public Color NeutralHighlight;
	}

	public enum TextColor
	{
		BROWN_NEUTRAL,
		BROWN_INACTIVE,
		BROWN_HIGHLIGHTED,
		GREENHIGHLIGHT,
		DARK,
		INTRACTABLE,
		WHITE,
		RED,
		WOOD,
		LIGHTALPHA,
		WOODGREEN,
		DARKDISABLED,
		DULLGREEN,
		WOODDARK,
		SELECTIONFRAME,
		TABSELECTED,
		TABUNSELECTED,
		TRIMC,
		PARCHMENT_DECAL,
		PARCHMENT_DECAL_DARK,
		PARCHMENT_BORDERING,
		ERROR,
		TOOLTIP_BG,
		PARCHMENT_LIGHT,
		LINK
	}

	public enum LinkStyle
	{
		NONE,
		PARCHMENT,
		CONSOLE,
		WOOD,
		NEWS
	}

	public enum LinkType
	{
		NEUTRAL,
		BUFF,
		DEBUFF
	}

	public ColorChanged OnColorChanged;

	public bool DoUpdate;

	public Color BrownNeutral;

	public Color BrownInactive;

	public Color BrownHighlighted;

	public Color GreenHighlight;

	public Color DullGreen;

	public Color Dark;

	public Color DarkDisabled;

	public Color Intractable;

	public Color White;

	public Color Red;

	public Color Error;

	public Color ParchmentEdging;

	public Color Wood;

	public Color LightAlpha;

	public Color WoodGreen;

	public Color WoodDark;

	public Color SelectionFrame;

	public Color TabSelected;

	public Color TabUnselected;

	public Color TrimC;

	public Color ParchmentBordering;

	public Color ParchmentDecal;

	public Color ParchmentDecalDark;

	public Color ParchmentLight;

	public Color TooltipBG;

	public LinkColors ParchmentLinks;

	public LinkColors ConsoleLinks;

	public LinkColors WoodLinks;

	public LinkColors NewsLinks;

	public static UIGlobalColor Instance { get; private set; }

	public Color Get(TextColor color)
	{
		return color switch
		{
			TextColor.BROWN_NEUTRAL => BrownNeutral, 
			TextColor.BROWN_INACTIVE => BrownInactive, 
			TextColor.BROWN_HIGHLIGHTED => BrownHighlighted, 
			TextColor.GREENHIGHLIGHT => GreenHighlight, 
			TextColor.DARK => Dark, 
			TextColor.DARKDISABLED => DarkDisabled, 
			TextColor.INTRACTABLE => Intractable, 
			TextColor.WHITE => White, 
			TextColor.RED => Red, 
			TextColor.WOOD => Wood, 
			TextColor.LIGHTALPHA => LightAlpha, 
			TextColor.WOODGREEN => WoodGreen, 
			TextColor.DULLGREEN => DullGreen, 
			TextColor.WOODDARK => WoodDark, 
			TextColor.SELECTIONFRAME => SelectionFrame, 
			TextColor.TABSELECTED => TabSelected, 
			TextColor.TABUNSELECTED => TabUnselected, 
			TextColor.ERROR => Error, 
			TextColor.TRIMC => TrimC, 
			TextColor.PARCHMENT_DECAL => ParchmentDecal, 
			TextColor.PARCHMENT_DECAL_DARK => ParchmentDecalDark, 
			TextColor.PARCHMENT_BORDERING => ParchmentBordering, 
			TextColor.TOOLTIP_BG => TooltipBG, 
			TextColor.PARCHMENT_LIGHT => ParchmentLight, 
			_ => Color.magenta, 
		};
	}

	public Color Get(TextColor color, LinkStyle linkStyle, bool hovered, LinkType linkType = LinkType.NEUTRAL)
	{
		if (color == TextColor.LINK)
		{
			LinkColors linkColors;
			switch (linkStyle)
			{
			case LinkStyle.PARCHMENT:
				linkColors = ParchmentLinks;
				break;
			case LinkStyle.CONSOLE:
				linkColors = ConsoleLinks;
				break;
			case LinkStyle.WOOD:
				linkColors = WoodLinks;
				break;
			case LinkStyle.NEWS:
				linkColors = NewsLinks;
				break;
			default:
				return Color.magenta;
			}
			switch (linkType)
			{
			case LinkType.NEUTRAL:
				if (!hovered)
				{
					return linkColors.Neutral;
				}
				return linkColors.NeutralHighlight;
			case LinkType.BUFF:
				if (GameState.Option.GetOption(GameOption.BoolOption.COLORBLIND_MODE))
				{
					if (!hovered)
					{
						return linkColors.BuffColorblind;
					}
					return linkColors.BuffHighlightColorblind;
				}
				if (!hovered)
				{
					return linkColors.Buff;
				}
				return linkColors.BuffHighlight;
			case LinkType.DEBUFF:
				if (!hovered)
				{
					return linkColors.Debuff;
				}
				return linkColors.DebuffHighlight;
			default:
				return Color.magenta;
			}
		}
		return Get(color);
	}

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}
}
