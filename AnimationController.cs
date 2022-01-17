using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Animation/Animation Controller")]
public class AnimationController : MonoBehaviour
{
	public enum MovementType
	{
		None,
		Stand,
		Walk,
		Run,
		Sprint
	}

	public enum ActionType
	{
		None,
		Attack,
		Ability_DONOTUSE,
		Reload,
		Fidget,
		Ambient,
		Use,
		Pending
	}

	public enum ReactionType
	{
		None,
		Hit,
		Stun,
		Dead,
		Standup,
		Knockdown,
		DeadProne,
		Pending
	}

	public class Action
	{
		public ActionType m_actionType;

		public int m_variation;

		public float m_speed = 1f;

		public bool m_offhand;

		public void Reset()
		{
			m_actionType = ActionType.None;
			m_variation = 0;
			m_speed = 1f;
			m_offhand = false;
		}

		public override bool Equals(object o)
		{
			if (!(o is Action action))
			{
				return false;
			}
			if (m_actionType == action.m_actionType)
			{
				return m_variation == action.m_variation;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_actionType.GetHashCode() + 31 * m_variation;
		}
	}

	protected string[] MovementTable = new string[5]
	{
		string.Empty,
		"idle",
		"walk",
		"run",
		"run"
	};

	protected string[] ActionTable = new string[8]
	{
		string.Empty,
		"Attack",
		"Ability",
		"Reload",
		"Fidget",
		"Ambient",
		"Use",
		"pending"
	};

	protected string[] ActionTableLowerCase = new string[8]
	{
		string.Empty,
		"attack",
		"ability",
		"reload",
		"fidget",
		"ambient",
		"use",
		"pending"
	};

	protected string[] ReactionTable = new string[8]
	{
		string.Empty,
		"Hit",
		"Stun",
		"Dead",
		"Standup",
		"Knockdown",
		"DeadProne",
		"error"
	};

	protected string[] ReactionTableLowerCase = new string[8]
	{
		string.Empty,
		"hit",
		"stun",
		"dead",
		"standup",
		"knockdown",
		"deadprone",
		"error"
	};

	protected BitArray m_eventUsed = new BitArray(14);

	private static int IdleHash;

	public static string[] CachedVariationStrings;

	private Animator m_avatar;

	private Mover m_agent;

	private Action m_desiredAction = new Action();

	private Action m_currentAction = new Action();

	private ReactionType m_currentReaction;

	private int m_stance;

	public int FidgetCount;

	public float MinFidgetTime = 8f;

	public float RandomUpdateTime = 1f;

	private float m_randomTimer;

	private float m_avatarSpeed = 1f;

	private int m_timescaleOverride;

	private float m_combatBlend;

	private float m_stanceBlend;

	private float m_sneakBlend;

	private Transform IKTarget;

	private List<Animator> m_syncList = new List<Animator>();

	private bool m_canChangeStance;

	[HideInInspector]
	public float OverrideSpeed;

	[HideInInspector]
	public float CutsceneSpeed = -1f;

	public CharacterStats.Race RacialBodyTypeOverride;

	public Gender GenderOverride = Gender.Neuter;

	private float m_verticalLaunchVelocity;

	private float m_verticalLaunchOffset;

	private Transform m_skeleton;

	public List<Animator> SyncList
	{
		get
		{
			return m_syncList;
		}
		set
		{
			m_syncList = value;
		}
	}

	public Animator CurrentAvatar
	{
		get
		{
			return m_avatar;
		}
		set
		{
			m_avatar = value;
		}
	}

	public Action DesiredAction
	{
		get
		{
			return m_desiredAction;
		}
		set
		{
			m_desiredAction = value;
		}
	}

	public virtual Action CurrentAction
	{
		get
		{
			return m_currentAction;
		}
		set
		{
			m_currentAction = value;
		}
	}

	public virtual ReactionType CurrentReaction => m_currentReaction;

	public float AnimationSpeed
	{
		get
		{
			return m_avatar.speed;
		}
		set
		{
			CallActionOnAnimators(delegate(Animator a)
			{
				a.speed = value;
			});
		}
	}

	public float SpeedAnimationParameter
	{
		get
		{
			return m_avatar.GetFloat("Speed");
		}
		set
		{
			SetFloat("Speed", value);
		}
	}

