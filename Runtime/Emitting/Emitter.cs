using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Binding.Nodes.Expressions;
using IllusionScript.Runtime.Binding.Nodes.Statements;
using IllusionScript.Runtime.Binding.Operators;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Emitting;

internal class Emitter
{
    private int poolSize;
    private string functionLabel;
    private readonly FunctionSymbol function;
    private readonly BoundBlockStatement body;
    private StringWriter writer;
    private const string INDENT = "    ";
    private Dictionary<VariableSymbol, int> pool;
    private static Dictionary<string, string> stringMemory = new Dictionary<string, string>();

    private Emitter(FunctionSymbol function, BoundBlockStatement body)
    {
        this.poolSize = 10; // 10 cause of the return and program-pool address
        this.functionLabel = function.name;
        this.function = function;
        this.body = body;
        this.pool = new Dictionary<VariableSymbol, int>();
    }


    private string Emit()
    {
        writer = new StringWriter();

        WriteInst("mallocpool", functionLabel);
        WriteInst("push", 8);
        WriteInst("store", 0);
        WriteInst("push", 2);
        WriteInst("store", 8);

        if (function.parameters.Length > 0)
        {
            foreach (ParameterSymbol parameter in function.parameters)
            {
                pool.Add(parameter, poolSize);
                poolSize += GetTypeSize(parameter.type);
            }
            foreach (ParameterSymbol parameter in function.parameters.Reverse())
            {
                WriteInst("push", GetTypeSize(parameter.type));
                WriteInst("store", pool[parameter]);
            }
        }

        foreach (BoundStatement statement in body.statements)
        {
            switch (statement.boundType)
            {
                case BoundNodeType.ExpressionStatement:
                    BoundExpressionStatement expressionStatement = (BoundExpressionStatement)statement;
                    EmitExpression(expressionStatement.expression);
                    if (expressionStatement.expression.type != TypeSymbol.@void)
                    {
                        WriteInst("pop");
                    }
                    break;
                case BoundNodeType.VariableDeclarationStatement:
                    BoundVariableDeclarationStatement variableDeclarationStatement = (BoundVariableDeclarationStatement)statement;
                    pool.Add(variableDeclarationStatement.variable, poolSize);
                    poolSize += GetTypeSize(variableDeclarationStatement.variable.type);
                    EmitExpression(variableDeclarationStatement.initializer);
                    WriteInst("push", GetTypeSize(variableDeclarationStatement.variable.type));
                    WriteInst("store", pool[variableDeclarationStatement.variable]);
                    break;
                case BoundNodeType.LabelStatement:
                    WriteLabel(((BoundLabelStatement)statement).BoundLabel.name);
                    break;
                case BoundNodeType.ConditionalGotoStatement:
                    BoundConditionalGotoStatement conditionalGotoStatement = (BoundConditionalGotoStatement)statement;
                    EmitExpression(conditionalGotoStatement.condition);
                    WriteInst(conditionalGotoStatement.jmpIfTrue ? "jmpnz" : "jmpz", conditionalGotoStatement.boundLabel.name);
                    break;
                case BoundNodeType.GotoStatement:
                    WriteInst("jmp", ((BoundGotoStatement)statement).BoundLabel.name);
                    break;
                case BoundNodeType.ReturnStatement:
                    BoundReturnStatement returnStatement = (BoundReturnStatement)statement;
                    if (returnStatement.expression != null)
                    {
                        EmitExpression(returnStatement.expression);
                    }
                    WriteInst("push", 2);
                    WriteInst("load", 8);
                    WriteInst("push", 8);
                    WriteInst("load", 0);
                    WriteInst("freepool");
                    WriteInst("ret");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        string emitted =
            "[" + poolSize + "] " + functionLabel + ":\n" +
            writer;

        return emitted;
    }

    private void EmitExpression(BoundExpression expression)
    {
        string type;

        switch (expression.boundType)
        {
            case BoundNodeType.LiteralExpression:
                BoundLiteralExpression literalExpression = (BoundLiteralExpression)expression;
                if (literalExpression.type == TypeSymbol.@string)
                {
                    string name = $"__string_{functionLabel}_{stringMemory.Count}";
                    stringMemory.Add(name, (string)literalExpression.value);
                    WriteInst("loadmemory", name);
                }
                else
                {
                    WriteInst("push", literalExpression.value);
                }
                break;
            case BoundNodeType.UnaryExpression:
                BoundUnaryExpression unaryExpression = (BoundUnaryExpression)expression;
                EmitExpression(unaryExpression.right);

                type = GetTypeChar(unaryExpression.unaryOperator.resultType);

                switch (unaryExpression.unaryOperator.operatorType)
                {
                    case BoundUnaryOperatorType.Identity:
                        break;
                    case BoundUnaryOperatorType.Negation:
                        WriteInst(type + "neg");
                        break;
                    case BoundUnaryOperatorType.LogicalNegation:
                        WriteInst("not");
                        break;
                    case BoundUnaryOperatorType.OnesComplement:
                        WriteInst("ones");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case BoundNodeType.BinaryExpression:
                BoundBinaryExpression binaryExpression = (BoundBinaryExpression)expression;
                EmitExpression(binaryExpression.left);
                EmitExpression(binaryExpression.right);

                type = GetTypeChar(binaryExpression.binaryOperator.resultType);


                switch (binaryExpression.binaryOperator.operatorType)
                {
                    case BoundBinaryOperatorType.Addition:
                        WriteInst(type + "add");
                        break;
                    case BoundBinaryOperatorType.Subtraction:
                        WriteInst(type + "sub");
                        break;
                    case BoundBinaryOperatorType.Multiplication:
                        WriteInst(type + "mul");
                        break;
                    case BoundBinaryOperatorType.Division:
                        WriteInst(type + "div");
                        break;
                    case BoundBinaryOperatorType.Modulo:
                        WriteInst(type + "mod");
                        break;
                    case BoundBinaryOperatorType.LogicalAnd:
                        WriteInst("and");
                        break;
                    case BoundBinaryOperatorType.LogicalOr:
                        WriteInst("or");
                        break;
                    case BoundBinaryOperatorType.NotEquals:
                        WriteInst("neq");
                        break;
                    case BoundBinaryOperatorType.Equals:
                        WriteInst("eq");
                        break;
                    case BoundBinaryOperatorType.BitwiseAnd:
                        WriteInst("and");
                        break;
                    case BoundBinaryOperatorType.BitwiseOr:
                        WriteInst("or");
                        break;
                    case BoundBinaryOperatorType.BitwiseXor:
                        WriteInst("xor");
                        break;
                    case BoundBinaryOperatorType.BitwiseShiftLeft:
                        WriteInst("lshift");
                        break;
                    case BoundBinaryOperatorType.BitwiseShiftRight:
                        WriteInst("rshift");
                        break;
                    case BoundBinaryOperatorType.Less:
                        WriteInst(type + "le");
                        break;
                    case BoundBinaryOperatorType.LessEquals:
                        WriteInst(type + "leq");
                        break;
                    case BoundBinaryOperatorType.Greater:
                        WriteInst(type + "ge");
                        break;
                    case BoundBinaryOperatorType.GreaterEquals:
                        WriteInst(type + "geq");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case BoundNodeType.VariableExpression:
                BoundVariableExpression variableExpression = (BoundVariableExpression)expression;
                WriteInst("push", GetTypeSize(variableExpression.variableSymbol.type));
                WriteInst("load", pool[variableExpression.variableSymbol]);
                break;
            case BoundNodeType.AssignmentExpression:
                BoundAssignmentExpression assignmentExpression = (BoundAssignmentExpression)expression;
                EmitExpression(assignmentExpression.expression);
                WriteInst("push", GetTypeSize(assignmentExpression.variableSymbol.type));
                WriteInst("store", pool[assignmentExpression.variableSymbol]);
                break;
            case BoundNodeType.CallExpression:
                BoundCallExpression callExpression = (BoundCallExpression)expression;
                foreach (BoundExpression argument in callExpression.arguments)
                {
                    EmitExpression(argument);
                }
                WriteInst("call", callExpression.function.name);
                break;
            case BoundNodeType.ConversionExpression:
                BoundConversionExpression conversionExpression = (BoundConversionExpression)expression;
                EmitExpression(conversionExpression.expression);
                TypeSymbol currentType = conversionExpression.expression.type;
                TypeSymbol targetType = conversionExpression.type;

                if (currentType.HasFlag(TypeSymbol.Attributes.INTEGER) && targetType.HasFlag(TypeSymbol.Attributes.INTEGER))
                {
                    if (currentType.size > targetType.size)
                    {
                        WriteInst("push", (long)Math.Pow(2, targetType.size * 8) - 1);
                        WriteInst("and");
                    }
                    return;
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER) && targetType.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER))
                {
                    if (currentType.size > targetType.size)
                    {
                        WriteInst("push", (long)Math.Pow(2, targetType.size * 8) - 1);
                        WriteInst("and");
                    }
                    return;
                }
                else
                {
                    // TODO Implemented float conversion
                    //   Implemented single precision to double precision and back maybe using a external method
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.INTEGER) && targetType.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER))
                {
                    WriteInst("itu");
                    return;
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.INTEGER) && targetType.HasFlag(TypeSymbol.Attributes.FLOAT))
                {
                    WriteInst("itf");
                    return;
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER) && targetType.HasFlag(TypeSymbol.Attributes.INTEGER))
                {
                    WriteInst("uti");
                    return;
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER) && targetType.HasFlag(TypeSymbol.Attributes.FLOAT))
                {
                    WriteInst("utf");
                    return;
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.FLOAT) && targetType.HasFlag(TypeSymbol.Attributes.INTEGER))
                {
                    WriteInst("fti");
                    return;
                }
                if (currentType.HasFlag(TypeSymbol.Attributes.FLOAT) && targetType.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER))
                {
                    WriteInst("ftu");
                    return;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private static string GetTypeChar(TypeSymbol typeSymbol)
    {
        string type = typeSymbol.HasFlag(TypeSymbol.Attributes.INTEGER)
            ? "i"
            : typeSymbol.HasFlag(TypeSymbol.Attributes.FLOAT)
                ? "f"
                : "u";
        return type;
    }

    private int GetTypeSize(TypeSymbol type)
    {
        return type.size;
    }

    private void WriteLabel(string label)
    {
        writer.WriteLine(label + ":");
    }

    private void WriteInst(string inst, string arg = null)
    {
        writer.Write(INDENT);
        writer.Write(inst);
        if (arg != null)
        {
            writer.Write(" ");
            writer.Write(arg);
        }
        writer.WriteLine();
    }

    private void WriteInst(string inst, int arg)
    {
        WriteInst(inst, arg.ToString());
    }

    private void WriteInst(string inst, long arg)
    {
        WriteInst(inst, arg.ToString());
    }

    private void WriteInst(string inst, object obj)
    {
        WriteInst(inst, obj.ToString());
    }

    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath)
    {
        stringMemory.Clear();

        if (program.diagnostics.Any())
        {
            return program.diagnostics.ToImmutableArray();
        }

        if (!Directory.Exists(Path.Combine(outputPath, ".cf")))
        {
            Directory.CreateDirectory(Path.Combine(outputPath, ".cf"));
        }

        List<string> asmFiles = new List<string>();
        asmFiles.Add(EmitPackage(program, outputPath, "main"));
        asmFiles.Add(EmitStaticMemory(outputPath));
        foreach (string nativeFile in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native")))
        {
            asmFiles.Add(nativeFile);
        }

        List<string> objFiles = new List<string>();
        objFiles.Add(RunCfAsm(outputPath, "crtbegin", asmFiles));
        RunCfBuilder(outputPath, objFiles);

        return ImmutableArray<Diagnostic>.Empty;
    }

    private static string EmitPackage(BoundProgram program, string outputPath, string packageName)
    {
        string packagePath = Path.Combine(outputPath, ".cf", packageName + ".cf");
        StreamWriter writer = new StreamWriter(packagePath);
        foreach (FunctionSymbol function in program.globalScope.functions)
        {
            if (function.declaration == null)
            {
                continue;
            }

            Emitter emitter = new Emitter(function, program.functionBodies[function]);
            string emitted = emitter.Emit();
            writer.WriteLine(emitted);
        }
        writer.Close();
        writer.Dispose();
        return packagePath;
    }

    private static string EmitStaticMemory(string outputPath)
    {
        string staticMemoryPath = Path.Combine(outputPath, ".cf", "__memory__.cf");

        StreamWriter writer = new StreamWriter(staticMemoryPath);
        writer.WriteLine("#memory\n");
        foreach ((string name, string value) in stringMemory)
        {
            writer.Write(name);
            writer.Write(": ");

            bool isOpen = false;
            foreach (char c in value)
            {
                if (c is '\t' or '\n' or '\r' or '\b' or '\0')
                {
                    if (isOpen)
                    {
                        writer.Write("\", ");
                        isOpen = false;
                    }

                    writer.Write(Encoding.ASCII.GetBytes(Convert.ToString(c))[0]);
                }
                else
                {
                    if (!isOpen)
                    {
                        writer.Write('"');
                        isOpen = true;
                    }
                    writer.Write(c);
                }
            }

            if (isOpen)
            {
                writer.Write('"');
            }

            writer.WriteLine(", 0");
        }

        writer.Close();
        writer.Dispose();
        return staticMemoryPath;
    }

    private static string RunCfAsm(string outputPath, string entryPoint, IEnumerable<string> files)
    {
#if DEBUG
        string processPath = "M:\\langs\\cs\\CodeFusion\\CodeFusion.ASM\\bin\\Debug\\net7.0\\CodeFusion.ASM.exe";
#else
        string processPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeFusion.ASM.exe");
#endif
        string outputName = Path.Combine(outputPath, ".cf", "program.bin");

        ProcessStartInfo processStartInfo = new ProcessStartInfo(processPath)
        {
            WorkingDirectory = outputPath,
        };
        processStartInfo.ArgumentList.Add("-t");
        processStartInfo.ArgumentList.Add("exe");
        processStartInfo.ArgumentList.Add("-o");
        processStartInfo.ArgumentList.Add(outputName);
        processStartInfo.ArgumentList.Add("-e");
        processStartInfo.ArgumentList.Add(entryPoint);
        foreach (string file in files)
        {
            Console.WriteLine(file);
            processStartInfo.ArgumentList.Add(file);
        }
        Process.Start(processStartInfo)?.WaitForExit();
        return outputName;
    }

    private static void RunCfBuilder(string outputPath, IEnumerable<string> files)
    {
#if DEBUG
        string processPath = "M:\\langs\\cs\\CodeFusion\\CodeFusion.Builder\\bin\\Debug\\net7.0\\CodeFusion.Builder.exe";
#else
        string processPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeFusion.Builder.exe");
#endif
        ProcessStartInfo processStartInfo = new ProcessStartInfo(processPath)
        {
            WorkingDirectory = outputPath,
        };
        processStartInfo.ArgumentList.Add("-t");
        processStartInfo.ArgumentList.Add("exe");
        foreach (string file in files)
        {
            Console.WriteLine(file);
            processStartInfo.ArgumentList.Add(file);
        }
        Process.Start(processStartInfo)?.WaitForExit();
    }
}