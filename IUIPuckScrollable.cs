public interface IUIPuckScrollable
{
	bool RestrictWithinBoundsEnabled { get; set; }

	void SetScroll(float val);

	float GetScroll();

	float GetVisibleWidth();

	float GetVisibleHeight();

	float GetRealMax();

	float GetRealRange();
}