	public bool Walk
	{
		get
		{
			return m_avatar.GetBool("Walk");
		}
		set
		{
			SetBool("Walk", value);
		}
	}

	public bool Idle
	{
		get
		{
			if ((bool)m_avatar)
			{
				return m_avatar.GetCurrentAnimatorStateInfo(0).tagHash == IdleHash;
			}
			return false;
		}
	}

	public float ReloadTime
	{
		get
		{
			return m_avatar.GetFloat("ReloadTime");
		}
		set
		{
			SetFloat("ReloadTime", value);
		}
	}

	public bool Loop
	{
		get
		{
			return m_avatar.GetBool("Loop");
		}
		set
		{
			SetBool("Loop", value);
		}
	}

	public bool Instant
	{
		get
		{
			return m_avatar.GetBool("Instant");
		}
		set
		{
			SetBool("Instant", value);
		}
	}

	public bool Sneak
	{
		get
		{
			return m_avatar.GetBool("Sneak");
		}
		set
		{
			SetBool("Sneak", value);
			if (value)
			{
				m_sneakBlend = 1f - m_combatBlend;
			}
			else
			{
				m_sneakBlend = m_combatBlend;
			}
			ForceCombatIdle = value;
		}
	}

	public float RandomFixed
	{
		get
		{
			return m_avatar.GetFloat("RandomFixed");
		}
		set
		{
			SetFloat("RandomFixed", value);
		}
	}

	public float RandomInterval
	{
		get
		{
			return m_avatar.GetFloat("RandomInterval");
		}
		set
		{
			SetFloat("RandomInterval", value);
		}
	}

	public bool IsInCombatMode { get; set; }

	public bool ForceCombatIdle { get; set; }

	public int Stance
	{
		get
		{
			return m_stance;
		}
		set
		{
			if (m_avatar == null)
			{
				BindComponents();
			}
			if (m_avatar == null)
			{
				return;
			}
			if (!Sneak && m_stance != value)
			{
				if (m_combatBlend >= 1f)
				{
					m_stanceBlend = 0f;
				}
				else
				{
					m_stanceBlend = 1f;
				}
			}
			m_stance = value;
		}
	}

	public bool IsFidgeting
	{
		get
		{
			if (m_currentAction.m_actionType != ActionType.Fidget)
			{
				return m_desiredAction.m_actionType == ActionType.Fidget;
			}
			return true;
		}
	}

	public float TimeScale
	{
		get
		{
			return m_avatarSpeed;
		}
		set
		{
			m_avatarSpeed = value;
			UpdateAnimationSpeed();
		}
	}

	public int TimeScaleOverride
	{
		get
		{
			return m_timescaleOverride;
		}
		set
		{
			m_timescaleOverride = value;
			UpdateAnimationSpeed();
		}
	}

	public bool IsTransitioning
	{
		get
		{
			if (!m_avatar.IsInTransition(0))
			{
				return m_avatar.IsInTransition(m_stance);
			}
			return true;
		}
	}

	public event EventHandler OnEventHit;

	public event EventHandler OnEventHitReact;

	public event EventHandler OnEventCancelStart;

	public event EventHandler OnEventCancelEnd;

	public event EventHandler OnEventAudio;

	public event EventHandler OnEventFootstep;

	public event EventHandler OnEventJostle;

	public event EventHandler OnTargetableToggled;

	public event EventHandler OnEventPlayFX;

	public event EventHandler OnEventShowSlot;

	public event EventHandler OnEventHideSlot;

	public event EventHandler OnEventMoveToHand;

	public event EventHandler OnEventMoveFromHand;

	public void VerticalLaunch(float velocity)
	{
		m_verticalLaunchVelocity = velocity;
	}

	static AnimationController()
	{
		IdleHash = Animator.StringToHash("idle");
		CachedVariationStrings = null;
		CachedVariationStrings = new string[1000];
		for (int i = 0; i < 1000; i++)
		{
			CachedVariationStrings[i] = i.ToString("00");
		}
	}

	public float GetLengthOfCurrentAnim()
	{
		return m_avatar.GetCurrentAnimatorStateInfo(0).length;
	}

