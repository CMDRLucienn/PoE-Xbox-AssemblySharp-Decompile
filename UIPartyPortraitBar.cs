using System;
using System.Linq;
using UnityEngine;

public class UIPartyPortraitBar : MonoBehaviour
{
	[Serializable]
	public class ObfusticatorColor
	{
		public Texture2D FlameImage;

		public Color PulseColor1;

		public Color PulseColor2;
	}

	[Tooltip("A 'prefab' object to clone.")]
	public GameObject PartyPortrait;

	[Tooltip("An anchor point on the rightmost portrait.")]
	public UIWidget RightEdge;

	[Tooltip("A VFX prefab for level up notification.")]
	public GameObject LevelUpVfx;

	[Tooltip("A VFX prefab for cipher focus.")]
	public GameObject CipherFocusVfx;

	public UIWidget SolidBackground;

	public float Spacing = 8f;

	public float PortraitWidth = 101f;

	public int PartyBackgroundPad = 14;

	[Tooltip("List of possible color sets for the health obfusticator, index is used by the HidesHealthStamina effect.")]
	public ObfusticatorColor[] ObfusticatorColors;

	[HideInInspector]
	public int ActivePortraitCount;

	private UIPartyPortrait[] m_Portraits = new UIPartyPortrait[6];

	public float PortraitSlideSpeed = 500f;

	public int OffsetOnDrag = 20;

	private UIPartyPortrait m_prevDragLeader;

	private bool m_NeedsRebundle;

	public static UIPartyPortraitBar Instance { get; private set; }

	public float TotalWidth => Spacing * (float)(m_Portraits.Length - 1) + PortraitWidth * (float)m_Portraits.Length;

