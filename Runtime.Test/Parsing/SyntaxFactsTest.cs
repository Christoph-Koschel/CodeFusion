using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Parsing;
using Xunit;

namespace IllusionScript.Runtime.Test.Parsing;

public class SyntaxFactsTest
{
    [Theory]
    [MemberData(nameof(GetSyntaxTypeData))]
    public void GetTextTest(SyntaxType type)
    {
        string text = SyntaxFacts.GetText(type);
        if (text == null)
        {
            return;
        }

        Token[] tokens = SyntaxTree.ParseTokens(text, out ImmutableArray<Diagnostic> diagnostics).ToArray();
        Token token = Assert.Single(tokens);
        Assert.Equal(type, token.type);
        Assert.Equal(text, token.text);
    }

    public static IEnumerable<object[]> GetSyntaxTypeData()
    {
        SyntaxType[] values = (SyntaxType[])Enum.GetValues(typeof(SyntaxType));
        foreach (SyntaxType value in values)
        {
            yield return new object[] { value };
        }
    }
}