#nullable enable
using System;
using System.Collections.Generic;

namespace IllusionScript.Runtime.Parsing;

public class SyntaxFacts
{
    public static int GetBinaryOperatorPrecedence(SyntaxType type)
    {
        switch (type)
        {
            case SyntaxType.DoubleStarToken:
                return 7;
            case SyntaxType.StarToken:
            case SyntaxType.SlashToken:
            case SyntaxType.PercentToken:
                return 5;

            case SyntaxType.MinusToken:
            case SyntaxType.PlusToken:
                return 4;
            case SyntaxType.DoubleEqualsToken:
            case SyntaxType.BangEqualsToken:
            case SyntaxType.LessToken:
            case SyntaxType.LessEqualsToken:
            case SyntaxType.GreaterToken:
            case SyntaxType.GreaterEqualsToken:
                return 3;
            case SyntaxType.AndToken:
            case SyntaxType.DoubleAndToken:
                return 2;
            case SyntaxType.DoubleLessToken:
            case SyntaxType.DoubleGreaterToken:
            case SyntaxType.SplitToken:
            case SyntaxType.DoubleSplitToken:
            case SyntaxType.HatToken:
                return 1;
            default:
                return 0;
        }
    }

    public static int GetUnaryOperatorPrecedence(SyntaxType type)
    {
        switch (type)
        {
            case SyntaxType.MinusToken:
            case SyntaxType.PlusToken:
            case SyntaxType.BangToken:
            case SyntaxType.TildeToken:
                return 6;

            default:
                return 0;
        }
    }

    protected static SyntaxType GetKeywordType(string text)
    {
        switch (text)
        {
            case "true":
                return SyntaxType.TrueKeyword;
            case "false":
                return SyntaxType.FalseKeyword;
            case "while":
                return SyntaxType.WhileKeyword;
            case "for":
                return SyntaxType.ForKeyword;
            case "do":
                return SyntaxType.DoKeyword;
            case "to":
                return SyntaxType.ToKeyword;
            case "return":
                return SyntaxType.ReturnKeyword;
            case "break":
                return SyntaxType.BreakKeyword;
            case "continue":
                return SyntaxType.ContinueKeyword;
            case "if":
                return SyntaxType.IfKeyword;
            case "else":
                return SyntaxType.ElseKeyword;
            case "define":
                return SyntaxType.FunctionKeyword;
            case "let":
                return SyntaxType.LetKeyword;
            case "const":
                return SyntaxType.ConstKeyword;
            case "extern":
                return SyntaxType.ExternKeyword;
            default:
                return SyntaxType.IdentifierToken;
        }
    }

    public static IEnumerable<SyntaxType> GetUnaryOperators()
    {
        SyntaxType[] types = (SyntaxType[])Enum.GetValues(typeof(SyntaxType));
        foreach (SyntaxType type in types)
        {
            if (GetUnaryOperatorPrecedence(type) > 0)
                yield return type;
        }
    }

    public static IEnumerable<SyntaxType> GetBinaryOperators()
    {
        SyntaxType[] types = (SyntaxType[])Enum.GetValues(typeof(SyntaxType));
        foreach (SyntaxType type in types)
        {
            if (GetBinaryOperatorPrecedence(type) > 0)
                yield return type;
        }
    }

    public static string? GetText(SyntaxType type)
    {
        switch (type)
        {
            case SyntaxType.PlusToken:
                return "+";
            case SyntaxType.PlusEqualsToken:
                return "+=";
            case SyntaxType.MinusToken:
                return "-";
            case SyntaxType.MinusEqualsToken:
                return "-=";
            case SyntaxType.StarToken:
                return "*";
            case SyntaxType.StarEqualsToken:
                return "*=";
            case SyntaxType.SlashToken:
                return "/";
            case SyntaxType.SlashEqualsToken:
                return "/=";
            case SyntaxType.PercentToken:
                return "%";
            case SyntaxType.PercentEqualsToken:
                return "%=";
            case SyntaxType.DoubleStarToken:
                return "**";
            case SyntaxType.DoubleStarEqualsToken:
                return "**=";
            case SyntaxType.BangToken:
                return "!";
            case SyntaxType.EqualsToken:
                return "=";
            case SyntaxType.TildeToken:
                return "~";
            case SyntaxType.LessToken:
                return "<";
            case SyntaxType.LessEqualsToken:
                return "<=";
            case SyntaxType.DoubleLessToken:
                return "<<";
            case SyntaxType.GreaterToken:
                return ">";
            case SyntaxType.GreaterEqualsToken:
                return ">=";
            case SyntaxType.DoubleGreaterToken:
                return ">>";
            case SyntaxType.AndToken:
                return "&";
            case SyntaxType.DoubleAndToken:
                return "&&";
            case SyntaxType.AndEqualsToken:
                return "&=";
            case SyntaxType.SplitToken:
                return "|";
            case SyntaxType.SplitEqualsToken:
                return "|=";
            case SyntaxType.DoubleSplitToken:
                return "||";
            case SyntaxType.HatToken:
                return "^";
            case SyntaxType.HatEqualsToken:
                return "^=";
            case SyntaxType.DoubleEqualsToken:
                return "==";
            case SyntaxType.BangEqualsToken:
                return "!=";
            case SyntaxType.LParenToken:
                return "(";
            case SyntaxType.RParenToken:
                return ")";
            case SyntaxType.LBraceToken:
                return "{";
            case SyntaxType.RBraceToken:
                return "}";
            case SyntaxType.ColonToken:
                return ":";
            case SyntaxType.SemicolonToken:
                return ";";
            case SyntaxType.CommaToken:
                return ",";
            case SyntaxType.BreakKeyword:
                return "break";
            case SyntaxType.ContinueKeyword:
                return "continue";
            case SyntaxType.ElseKeyword:
                return "else";
            case SyntaxType.FalseKeyword:
                return "false";
            case SyntaxType.ForKeyword:
                return "for";
            case SyntaxType.FunctionKeyword:
                return "define";
            case SyntaxType.IfKeyword:
                return "if";
            case SyntaxType.ReturnKeyword:
                return "return";
            case SyntaxType.ToKeyword:
                return "to";
            case SyntaxType.TrueKeyword:
                return "true";
            case SyntaxType.LetKeyword:
                return "let";
            case SyntaxType.ConstKeyword:
                return "const";
            case SyntaxType.WhileKeyword:
                return "while";
            case SyntaxType.DoKeyword:
                return "do";
            case SyntaxType.ExternKeyword:
                return "extern";
            default:
                return null;
        }
    }
}