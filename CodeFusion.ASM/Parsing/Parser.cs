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


    public CodeUnit ParseUnit(long addressOffset)
    {
        codeUnit = new CodeUnit(source, addressOffset);

        while(!atEnd) {
            Inst inst = ParseInst();
            if (inst.opcode != Opcode.NOP)
            {
                codeUnit.insts.Add(inst);
            }
        }

        foreach (KeyValuePair<long, Token> unresolved in codeUnit.unresolved)
        {
            if (codeUnit.labels.TryGetValue(unresolved.Value.text, out long value)) {
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
        if (atEnd) {
            return new Inst(Opcode.NOP);
        }

        if (current.type == TokenType.LBRACKET)
        {
            Match(TokenType.LBRACKET);
            Token intToken = Match(TokenType.INT);
            Match(TokenType.RBRACKET);
            Token label = Match(TokenType.IDENTIFIER);
            Match(TokenType.COLON);
            int poolValue = int.Parse(intToken.text);
            int labelValue = codeUnit.insts.Count;

            codeUnit.pool.Add(label.text, poolValue);
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
                int intValue = int.Parse(intToken.text);
                codeUnit.labels.Add(label.text, intValue);
                return ParseInst();
            }
            int labelValue = codeUnit.insts.Count;
            codeUnit.labels.Add(label.text, labelValue + codeUnit.addressOffset);
            return ParseInst();
        }

        byte opcode = GetOpcode(Match(TokenType.IDENTIFIER));
        if(Opcode.HasOperand(opcode)) {
            if (current.type == TokenType.INT) {
                Token intToken = Match(TokenType.INT);
                return new Inst(opcode, new Word(long.Parse(intToken.text)));
            } else {
                Token labelToken = Match(TokenType.IDENTIFIER);
                if (codeUnit.labels.TryGetValue(labelToken.text, out long value)) {
                    return new Inst(opcode, new Word(value));
                }
                codeUnit.unresolved.Add(codeUnit.insts.Count, labelToken);
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
        }

        Report.PrintReport(source, token, $"Undefined instruction '{token.text}'");
        return 0;
    }
}