	public bool IsPerformingAction(ActionType action, int variation)
	{
		string text = ActionTableLowerCase[(int)action] + ((variation < 1000 && variation >= 0) ? CachedVariationStrings[variation] : variation.ToString("00"));
		return ((!m_avatar.IsInTransition(0)) ? m_avatar.GetCurrentAnimatorStateInfo(0) : m_avatar.GetNextAnimatorStateInfo(0)).IsTag(text);
	}

	public virtual bool IsPerformingReaction(ReactionType reaction)
	{
		if (m_avatar == null)
		{
			return false;
		}
		string text = ReactionTableLowerCase[(int)reaction];
		return ((!m_avatar.IsInTransition(0)) ? m_avatar.GetCurrentAnimatorStateInfo(0) : m_avatar.GetNextAnimatorStateInfo(0)).IsTag(text);
	}

	public void ClearReactions()
	{
		Loop = false;
		m_currentReaction = ReactionType.None;
	}

	public void ClearActions()
	{
		Loop = false;
		if ((bool)m_avatar)
		{
			for (int i = 1; i < ActionTable.Length - 1; i++)
			{
				SetInteger(ActionTable[i], 0);
			}
			SetBool("Offhand", b: false);
		}
		TimeScale = 1f;
		m_currentAction.Reset();
	}

	private void UpdateAnimationSpeed()
	{
		if (!(m_avatar != null))
		{
			return;
		}
		if (TimeScaleOverride == 0)
		{
			if (AnimationSpeed != m_avatarSpeed)
			{
				AnimationSpeed = m_avatarSpeed;
			}
		}
		else if (AnimationSpeed > float.Epsilon)
		{
			AnimationSpeed = 0f;
		}
	}

	protected virtual void Start()
	{
		BindComponents();
		RandomFixed = OEIRandom.FloatValue();
		RandomInterval = OEIRandom.FloatValue();
		m_randomTimer = RandomUpdateTime;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLoadSceneCallback;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLoadSceneCallback;
	}

	private void OnLoadSceneCallback(Scene scene, LoadSceneMode sceneMode)
	{
		if (!GameState.CurrentSceneIsTransitionScene())
		{
			BindComponents();
		}
	}

	private void HandleBodyTypeSettings()
	{
		if (!(m_avatar != null))
		{
			return;
		}
		m_avatar.logWarnings = false;
		if (m_avatar.isHuman)
		{
			CharacterStats component = GetComponent<CharacterStats>();
			if ((bool)component)
			{
				SetInteger("Race", (int)((RacialBodyTypeOverride != 0) ? RacialBodyTypeOverride : component.RacialBodyType));
				SetBool("Female", (GenderOverride != Gender.Neuter) ? (GenderOverride == Gender.Female) : (component.Gender == Gender.Female));
			}
		}
	}

	public void BindComponents()
	{
		m_agent = GetComponent<Mover>();
		m_avatar = GetComponent<Animator>();
		if (!m_avatar)
		{
			m_avatar = GetComponentInChildren<Animator>();
		}
		if (m_avatar != null)
		{
			m_avatar.logWarnings = false;
			if (m_avatar.isHuman)
			{
				HandleBodyTypeSettings();
				IKTarget = GetBoneTransform("ik_leftHand", base.gameObject.transform);
			}
			m_canChangeStance = m_avatar.layerCount > 1;
			if (m_avatar.layerCount == 2 && m_avatar.GetLayerName(1).ToLower() == "additive")
			{
				m_canChangeStance = false;
			}
		}
		m_skeleton = GameUtilities.FindSkeletonTransform(base.gameObject);
	}

