using UnityEngine;

public class UIHealthstringManager : MonoBehaviour
{
	public GameObject HealthStringPrefab;

	public Color DamageColor;

	public Color WarningColor = new Color(1f, 0.5f, 0.5f);

	private static UIHealthstringManager s_Instance;

	public Color HealingColor => InGameHUD.GetFriendlyColor();

	public static UIHealthstringManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		HealthStringPrefab.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void ShowNumber(DamageInfo damage, GameObject victim)
	{
		if (InGameHUD.Instance.ShowHUD && !(damage.FinalAdjustedDamage <= 0f))
		{
			UIHealthstring uIHealthstring = InstantiateString();
			uIHealthstring.Set(damage);
			uIHealthstring.Begin(victim, 1f);
		}
	}

	public void ShowNumber(float damage, GameObject victim)
	{
		if (InGameHUD.Instance.ShowHUD && !(Mathf.Abs(damage) < float.Epsilon))
		{
			UIHealthstring uIHealthstring = InstantiateString();
			uIHealthstring.Set(damage);
			uIHealthstring.Begin(victim, 1f);
		}
	}

	public void ShowWarning(string warning, GameObject victim, float duration = 1f)
	{
		if (InGameHUD.Instance.ShowHUD)
		{
			UIHealthstring uIHealthstring = InstantiateString();
			uIHealthstring.Set(warning, WarningColor);
			uIHealthstring.Begin(victim, duration);
		}
	}

	public void ShowNotice(string warning, GameObject victim, float duration = 1f)
	{
		if (InGameHUD.Instance.ShowHUD)
		{
			UIHealthstring uIHealthstring = InstantiateString();
			uIHealthstring.Set(warning, Color.white);
			uIHealthstring.Begin(victim, duration);
		}
	}

	private UIHealthstring InstantiateString()
	{
		GameObject obj = NGUITools.AddChild(base.gameObject, HealthStringPrefab.gameObject);
		obj.SetActive(value: true);
		obj.transform.localScale = HealthStringPrefab.transform.localScale;
		return obj.GetComponent<UIHealthstring>();
	}
}
