using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIStatusEffectIcon : MonoBehaviour, ITooltipContent
{
	[HideInInspector]
	public OEIBindingList<StatusEffect> StatusEffects = new OEIBindingList<StatusEffect>();

	[HideInInspector]
	public List<StatusEffect> EffectsFromAuras = new List<StatusEffect>();

	[HideInInspector]
	public List<StatusEffect> CleanedUpStatusEffects = new List<StatusEffect>();

	private UIStatusEffectStrip m_Owner;

	public GenericAbility AbilityOrigin
	{
		get
		{
			if (StatusEffects.Count > 0)
			{
				return StatusEffects[0].AbilityOrigin;
			}
			return null;
		}
	}

	public UITexture Texture { get; private set; }

	public bool Empty
	{
		get
		{
			for (int i = 0; i < StatusEffects.Count; i++)
			{
				if (!StatusEffects[i].HideBecauseUntriggered)
				{
					return false;
				}
			}
			return true;
		}
	}

	private void Start()
	{
		TryInit();
	}

	private void OnEffectListChanged(object sender, ListChangedEventArgs e)
	{
		CleanedUpStatusEffects.Clear();
		CleanedUpStatusEffects.AddRange(StatusEffects);
		StatusEffectParams.CleanUp(CleanedUpStatusEffects);
	}

	private void TryInit()
	{
		if (!(Texture != null))
		{
			Texture = GetComponent<UITexture>();
			StatusEffects.ListChanged += OnEffectListChanged;
			OnEffectListChanged(StatusEffects, null);
		}
	}

	public void AddEffect(StatusEffect effect, bool isFromAura)
	{
		TryInit();
		if (!StatusEffects.Contains(effect))
		{
			StatusEffects.Add(effect);
			if (isFromAura)
			{
				EffectsFromAuras.Add(effect);
			}
			Texture.mainTexture = effect.GetDisplayIcon();
			if (Texture.mainTexture == null)
			{
				Texture.mainTexture = m_Owner.DefaultIcon;
			}
		}
	}

	public void SetOwner(UIStatusEffectStrip owner)
	{
		m_Owner = owner;
	}

	public bool RemoveEffect(StatusEffect effect)
	{
		return StatusEffects.Remove(effect);
	}

	public bool CanBundle(StatusEffect neweffect)
	{
		if (StatusEffects.Count <= 0)
		{
			return false;
		}
		return StatusEffects[0].BundlesWith(neweffect);
	}

	public string GetTooltipContent(GameObject owner)
	{
		if (CleanedUpStatusEffects.Count <= 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		int num = 0;
		string text = CleanedUpStatusEffects[0].HitStrength;
		if (!string.IsNullOrEmpty(text))
		{
			text = GUIUtils.Format(1731, text + " x" + CleanedUpStatusEffects[0].Scale) + "\n";
		}
		List<StatusEffect> list = new List<StatusEffect>();
		list.AddRange(CleanedUpStatusEffects.Where((StatusEffect e) => !e.HideBecauseUntriggered));
		List<List<StatusEffect>> list2 = StatusEffect.FilterAfflictions(list);
		for (int i = 0; i < list2.Count; i++)
		{
			List<StatusEffect> list3 = list2[i];
			if (list3.Count > 0)
			{
				bool flag2 = list3.Any((StatusEffect e) => !EffectsFromAuras.Contains(e) && e.IsAura);
				bool flag3 = list3.All((StatusEffect e) => e.Params.IsHostile);
				bool num2 = list3.All((StatusEffect e) => e.IsSuspended || e.IsSuppressed);
				if (flag2)
				{
					flag = true;
				}
				Color c = Color.white;
				if (num2)
				{
					c = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.DARKDISABLED);
				}
				else if (flag3)
				{
					c = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.ERROR);
				}
				Affliction afflictionOrigin = list3[0].AfflictionOrigin;
				if (num == 1)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("[" + NGUITools.EncodeColor(c) + "]");
				if (!afflictionOrigin.ThinUI)
				{
					stringBuilder.Append(afflictionOrigin.ToString());
					stringBuilder.Append(": ");
				}
				stringBuilder.Append(StatusEffectParams.ListToString(list3, null, null, StatusEffectFormatMode.PartyBar));
				stringBuilder.Append("[-]");
				num = 1;
			}
		}
		bool flag4 = list.All((StatusEffect e) => e.Params.IsHostile);
		bool num3 = list.All((StatusEffect e) => e.IsSuspended || e.IsSuppressed);
		Color c2 = Color.white;
		if (num3)
		{
			c2 = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.DARKDISABLED);
		}
		else if (flag4)
		{
			c2 = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.ERROR);
		}
		stringBuilder.Append("[" + NGUITools.EncodeColor(c2) + "]");
		float num4 = 0f;
		while (list.Count > 0)
		{
			StatusEffect statusEffect = list[0];
			list.RemoveAt(0);
			if (statusEffect.IsAura && !EffectsFromAuras.Contains(statusEffect))
			{
				flag = true;
			}
			bool num5 = statusEffect.IsSuppressed || statusEffect.IsSuspended;
			c2 = Color.white;
			if (num5)
			{
				c2 = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.DARKDISABLED);
			}
			else if (statusEffect.Params.IsHostile)
			{
				c2 = UIGlobalColor.Instance.Get(UIGlobalColor.TextColor.ERROR);
			}
			string @string = statusEffect.Params.GetString(statusEffect, null, showTime: false, list);
			if (!string.IsNullOrEmpty(@string))
			{
				if (num == 2)
				{
					stringBuilder.Append(GUIUtils.Comma());
				}
				if (num == 1)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("[" + NGUITools.EncodeColor(c2) + "]");
				stringBuilder.Append(@string);
				stringBuilder.Append("[-]");
				num = 2;
			}
			if (!num5)
			{
				num4 = Mathf.Max(num4, statusEffect.TimeLeft);
			}
		}
		if (num4 > 0f)
		{
			stringBuilder.AppendGuiFormat(1731, GUIUtils.Format(211, num4.ToString("#0.0")));
		}
		stringBuilder.Append("[-]");
		if (stringBuilder.Length > 0)
		{
			if (flag)
			{
				return GUIUtils.GetText(902, CharacterStats.GetGender(owner)) + "\n" + text + stringBuilder.ToString();
			}
			return text + stringBuilder.ToString();
		}
		return "";
	}

	public string GetTooltipName(GameObject owner)
	{
		if (CleanedUpStatusEffects != null && CleanedUpStatusEffects.Count > 0)
		{
			return CleanedUpStatusEffects[0].GetDisplayName();
		}
		return "";
	}

	public Texture GetTooltipIcon()
	{
		return Texture.mainTexture;
	}
}