	protected virtual void Update()
	{
		m_eventUsed.SetAll(value: false);
		if (m_avatar == null)
		{
			return;
		}
		m_randomTimer -= Time.deltaTime;
		if (m_randomTimer <= 0f)
		{
			RandomInterval = OEIRandom.FloatValue();
			m_randomTimer = RandomUpdateTime;
		}
		if (m_verticalLaunchOffset > 0f || m_verticalLaunchVelocity != 0f)
		{
			float num = -40f;
			m_verticalLaunchVelocity += num * Time.deltaTime;
			m_verticalLaunchOffset += m_verticalLaunchVelocity * Time.deltaTime;
			if (m_verticalLaunchOffset < 0f)
			{
				m_verticalLaunchOffset = 0f;
				m_verticalLaunchVelocity = 0f;
				m_avatar.SetTrigger("Knockdown");
			}
		}
		if ((bool)m_skeleton)
		{
			m_skeleton.transform.localPosition = new Vector3(0f, m_verticalLaunchOffset, 0f);
		}
		if (m_canChangeStance)
		{
			float num2 = 2f * Time.deltaTime;
			if (IsInCombatMode || ForceCombatIdle)
			{
				m_combatBlend += num2;
			}
			else
			{
				m_combatBlend -= num2;
			}
			m_sneakBlend -= num2;
			m_stanceBlend += 3f * Time.unscaledDeltaTime;
			m_combatBlend = Mathf.Clamp(m_combatBlend, 0f, 1f);
			m_stanceBlend = Mathf.Clamp(m_stanceBlend, 0f, 1f);
			m_sneakBlend = Mathf.Clamp(m_sneakBlend, 0f, 1f);
			float alpha = 0f;
			if (m_combatBlend < 1f)
			{
				alpha = Mathf.SmoothStep(0f, 1f, m_combatBlend);
			}
			else
			{
				alpha = Mathf.SmoothStep(0f, 1f, m_stanceBlend);
			}
			int stance = 0;
			stance = ((Sneak || m_sneakBlend > float.Epsilon) ? 1 : m_stance);
			if (stance == 0)
			{
				stance = 1;
			}
			int i;
			for (i = 1; i < m_avatar.layerCount - 1; i++)
			{
				if (i == stance)
				{
					CallActionOnAnimators(delegate(Animator a)
					{
						SetLayerWeight(a, stance, alpha);
					});
				}
				else
				{
					if (!(m_avatar.GetLayerWeight(i) > float.Epsilon))
					{
						continue;
					}
					if (m_combatBlend < float.Epsilon)
					{
						CallActionOnAnimators(delegate(Animator a)
						{
							SetLayerWeight(a, i, 0f);
						});
					}
					else
					{
						CallActionOnAnimators(delegate(Animator a)
						{
							SetLayerWeight(a, i, 1f - alpha);
						});
					}
				}
			}
		}
		ProcessMovement();
		if (m_currentReaction != 0)
		{
			if (m_avatar.IsInTransition(0) || IsPerformingReaction(m_currentReaction))
			{
				return;
			}
			m_currentReaction = ReactionType.None;
		}
		ProcessActions();
	}

	private void CallActionOnAnimators(Action<Animator> action)
	{
		if (!m_avatar)
		{
			return;
		}
		action(m_avatar);
		if (SyncList == null)
		{
			return;
		}
		for (int i = 0; i < SyncList.Count; i++)
		{
			if (SyncList[i] != null)
			{
				action(SyncList[i]);
			}
		}
	}

	private void SetLayerWeight(Animator animator, int layer, float weight)
	{
		if ((bool)animator && animator.layerCount > layer)
		{
			animator.SetLayerWeight(layer, weight);
		}
	}

