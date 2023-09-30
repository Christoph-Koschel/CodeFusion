using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace IllusionScript.Runtime.Test.Lexing;

public class LexerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public LexerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void LexerLexesUnterminatedString()
    {
        string text = "\"string";
        IEnumerable<Token> tokens = SyntaxTree.ParseTokens(text, out ImmutableArray<Diagnostic> diagnostics);

        Token token = Assert.Single(tokens);
        Assert.Equal(SyntaxType.StringToken, token.type);
        Assert.Equal(text, token.text);

        foreach (Diagnostic diagnostic1 in diagnostics)
        {
            testOutputHelper.WriteLine(diagnostic1.ToString());                
        }
            
        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ERROR: Unterminated string literal", diagnostic.message);
        Assert.Equal(new TextSpan(0, 1), diagnostic.location.span);
    }

    [Fact]
    public void LexerTestAllTokens()
    {
        List<SyntaxType> tokenTypes = Enum.GetValues(typeof(SyntaxType))
            .Cast<SyntaxType>()
            .Where(k => k.ToString().EndsWith("Keyword") || k.ToString().EndsWith("Token") && k.ToString() != "AnyToken").ToList();


        IEnumerable<SyntaxType> testedTokenTypes = GetTokens().Concat(GetSeparators()).Select(t => t.type);
        SortedSet<SyntaxType> untestedTypes = new SortedSet<SyntaxType>(tokenTypes);
        untestedTypes.Remove(SyntaxType.BadToken);
        untestedTypes.Remove(SyntaxType.EOFToken);
        untestedTypes.ExceptWith(testedTokenTypes);

        foreach (SyntaxType type in untestedTypes)
        {
            testOutputHelper.WriteLine(type.ToString());
        }

        Assert.Empty(untestedTypes);
    }

    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void LexerLexTokens(SyntaxType type, string text)
    {
        IEnumerable<Token> tokens = SyntaxTree.ParseTokens(text, out ImmutableArray<Diagnostic> diagnostics);

        Token token = Assert.Single(tokens);
        Assert.Equal(type, token.type);
        Assert.Equal(text, token.text);
    }

    [Theory]
    [MemberData(nameof(GetTokensPairData))]
    public void LexerLexPairTokens(SyntaxType type1, string text1,
        SyntaxType type2, string text2)
    {
        string text = text1 + text2;
        Token[] tokens = SyntaxTree.ParseTokens(text, out ImmutableArray<Diagnostic> diagnostics).ToArray();

        foreach (Token token in tokens)
        {
            testOutputHelper.WriteLine(token.type.ToString());
        }

        Assert.Equal(2, tokens.Length);
        Assert.Equal(type1, tokens[0].type);
        Assert.Equal(text1, tokens[0].text);
        Assert.Equal(type2, tokens[1].type);
        Assert.Equal(text2, tokens[1].text);
    }

    [Theory]
    [MemberData(nameof(GetTokensPairWithSpacesData))]
    public void LexerLexPairTokensWithSpaces(SyntaxType type1, string text1, SyntaxType separatorType,
        string separatorText, SyntaxType type2, string text2)
    {
        string text = text1 + separatorText + text2;
        Token[] tokens = SyntaxTree.ParseTokens(text, out ImmutableArray<Diagnostic> diagnostics).ToArray();
        Assert.Equal(3, tokens.Length);
        Assert.Equal(tokens[0].type, type1);
        Assert.Equal(tokens[0].text, text1);
        Assert.Equal(tokens[1].type, separatorType);
        Assert.Equal(tokens[1].text, separatorText);
        Assert.Equal(tokens[2].type, type2);
        Assert.Equal(tokens[2].text, text2);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        foreach ((SyntaxType kind, string text) token in GetTokens().Concat(GetSeparators()))
        {
            yield return new object[] { token.kind, token.text };
        }
    }

    public static IEnumerable<object[]> GetTokensPairData()
    {
        foreach ((SyntaxType t1Type, string t1Text, SyntaxType t2Type, string t2Text) pair in GetTokenPairs())
        {
            yield return new object[] { pair.t1Type, pair.t1Text, pair.t2Type, pair.t2Text };
        }
    }

    public static IEnumerable<object[]> GetTokensPairWithSpacesData()
    {
        foreach ((SyntaxType t1Type, string t1Text, SyntaxType seperatorType, string seperatorText, SyntaxType
                 t2Type, string t2Text) pair in GetTokensPairsWithSpaces())
        {
            yield return new object[]
                { pair.t1Type, pair.t1Text, pair.seperatorType, pair.seperatorText, pair.t2Type, pair.t2Text };
        }
    }

    private static IEnumerable<(SyntaxType type, string text)> GetTokens()
    {
        IEnumerable<(SyntaxType Type, string text)> fixedTokens = Enum.GetValues(typeof(SyntaxType))
            .Cast<SyntaxType>()
            .Select(v => (type: v, text: SyntaxFacts.GetText(v)))
            .Where((t) => t.text != null);


        IEnumerable<(SyntaxType Type, string text)> dynamicTokens = new[]
        {
            (SyntaxType.NumberToken, "1"),
            (SyntaxType.NumberToken, "123"),
            (SyntaxType.IdentifierToken, "a"),
            (SyntaxType.IdentifierToken, "abc"),
            (SyntaxType.StringToken, "\"this is a string\""),
            (SyntaxType.StringToken, "\"this is a string \\\" with a escaped quote\"")
        };

        return fixedTokens.Concat(dynamicTokens);
    }

    private static IEnumerable<(SyntaxType type, string text)> GetSeparators()
    {
        return new[]
        {
            (WhiteSpace: SyntaxType.WhiteSpaceToken, " "),
            (WhiteSpace: SyntaxType.WhiteSpaceToken, "  "),
            (WhiteSpace: SyntaxType.WhiteSpaceToken, "\r"),
            (WhiteSpace: SyntaxType.WhiteSpaceToken, "\n"),
            (WhiteSpace: SyntaxType.WhiteSpaceToken, "\r\n")
        };
    }

    private static IEnumerable<(SyntaxType t1Type, string t1Text, SyntaxType t2Type, string t2Text)> GetTokenPairs()
    {
        foreach ((SyntaxType type, string text) t1 in GetTokens())
        {
            foreach ((SyntaxType type, string text) t2 in GetTokens())
            {
                if (!RequiresSeparator(t1.type, t2.type))
                    yield return (t1.type, t1.text, t2.type, t2.text);
            }
        }
    }

    private static IEnumerable<(SyntaxType t1Type, string t1Text,
        SyntaxType seperatorKind, string seperatorText,
        SyntaxType t2Type, string t2Text)> GetTokensPairsWithSpaces()
    {
        foreach ((SyntaxType type, string text) t1 in GetTokens())
        {
            foreach ((SyntaxType type, string text) t2 in GetTokens())
            {
                if (RequiresSeparator(t1.type, t2.type))
                {
                    foreach ((SyntaxType type, string text) separator in GetSeparators())
                    {
                        yield return (t1.type, t1.text, separator.type, separator.text, t2.type, t2.text);
                    }
                }
            }
        }
    }

    private static bool RequiresSeparator(SyntaxType t1Type, SyntaxType t2Type)
    {
        bool t1IsKeyword = t1Type.ToString().EndsWith("Keyword");
        bool t2IsKeyword = t2Type.ToString().EndsWith("Keyword");

        if (t1IsKeyword && t2IsKeyword)
        {
            return true;
        }

        if (t1IsKeyword && t2Type == SyntaxType.IdentifierToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.IdentifierToken && t2IsKeyword)
        {
            return true;
        }

        if (t1Type == SyntaxType.IdentifierToken && t2Type == SyntaxType.IdentifierToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.NumberToken && t2Type == SyntaxType.IdentifierToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.IdentifierToken && t2Type == SyntaxType.NumberToken)
        {
            return true;
        }

        if (t1IsKeyword && t2Type == SyntaxType.NumberToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.NumberToken && t2Type == SyntaxType.NumberToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.StarToken && t2Type == SyntaxType.StarToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.StarToken && t2Type == SyntaxType.DoubleStarToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.StarToken && t2Type == SyntaxType.DoubleStarEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.StarToken && t2Type == SyntaxType.StarEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.BangToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.BangToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.PercentToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.PercentToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.AndToken && t2Type == SyntaxType.AndToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.AndToken && t2Type == SyntaxType.DoubleAndToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.AndToken && t2Type == SyntaxType.AndEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.AndToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.AndToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.StarToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.StarToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.PlusToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.PlusToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.MinusToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.MinusToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.SlashToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.SlashToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.LessToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.LessToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.LessToken && t2Type == SyntaxType.DoubleLessToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.LessToken && t2Type == SyntaxType.LessEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.LessToken && t2Type == SyntaxType.LessToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.EqualsToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.EqualsToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.GreaterToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.GreaterToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.GreaterToken && t2Type == SyntaxType.DoubleGreaterToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.GreaterToken && t2Type == SyntaxType.GreaterEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.GreaterToken && t2Type == SyntaxType.GreaterToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.HatToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.HatToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.SplitToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.SplitToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }


        if (t1Type == SyntaxType.SplitToken && t2Type == SyntaxType.SplitToken)
        {
            return true;
        }


        if (t1Type == SyntaxType.SplitToken && t2Type == SyntaxType.SplitEqualsToken)
        {
            return true;
        }


        if (t1Type == SyntaxType.SplitToken && t2Type == SyntaxType.DoubleSplitToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.DoubleStarToken && t2Type == SyntaxType.EqualsToken)
        {
            return true;
        }

        if (t1Type == SyntaxType.DoubleStarToken && t2Type == SyntaxType.DoubleEqualsToken)
        {
            return true;
        }


        return false;
    }
}