	public float CurrentWidth => Spacing * (float)(ActivePortraitCount - 1) + PortraitWidth * (float)ActivePortraitCount;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		PartyPortrait.transform.localPosition = Vector3.zero;
		m_Portraits[0] = PartyPortrait.GetComponent<UIPartyPortrait>();
		for (int i = 1; i < m_Portraits.Length; i++)
		{
			m_Portraits[i] = NGUITools.AddChild(base.gameObject, PartyPortrait).GetComponent<UIPartyPortrait>();
		}
		UIConsole.Instance.LastPartyPortraitMarker = m_Portraits[m_Portraits.Length - 1].StatusEffectStrip.gameObject;
		RepositionPortraits();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		ActivePortraitCount = 0;
		int i = 0;
		for (int j = 0; j < 6; j++)
		{
			PartyMemberAI partyMemberAI = PartyMemberAI.PartyMembers[j];
			if (partyMemberAI != null && i < m_Portraits.Length)
			{
				m_Portraits[i].gameObject.SetActive(value: true);
				m_Portraits[i].SetPartyMember(partyMemberAI);
				m_Portraits[i].CurrentSlot = i;
				m_Portraits[i].Minion.SetPartyMember(GameUtilities.FindAnimalCompanion(partyMemberAI.gameObject));
				ActivePortraitCount++;
				i++;
			}
		}
		if ((bool)RightEdge)
		{
			RightEdge.transform.localPosition = new Vector3(GetDesiredXPosition(ActivePortraitCount - 1) + PortraitWidth / 2f - 4f, 0f, 0f);
		}
		if (m_NeedsRebundle)
		{
			m_NeedsRebundle = false;
			UIPartyPortrait[] portraits = m_Portraits;
			foreach (UIPartyPortrait uIPartyPortrait in portraits)
			{
				if ((bool)uIPartyPortrait)
				{
					uIPartyPortrait.RebundleEffects();
				}
			}
		}
		int[][] array = (GameState.s_playerCharacter ? GameState.s_playerCharacter.SelectionGroups : null);
		if (!GameInput.BlockAllKeys && UIWindowManager.KeyInputAvailable && !Cutscene.CutsceneActive && array != null)
		{
			for (int l = 0; l < 10; l++)
			{
				if (l >= array.Length || array[l] == null || array[l].Length == 0)
				{
					continue;
				}
				int num = l + 1;
				if (num > 10)
				{
					num = 0;
				}
				if (!GameInput.GetDoublePressed((KeyCode)(48 + num), handle: true))
				{
					continue;
				}
				Vector3 zero = Vector3.zero;
				int num2 = 0;
				int[] array2 = array[l];
				foreach (int num3 in array2)
				{
					if ((bool)PartyMemberAI.PartyMembers[num3])
					{
						zero += PartyMemberAI.PartyMembers[num3].transform.position;
						num2++;
					}
				}
				if ((bool)CameraControl.Instance && num2 > 0)
				{
					CameraControl.Instance.FocusOnPoint(zero / num2, 0.2f);
				}
			}
			int numberPressed = GameInput.NumberPressed;
			int num4 = numberPressed - 1;
			if (num4 == -1)
			{
				num4 = 10;
			}
			if (num4 >= 0 && array != null)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				if (GameInput.GetControlkey())
				{
					array[num4] = new int[PartyMemberAI.SelectedPartyMembers.Count((GameObject go) => go)];
					int num5 = 0;
					for (int m = 0; m < PartyMemberAI.SelectedPartyMembers.Length; m++)
					{
						if ((bool)PartyMemberAI.SelectedPartyMembers[m])
						{
							array[num4][num5] = m;
							num5++;
						}
					}
				}
				else
				{
					int[] array3 = array[num4];
					if (array3 != null && array3.Length != 0)
					{
						int n;
						for (n = 0; n < array3.Length; n++)
						{
							PartyMemberAI partyMemberAI2 = PartyMemberAI.PartyMembers[array3[n]];
							if ((bool)partyMemberAI2)
							{
								if (!GameInput.GetShiftkey())
								{
									partyMemberAI2.ExclusiveSelect();
								}
								else
								{
									partyMemberAI2.DragSelected = !partyMemberAI2.DragSelected;
								}
								break;
							}
						}
						for (; n < array3.Length; n++)
						{
							PartyMemberAI partyMemberAI3 = PartyMemberAI.PartyMembers[array3[n]];
							if ((bool)partyMemberAI3)
							{
								partyMemberAI3.Selected = true;
							}
						}
					}
					else
					{
						UISystemMessager.Instance.PostMessage(GUIUtils.Format(1619, numberPressed), Color.red);
					}
				}
			}
			int num6 = 0;
			if (GameInput.GetControlDownWithRepeat(MappedControl.NEXT_COMPANION, handle: true))
			{
				num6++;
			}
			if (GameInput.GetControlDownWithRepeat(MappedControl.PREVIOUS_COMPANION, handle: true))
			{
				num6--;
			}
			if (num6 != 0)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				PartyMemberAI current = PartyMemberAI.OnlyPrimaryPartyMembers.First((PartyMemberAI pai) => pai.Selected);
				if (num6 > 0)
				{
					PartyHelper.SeekNextPartyMember(current).ExclusiveSelect();
				}
				else
				{
					PartyHelper.SeekPreviousPartyMember(current).ExclusiveSelect();
				}
			}
		}
		for (; i < m_Portraits.Length; i++)
		{
			m_Portraits[i].Minion.SetPartyMember(null);
			m_Portraits[i].gameObject.SetActive(value: false);
		}
		CompressPortraits();
		if ((bool)SolidBackground)
		{
			SolidBackground.transform.localScale = new Vector3(CurrentWidth + (float)PartyBackgroundPad, SolidBackground.transform.localScale.y, 1f);
		}
	}

	public void ReloadPartyMembers()
	{
		UIPartyPortrait[] portraits = m_Portraits;
		foreach (UIPartyPortrait uIPartyPortrait in portraits)
		{
			if ((bool)uIPartyPortrait)
			{
				uIPartyPortrait.ReloadPartyMember();
			}
		}
	}

	public void ShiftPortrait(GameObject portrait, int slots)
	{
		CompressPortraits();
		PartyMemberAI.CompressPartyMembers();
		int num = -1;
		for (int i = 0; i < m_Portraits.Length; i++)
		{
			if (m_Portraits[i].gameObject == portrait)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		int value = num + slots;
		value = Mathf.Clamp(value, 0, ActivePortraitCount - 1);
		UIPartyPortrait uIPartyPortrait = m_Portraits[num];
		if (value > num)
		{
			for (int j = num; j < value; j++)
			{
				m_Portraits[j] = m_Portraits[j + 1];
				PartyMemberAI.PartyMembers[j].SwapWith(j + 1);
			}
		}
		else
		{
			for (int num2 = num; num2 > value; num2--)
			{
				m_Portraits[num2] = m_Portraits[num2 - 1];
				PartyMemberAI.PartyMembers[num2].SwapWith(num2 - 1);
			}
		}
		m_Portraits[value] = uIPartyPortrait;
	}

	public void CompressPortraits()
	{
		int num = -1;
		for (int i = 0; i < m_Portraits.Length; i++)
		{
			if (m_Portraits[i].gameObject.activeSelf)
			{
				if (num >= 0)
				{
					UIPartyPortrait uIPartyPortrait = m_Portraits[num];
					m_Portraits[num] = m_Portraits[i];
					m_Portraits[i] = uIPartyPortrait;
					i = num;
					num = -1;
				}
			}
			else if (num < 0)
			{
				num = i;
			}
		}
	}

	public void RepositionPortraits()
	{
		for (int i = 0; i < m_Portraits.Length; i++)
		{
			m_Portraits[i].transform.localPosition = new Vector3(GetDesiredXPosition(i), m_Portraits[i].transform.localPosition.y, PartyPortrait.transform.localPosition.z);
		}
	}

	public float GetDesiredXPosition(int slot)
	{
		return (Spacing + PortraitWidth) * (float)slot;
	}

	public void StartDrag()
	{
		m_prevDragLeader = m_Portraits[0];
	}

	public void EndDrag()
	{
		if (m_prevDragLeader != m_Portraits[0] && (bool)m_Portraits[0].PartyMemberAI.SoundSet)
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(m_Portraits[0].PartyMemberAI.gameObject, SoundSet.SoundAction.Leading, SoundSet.s_VeryShortVODelay, forceInterrupt: false);
		}
	}

	public void RebundleEffectsAll()
	{
		m_NeedsRebundle = true;
	}

	public UIPartyPortrait GetPortraitFor(PartyMemberAI pai)
	{
		if (m_Portraits.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < m_Portraits.Length; i++)
		{
			UIPartyPortrait uIPartyPortrait = m_Portraits[i];
			if (uIPartyPortrait.PartyMemberAI == pai || uIPartyPortrait.Minion.SelectedPartyMemberAI == pai)
			{
				return uIPartyPortrait;
			}
		}
		return m_Portraits[0];
	}
}