	public void SetFloat(string str, float f)
	{
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetFloat(str, f);
		});
	}

	public void SetInteger(string str, int i)
	{
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetInteger(str, i);
		});
	}

	public void SetBool(string str, bool b)
	{
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetBool(str, b);
		});
	}

	public void SetTrigger(string str)
	{
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetTrigger(str);
		});
	}

	public void ResetTrigger(string str)
	{
		CallActionOnAnimators(delegate(Animator a)
		{
			a.ResetTrigger(str);
		});
	}

	public void Interrupt()
	{
		if ((bool)m_avatar && m_currentReaction == ReactionType.None)
		{
			SetTrigger("Interrupt");
			ClearActions();
		}
	}

	public void ClearInterrupt()
	{
		if ((bool)m_avatar && m_currentReaction == ReactionType.None)
		{
			ResetTrigger("Interrupt");
		}
	}

	public void Flinch()
	{
		SetTrigger("Flinch");
	}

	protected virtual bool ProcessActions()
	{
		if (m_desiredAction.m_actionType == ActionType.None && m_currentAction.m_actionType == ActionType.None)
		{
			return false;
		}
		if (m_desiredAction.m_actionType == m_currentAction.m_actionType && m_desiredAction.m_variation == m_currentAction.m_variation)
		{
			return true;
		}
		if (m_currentAction.m_actionType == ActionType.Pending)
		{
			if (IsPerformingAction(m_desiredAction.m_actionType, m_desiredAction.m_variation))
			{
				m_currentAction.m_actionType = m_desiredAction.m_actionType;
				m_currentAction.m_variation = m_desiredAction.m_variation;
				TimeScale = m_desiredAction.m_speed;
			}
			return true;
		}
		if (m_desiredAction.m_actionType == ActionType.None && m_currentAction.m_actionType != 0 && m_currentAction.m_actionType != ActionType.Pending)
		{
			SetBool("Offhand", b: false);
			SetInteger(ActionTable[(int)m_currentAction.m_actionType], 0);
		}
		if (m_desiredAction.m_actionType == ActionType.None)
		{
			m_currentAction.m_actionType = ActionType.None;
			m_currentAction.m_variation = 0;
		}
		else
		{
			SetBool("Offhand", m_desiredAction.m_offhand);
			SetInteger(ActionTable[(int)m_desiredAction.m_actionType], m_desiredAction.m_variation);
			m_currentAction.m_actionType = ActionType.Pending;
			TimeScale = m_desiredAction.m_speed;
		}
		return true;
	}

	protected virtual bool ProcessMovement()
	{
		float num = 0f;
		num = ((!m_agent) ? OverrideSpeed : m_agent.AnimationSpeed);
		if (m_currentAction.m_actionType == ActionType.None && m_avatar != null && m_agent != null)
		{
			SpeedAnimationParameter = num;
			if (CutsceneSpeed >= 0f)
			{
				if (AnimationSpeed != CutsceneSpeed)
				{
					AnimationSpeed = CutsceneSpeed;
				}
			}
			else if (TimeScaleOverride == 0)
			{
				float animSpeedMultiplier = m_agent.AnimSpeedMultiplier;
				if (AnimationSpeed != animSpeedMultiplier)
				{
					AnimationSpeed = animSpeedMultiplier;
				}
			}
			else if (AnimationSpeed > float.Epsilon)
			{
				AnimationSpeed = 0f;
			}
		}
		return true;
	}

	public Transform GetBoneTransform(string name, Transform parent)
	{
		Transform transform = parent.Find(name);
		if (transform != null)
		{
			return transform;
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			transform = parent.GetChild(i).Find(name);
			if (transform != null)
			{
				return transform;
			}
		}
		for (int j = 0; j < childCount; j++)
		{
			transform = GetBoneTransform(name, parent.GetChild(j));
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static Transform SearchForBoneTransform(string matchString, Transform parent)
	{
		Transform transform = parent;
		if (transform.name.ToLower().Contains(matchString.ToLower()))
		{
			return transform;
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			transform = parent.GetChild(i).Find(matchString);
			if (parent.GetChild(i).name.ToLower().Contains(matchString.ToLower()))
			{
				return parent.GetChild(i);
			}
			if (transform != null)
			{
				return transform;
			}
		}
		for (int j = 0; j < childCount; j++)
		{
			transform = SearchForBoneTransform(matchString, parent.GetChild(j));
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public Transform GetBoneTransform(HumanBodyBones bone)
	{
		if (m_avatar == null)
		{
			m_avatar = GetComponent<Animator>();
		}
		if (m_avatar == null || m_avatar.avatar == null)
		{
			return null;
		}
		return m_avatar.GetBoneTransform(bone);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if ((m_stance != 2 && m_stance != 3) || (!IsFidgeting && !Idle))
		{
			return;
		}
		if (IKTarget == null)
		{
			IKTarget = GetBoneTransform("ik_leftHand", base.gameObject.transform);
			if (IKTarget == null)
			{
				return;
			}
		}
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_combatBlend);
		});
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetIKPosition(AvatarIKGoal.LeftHand, IKTarget.position);
		});
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_combatBlend);
		});
		CallActionOnAnimators(delegate(Animator a)
		{
			a.SetIKRotation(AvatarIKGoal.LeftHand, IKTarget.rotation);
		});
	}

	public virtual void AnimEventHit()
	{
		if (!m_eventUsed.Get(0))
		{
			m_eventUsed.Set(0, value: true);
			if (this.OnEventHit != null)
			{
				this.OnEventHit(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventFootstep()
	{
		if (!m_eventUsed.Get(1))
		{
			m_eventUsed.Set(1, value: true);
			if (this.OnEventFootstep != null)
			{
				this.OnEventFootstep(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventAudio(string sound)
	{
		if (!m_eventUsed.Get(2))
		{
			m_eventUsed.Set(2, value: true);
			if (this.OnEventAudio != null)
			{
				this.OnEventAudio(sound, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventJostle()
	{
		if (!m_eventUsed.Get(3))
		{
			m_eventUsed.Set(3, value: true);
			if (this.OnEventJostle != null)
			{
				this.OnEventJostle(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventHitReact()
	{
		if (!m_eventUsed.Get(4))
		{
			m_eventUsed.Set(4, value: true);
			if (this.OnEventHitReact != null)
			{
				this.OnEventHitReact(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventCancelStart()
	{
		if (!m_eventUsed.Get(5))
		{
			m_eventUsed.Set(5, value: true);
			if (this.OnEventCancelStart != null)
			{
				this.OnEventCancelStart(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventCancelEnd()
	{
		if (!m_eventUsed.Get(6))
		{
			m_eventUsed.Set(6, value: true);
			if (this.OnEventCancelEnd != null)
			{
				this.OnEventCancelEnd(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventTargetableToggle()
	{
		if (!m_eventUsed.Get(7))
		{
			m_eventUsed.Set(7, value: true);
			if (this.OnTargetableToggled != null)
			{
				this.OnTargetableToggled(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventPlayFX()
	{
		if (!m_eventUsed.Get(8))
		{
			m_eventUsed.Set(8, value: true);
			if (this.OnEventPlayFX != null)
			{
				this.OnEventPlayFX(base.gameObject, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventShowSlot(string slot)
	{
		if (!m_eventUsed.Get(9))
		{
			m_eventUsed.Set(9, value: true);
			if (this.OnEventShowSlot != null)
			{
				this.OnEventShowSlot(slot, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventHideSlot(string slot)
	{
		if (!m_eventUsed.Get(10))
		{
			m_eventUsed.Set(10, value: true);
			if (this.OnEventHideSlot != null)
			{
				this.OnEventHideSlot(slot, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventMoveToHand(string slot)
	{
		if (!m_eventUsed.Get(11))
		{
			m_eventUsed.Set(11, value: true);
			if (this.OnEventMoveToHand != null)
			{
				this.OnEventMoveToHand(slot, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventMoveFromHand(string slot)
	{
		if (!m_eventUsed.Get(12))
		{
			m_eventUsed.Set(12, value: true);
			if (this.OnEventMoveFromHand != null)
			{
				this.OnEventMoveFromHand(slot, EventArgs.Empty);
			}
		}
	}

	public virtual void AnimEventScreenShake(AnimationEvent animEvent)
	{
		if (!m_eventUsed.Get(13))
		{
			m_eventUsed.Set(13, value: true);
			if (!FogOfWar.Instance || FogOfWar.Instance.PointVisible(base.transform.position))
			{
				float num = Mathf.Clamp(animEvent.intParameter, 0, (int)Enum.GetValues(typeof(CameraControl.ScreenShakeValues)).Cast<CameraControl.ScreenShakeValues>().Last());
				float shakeDuration = CameraControl.Instance.GetShakeDuration((CameraControl.ScreenShakeValues)num);
				CameraControl.Instance.ScreenShake(shakeDuration, animEvent.floatParameter);
			}
		}
	}

	public virtual void SetReaction(ReactionType reaction)
	{
		if (!(m_avatar == null) && m_currentReaction != reaction)
		{
			if (reaction == ReactionType.Dead && IsPerformingReaction(ReactionType.Knockdown))
			{
				SetTrigger(ReactionTable[6]);
			}
			else
			{
				SetTrigger(ReactionTable[(int)reaction]);
			}
			m_currentReaction = reaction;
		}
	}
}
