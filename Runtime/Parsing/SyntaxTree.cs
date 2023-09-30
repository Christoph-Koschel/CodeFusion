using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Parsing.Nodes;

namespace IllusionScript.Runtime.Parsing;

public class SyntaxTree
{
    private delegate void ParseHandler(SyntaxTree syntaxTree, out CompilationUnit root,
        out ImmutableArray<Diagnostic> diagnostics);

    public readonly SourceText text;
    public readonly CompilationUnit root;
    public readonly ImmutableArray<Diagnostic> diagnostics;

    public static SyntaxTree Load(string filename)
    {
        string text = File.ReadAllText(filename);
        SourceText sourceText = SourceText.From(text, filename);
        return Parse(sourceText);
    }

    private SyntaxTree(SourceText text, ParseHandler handler)
    {
        this.text = text;

        handler(this, out CompilationUnit root, out ImmutableArray<Diagnostic> diagnostics);

        this.diagnostics = diagnostics;
        this.root = root;
    }

    private static void Parse(SyntaxTree syntaxTree, out CompilationUnit root,
        out ImmutableArray<Diagnostic> diagnostics)
    {
        Parser parser = new Parser(syntaxTree);
        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }

    public static SyntaxTree Parse(SourceText text)
    {
        return new SyntaxTree(text, Parse);
    }

    public static SyntaxTree Parse(string text, string filename = "")
    {
        SourceText sourceText = SourceText.From(text, filename);
        return Parse(sourceText);
    }

    public static ImmutableArray<Token> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
    {
        SourceText sourceText = SourceText.From(text);
        return ParseTokens(sourceText, out diagnostics);
    }


    public static ImmutableArray<Token> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics)
    {
        List<Token> tokens = new List<Token>();
        void ParseTokens(SyntaxTree syntaxTree, out CompilationUnit root,
            out ImmutableArray<Diagnostic> diagnostics)
        {
            root = null;
            Lexer l = new Lexer(syntaxTree);
            while (true)
            {
                Token token = l.Lex();
                if (token.type == SyntaxType.EOFToken)
                {
                    root = new CompilationUnit(syntaxTree, ImmutableArray<Member>.Empty, token);
                    break;
                }

                tokens.Add(token);
            }

            diagnostics = l.Diagnostics().ToImmutableArray();
        }

        SyntaxTree syntaxTree = new SyntaxTree(text, ParseTokens);
        diagnostics = syntaxTree.diagnostics;
        return tokens.ToImmutableArray();
    }
}