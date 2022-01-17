public abstract class UICharacterCreationElementSetter : UICharacterCreationElement
{
	private void OnClick()
	{
		Set();
	}

	private void OnDoubleClick()
	{
	}

	public virtual void SetIfUndefined()
	{
	}

	public virtual void Set()
	{
	}
}
