using System;
using System.Collections.Generic;
using System.Linq;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Parsing;
using IllusionScript.Runtime.Parsing.Nodes;
using Xunit;

namespace Runtime.Test;

internal sealed class AssertingEnumerator : IDisposable
{
    private readonly IEnumerator<Node> enumerator;
    private bool hasErrors;

    public AssertingEnumerator(Node node)
    {
        enumerator = Flatten(node).GetEnumerator();
    }

    private bool MarkFailed()
    {
        hasErrors = true;
        return false;
    }

    public void Dispose()
    {
        if (!hasErrors)
            Assert.False(enumerator.MoveNext());

        enumerator.Dispose();
    }

    private static IEnumerable<Node> Flatten(Node node)
    {
        Stack<Node> stack = new Stack<Node>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            Node n = stack.Pop();
            yield return n;

            foreach (Node child in n.GetChildren().Reverse())
                stack.Push(child);
        }
    }

    public void AssertNode(SyntaxType type)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(type,enumerator.Current.type);
            Assert.IsNotType<Token>(enumerator.Current);
        }
        catch when (MarkFailed())
        {
            throw;
        }
    }

    public void AssertToken(SyntaxType type, string text)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(type,enumerator.Current.type);
            Token token = Assert.IsType<Token>(enumerator.Current);
            Assert.Equal(text, token.text);
        }
        catch when (MarkFailed())
        {
            throw;
        }
    }
}