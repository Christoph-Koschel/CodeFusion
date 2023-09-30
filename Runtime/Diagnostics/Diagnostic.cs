namespace IllusionScript.Runtime.Diagnostics;

public sealed class Diagnostic
{
    public readonly TextLocation location;
    public readonly string message;

    public Diagnostic(TextLocation location, string message)
    {
        this.location = location;
        this.message = message;
    }

    public override string ToString()
    {
        return message;
    }
}