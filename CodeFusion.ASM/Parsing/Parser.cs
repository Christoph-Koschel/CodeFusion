using System;
using System.Collections.Generic;
using CodeFusion.ASM.Lexing;
using CodeFusion.VM;

namespace CodeFusion.ASM.Parsing;


class Parser
{
    private readonly SourceFile source;
    private readonly Token[] tokens;
    private int position;

    private CodeUnit codeUnit;

    public Parser(string path)
    {
        this.source = new SourceFile(path);
        this.position = 0;

        Lexer lexer = new Lexer(source);
        Token token;
        List<Token> tokens = new List<Token>();

        do
        {
            token = lexer.NextToken();
            if (token.type != TokenType.WHITESPACE && token.type != TokenType.BAD && token.type != TokenType.EOF)
            {
                tokens.Add(token);
            }
        } while (token.type != TokenType.EOF);

        this.tokens = tokens.ToArray();
    }

    private Token Peek(int offset = 0)
    {
        return position + offset >= tokens.Length ? tokens[tokens.Length - 1] : tokens[position + offset];
    }

    private Token current => Peek();
    private Token next => Peek(1);

    private bool atEnd => position >= tokens.Length;

    private Token Match(TokenType type)
    {
        if (current.type != type)
        {
            Report.PrintReport(source, current, $"Unexpected token '{current.type}' expected '{type}'");
            return new Token(type, "", current.span);
        }
        Token token = current;
        position++;
        return token;
    }


    public CodeUnit ParseUnit(ulong addressOffset)
    {
        codeUnit = new CodeUnit(source, addressOffset);

        while (!atEnd)
        {
            Inst inst = ParseInst();
            if (inst.opcode != Opcode.NOP)
            {
                codeUnit.insts.Add(inst);
            }
        }

        foreach (KeyValuePair<ulong, Token> unresolved in codeUnit.unresolved)
        {
            if (codeUnit.labels.TryGetValue(unresolved.Value.text, out ulong value))
            {
                Inst inst = codeUnit.insts[(int)unresolved.Key];
                inst.operand = new Word(value);
                codeUnit.insts[(int)unresolved.Key] = inst;
                codeUnit.unresolved.Remove(unresolved.Key);
            }
        }

        return codeUnit;
    }

    private Inst ParseInst()
    {
        if (atEnd)
        {
            return new Inst(Opcode.NOP);
        }

        if (current.type == TokenType.LBRACKET)
        {
            Match(TokenType.LBRACKET);
            Token intToken = Match(TokenType.INT);
            Match(TokenType.RBRACKET);
            Token label = Match(TokenType.IDENTIFIER);
            Match(TokenType.COLON);
            // Parsing with int to prevent the Overflow Exception
            int poolValue = int.Parse(intToken.text);
            uint labelValue = Convert.ToUInt32(codeUnit.insts.Count);

            codeUnit.pool.Add(new Word(labelValue + codeUnit.addressOffset), (ushort)poolValue);
            codeUnit.labels.Add(label.text, labelValue + codeUnit.addressOffset);

            return ParseInst();
        }

        if (current.type == TokenType.IDENTIFIER && next.type == TokenType.COLON)
        {
            Token label = Match(TokenType.IDENTIFIER);
            Match(TokenType.COLON);


            if (current.type == TokenType.INT)
            {
                Token intToken = Match(TokenType.INT);
                uint intValue = uint.Parse(intToken.text);
                codeUnit.variables.Add(label.text, intValue);
                return ParseInst();
            }
            uint labelValue = Convert.ToUInt32(codeUnit.insts.Count);
            codeUnit.labels.Add(label.text, labelValue + codeUnit.addressOffset);
            return ParseInst();
        }

        byte opcode = GetOpcode(Match(TokenType.IDENTIFIER));
        if (Opcode.HasOperand(opcode))
        {
            if (current.type == TokenType.INT)
            {
                Token intToken = Match(TokenType.INT);
                return new Inst(opcode, new Word(long.Parse(intToken.text)));
            }
            else
            {
                Token labelToken = Match(TokenType.IDENTIFIER);
                if (codeUnit.labels.TryGetValue(labelToken.text, out ulong value))
                {
                    codeUnit.lookups.Add(codeUnit.addressOffset + Convert.ToUInt32(codeUnit.insts.Count));
                    return new Inst(opcode, new Word(value));
                }
                if (codeUnit.variables.TryGetValue(labelToken.text, out value))
                {
                    return new Inst(opcode, new Word(value));
                }
                codeUnit.unresolved.Add(Convert.ToUInt64(codeUnit.insts.Count), labelToken);
            }
        }

        return new Inst(opcode);
    }

    private byte GetOpcode(Token token)
    {
        switch (token.text)
        {
            case "nop":
                return Opcode.NOP;
            case "push":
                return Opcode.PUSH;
            case "pop":
                return Opcode.POP;
            case "load":
                return Opcode.LOAD;
            case "store":
                return Opcode.STORE;
            case "mallocpool":
                return Opcode.MALLOC_POOL;
            case "freepool":
                return Opcode.FREE_POOL;
            case "pushptr":
                return Opcode.PUSH_PTR;
            case "loadptr":
                return Opcode.LOAD_PTR;
            case "storeptr":
                return Opcode.STORE_PTR;
            case "dup":
                return Opcode.DUP;
            case "pusharray":
                return Opcode.PUSH_ARRAY;
            case "loadarray":
                return Opcode.LOAD_ARRAY;
            case "storearray":
                return Opcode.STORE_ARRAY;
            case "iadd":
                return Opcode.IADD;
            case "fadd":
                return Opcode.FADD;
            case "uadd":
                return Opcode.UADD;
            case "isub":
                return Opcode.ISUB;
            case "fsub":
                return Opcode.FSUB;
            case "usub":
                return Opcode.USUB;
            case "imul":
                return Opcode.IMUL;
            case "fmul":
                return Opcode.FMUL;
            case "umul":
                return Opcode.UMUL;
            case "idiv":
                return Opcode.IDIV;
            case "fdiv":
                return Opcode.FDIV;
            case "udiv":
                return Opcode.UDIV;
            case "imod":
                return Opcode.IMOD;
            case "fmod":
                return Opcode.FMOD;
            case "umod":
                return Opcode.UMOD;

        }

        Report.PrintReport(source, token, $"Undefined instruction '{token.text}'");
        return 0;
    }
}