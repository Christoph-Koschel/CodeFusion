namespace CodeFusion.ASM.Lexing;

public class Lexer
{
    private SourceFile source;
    private int position;

    public Lexer(string path)
    {
        position = 0;
        source = new SourceFile(path);
    }

    public Lexer(SourceFile source) {
        this.position = 0;
        this.source = source;
    }

    public char Peek(int offset = 0)
    {
        return position + offset >= source.content.Length ? '\0' : source.content[position + offset];
    }

    private char current => Peek();
    private char next => Peek(1);

    public Token NextToken()
    {
        if (current == '\0')
        {
            return new Token(TokenType.EOF, "", new Span(position, position));
        }

        if (current == ':')
        {
            return new Token(TokenType.COLON, ":", new Span(position, ++position));
        }
        if (current == '[')
        {
            return new Token(TokenType.LBRACKET, "[", new Span(position, ++position));
        }
        if (current == ']')
        {
            return new Token(TokenType.RBRACKET, "]", new Span(position, ++position));
        }
        if (char.IsWhiteSpace(current))
        {
            int start = position;
            string text = "";
            while (char.IsWhiteSpace(current))
            {
                text += current;
                position++;
            }

            return new Token(TokenType.WHITESPACE, text, new Span(start, position));
        }
        if (char.IsDigit(current)) {
            int start = position;
            string text = "";
            while(char.IsDigit(current)) {
                text += current;
                position++;
            }

            return new Token(TokenType.INT, text, new Span(start, position));
        }
        if (char.IsLetter(current) || current == '_') {
            int start = position;
            string text = "";
            while(char.IsLetterOrDigit(current) || current == '_') {
                text += current;
                position++;
            }
            return new Token(TokenType.IDENTIFIER, text, new Span(start, position));
        }

        
        Token token = new Token(TokenType.BAD, current.ToString(), new Span(position, position + 1));
        Report.PrintReport(source, token, $"Bad character '{current}'");
        position++;
        return token;
    }
}
