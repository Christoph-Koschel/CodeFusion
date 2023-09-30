using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Memory.Symbols;
using IllusionScript.Runtime.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace IllusionScript.Runtime.Test.Interpreting;

public class InterpreterTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public InterpreterTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("1;", 1)]
    [InlineData("+1;", 1)]
    [InlineData("-1;", -1)]
    [InlineData("~1;", -2)]
    [InlineData("14 + 12;", 26)]
    [InlineData("12 - 3;", 9)]
    [InlineData("4 * 2;", 8)]
    [InlineData("9 / 3;", 3)]
    [InlineData("(10);", 10)]
    [InlineData("12 == 3;", false)]
    [InlineData("3 == 3;", true)]
    [InlineData("12 != 3;", true)]
    [InlineData("3 != 3;", false)]
    [InlineData("3 < 4;", true)]
    [InlineData("5 < 4;", false)]
    [InlineData("50 << 2;", 200)]
    [InlineData("2 << 1;", 4)]
    [InlineData("4 <= 4;", true)]
    [InlineData("4 <= 5;", true)]
    [InlineData("5 <= 4;", false)]
    [InlineData("4 > 3;", true)]
    [InlineData("4 > 5;", false)]
    [InlineData("1 >> 45;", 0)]
    [InlineData("50 >> 2;", 12)]
    [InlineData("4 >= 4;", true)]
    [InlineData("5 >= 4;", true)]
    [InlineData("4 >= 5;", false)]
    [InlineData("1 | 2;", 3)]
    [InlineData("1 | 0;", 1)]
    [InlineData("1 & 3;", 1)]
    [InlineData("1 & 0;", 0)]
    [InlineData("1 ^ 0;", 1)]
    [InlineData("0 ^ 1;", 1)]
    [InlineData("1 ^ 3;", 2)]
    [InlineData("false == false;", true)]
    [InlineData("true == false;", false)]
    [InlineData("false != false;", false)]
    [InlineData("true != false;", true)]
    [InlineData("true && true;", true)]
    [InlineData("false || false;", false)]
    [InlineData("false | false;", false)]
    [InlineData("false | true;", true)]
    [InlineData("true | false;", true)]
    [InlineData("true | true;", true)]
    [InlineData("false & false;", false)]
    [InlineData("false & true;", false)]
    [InlineData("true & false;", false)]
    [InlineData("true & true;", true)]
    [InlineData("false ^ false;", false)]
    [InlineData("true ^ false;", true)]
    [InlineData("false ^ true;", true)]
    [InlineData("true ^ true;", false)]
    [InlineData("true;", true)]
    [InlineData("false;", false)]
    [InlineData("!true;", false)]
    [InlineData("!false;", true)]
    [InlineData("let a: i64 = 10; a = 10 * a; return a;", 100)]
    [InlineData("let a: i64 = 0; if (a == 0) a = 10; return a;", 10)]
    [InlineData("let a: i64 = 0; if (a == 4) a = 10;", 0)]
    [InlineData("let a: i64 = 0; if (a == 0) a = 10; else a = 5; return a;", 10)]
    [InlineData("let a: i64 = 0; if (a == 4) a = 10; else a = 5; return a;", 5)]
    [InlineData("let a: i64 = 0; while (a != 4) a = a + 1; return a;", 4)]
    [InlineData("let a: i64 = 0; for (i = 1 to 10) { a = a + i; } return a;", 45)]
    [InlineData("let a: i64 = 0; do a = a + 1; while (a < 10); return a;", 10)]
    [InlineData("let i: i64 = 0; while (i < 5) { i = i + 1; if (i == 5) continue; } return i;", 5)]
    [InlineData("let i: i64 = 0; do { i = i + 1; if (i == 5) continue; } while (i < 5); i; return i;", 5)]
    public void InterpreterComputesCorrectValues(string text, object expectedValue)
    {
        testOutputHelper.WriteLine(text);
        AssertValue(text, expectedValue);
    }

    private static void AssertValue(string text, object expectedValue)
    {
        SyntaxTree syntaxThree = SyntaxTree.Parse(text);

        // TODO Does not work any more
        //   Cannot interpret any more the result so whe need to find alternative to fix this maybe use CodeFusion.Image
        //   as a library and compile the statements directly to ASM


        // Compilation compilation = Compilation.CreateScript(null, syntaxThree);
        // Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();
        // InterpreterResult result = compilation.Interpret(variables);
        //
        // Assert.Empty(result.diagnostics);
        // Assert.Equal(expectedValue, result.value);
    }

    [Fact]
    public void InterpreterVariableDeclaration()
    {
        string text = @"
                {
                    let x: i64 = 10;
                    let y: i64 = 100;
                    {
                        let x: i64 = 10;
                    }
                    let [x]: i64 = 5;
                }
            ";

        string diagnostics = @"
                ERROR: Symbol 'x' is already declared
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpreterVariableNameExpression()
    {
        string text = @"[x] * 10;";

        string diagnostics = @"
                ERROR: Variable 'x' doesnt exist
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpreterVariableAssignmentExpression()
    {
        string text = @"[x] = 10;";

        string diagnostics = @"
                ERROR: Variable 'x' doesnt exist
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpreterVariableConstAssignmentExpression()
    {
        string text = @"
                {
                    const x: i64 = 10;
                    [x] = 0;
                }
            ";

        string diagnostics = @"
                ERROR: Variable 'x' is read-ony and cannot be assigned to
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpreterUnaryUndefinedOperator()
    {
        string text = @"[+]true;";

        string diagnostics = @"
                ERROR: Unary operator '+' is not defined for type boolean
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpreterBinaryUndefinedOperator()
    {
        string text = @"10 [+] true;";

        string diagnostics = @"
                ERROR: Binary operator '+' is not defined for type i64 and boolean
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpreterNoInfiniteLoop()
    {
        string text = @"
            {
            [)][][]
            ";

        string diagnostics = @"
                ERROR: Unexpected token <RParenToken>, expected <IdentifierToken>
                ERROR: Unexpected token <RParenToken>, expected <SemicolonToken>
                ERROR: Unexpected token <EOFToken>, expected <RBraceToken>
            ";
        AssertDiagnostics(text, diagnostics, false);
    }

    [Fact]
    public void InterpreterErrorForInsertedToken()
    {
        string text = @"1 + [;]";

        string diagnostics = @"
              ERROR: Unexpected token <SemicolonToken>, expected <IdentifierToken>
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpretInvokeFunctionArgumentsMissing()
    {
        string text = @"
                print([)];
            ";

        string diagnostics = @"
                ERROR: Function 'print' requires 1 arguments but was given 0
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void InterpretInvokeFunctionArgumentsExceeding()
    {
        string text = @"
                print(""Hello""[, "" "", "" world!""]);
            ";

        string diagnostics = @"
                ERROR: Function 'print' requires 1 arguments but was given 3
            ";

        AssertDiagnostics(text, diagnostics);
    }


    private void AssertDiagnostics(string text, string diagnosticsText, bool checkSpans = true)
    {
        text = $"define print(a: object): void {{}} define test(): object {{{text}}}";
        AnnotatedText annotatedText = AnnotatedText.Parse(text);
        testOutputHelper.WriteLine(annotatedText.text);
        SyntaxTree syntaxTree = SyntaxTree.Parse(annotatedText.text);
        Compilation compilation = Compilation.Create(syntaxTree);
        ImmutableArray<Diagnostic> result = compilation.Check();

        string[] diagnostics = AnnotatedText.UnindentLines(diagnosticsText);
        if (annotatedText.spans.Length != diagnostics.Length)
        {
            throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");
        }

        foreach (Diagnostic diagnostic in result)
        {
            testOutputHelper.WriteLine(diagnostic.message);
            testOutputHelper.WriteLine(diagnostic.location.span.ToString());
        }

        Assert.Equal(diagnostics.Length, result.Length);

        for (int i = 0; i < diagnostics.Length; i++)
        {
            string expectedMessage = diagnostics[i];
            string actualMessage = result[i].message;

            Assert.Equal(expectedMessage, actualMessage);

            TextSpan expectedSpan = annotatedText.spans[i];
            TextSpan actualSpan = result[i].location.span;

            if (checkSpans)
            {
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}