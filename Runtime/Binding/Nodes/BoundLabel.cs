namespace IllusionScript.Runtime.Binding.Nodes;

internal sealed class BoundLabel
{
    public readonly string name;

    public BoundLabel(string name)
    {
        this.name = name;
    }

    public override string ToString()
    {
        return name;
    }
}