using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Parsing.Nodes;

namespace IllusionScript.Runtime.Extension;

public abstract class SeparatedSyntaxList
{
    private protected SeparatedSyntaxList()
    {
    }

    public abstract ImmutableArray<Node> GetWithSeparators();
}

public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
    where T: Node
{
    private readonly ImmutableArray<Node> nodesAndSeparators;

    internal SeparatedSyntaxList(ImmutableArray<Node> nodesAndSeparators)
    {
        this.nodesAndSeparators = nodesAndSeparators;
    }

    public int Length => (nodesAndSeparators.Length + 1) / 2;

    public T this[int index] => (T) nodesAndSeparators[index * 2];

    public Token GetSeparator(int index)
    {
        if (index < 0 || index >= Length - 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        return (Token) nodesAndSeparators[index * 2 + 1];
    }

    public override ImmutableArray<Node> GetWithSeparators() => nodesAndSeparators;

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Length; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}