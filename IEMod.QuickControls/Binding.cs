public class Binding<T>
{
	public Binding(IBindingValue<T> source, BindingMode mode = BindingMode.TwoWay)
	{
		Source = source;
		Mode = mode;
	}

	public IBindingValue<T> Source { get; }

	public BindingMode Mode { get; }
}
