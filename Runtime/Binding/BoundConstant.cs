namespace IllusionScript.Runtime.Binding;

internal sealed class BoundConstant
{
    public readonly object value;

    public BoundConstant(object value)
    {
        this.value = value;
    }
}