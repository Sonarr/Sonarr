using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration.Framework
{

    public class SqliteSyntaxReader
    {
        public String Buffer { get; private set; }
        public Int32 Index { get; private set; }

        private Int32 _previousIndex;

        public TokenType Type { get; private set; }
        public String Value { get; private set; }

        public String ValueToUpper
        {
            get { return Value.ToUpperInvariant(); }
        }

        public Boolean IsEndOfFile
        {
            get { return Index >= Buffer.Length; }
        }

        public enum TokenType
        {
            Start,
            Whitespace,
            End,
            ListStart,
            ListSeparator,
            ListEnd,
            Identifier,
            StringToken,
            StringLiteral,
            UnknownSymbol
        }

        public SqliteSyntaxReader(String sql)
        {
            Buffer = sql;
        }

        public void TrimBuffer()
        {
            Buffer = Buffer.Substring(Index);
            Index = 0;
            _previousIndex = 0;
        }

        public void SkipWhitespace()
        {
            while (!IsEndOfFile && char.IsWhiteSpace(Buffer[Index])) Index++;
        }

        public void SkipTillToken(TokenType tokenType)
        {
            if (IsEndOfFile)
                return;

            while (Read() != tokenType)
            {
                if (Type == TokenType.ListStart)
                    SkipTillToken(TokenType.ListEnd);
            }
        }

        public void Rollback()
        {
            Index = _previousIndex;
            Type = TokenType.Whitespace;
        }

        public TokenType Read()
        {
            if (!IsEndOfFile && char.IsWhiteSpace(Buffer[Index]))
            {
                Type = TokenType.Whitespace;
                SkipWhitespace();
            }

            _previousIndex = Index;

            if (IsEndOfFile)
            {
                Type = TokenType.End;
                return Type;
            }

            if (Buffer[Index] == '(')
            {
                Type = TokenType.ListStart;
                Value = null;
                Index++;
                return Type;
            }

            if (Buffer[Index] == ',')
            {
                Type = TokenType.ListSeparator;
                Value = null;
                Index++;
                return Type;
            }

            if (Buffer[Index] == ')')
            {
                Type = TokenType.ListEnd;
                Value = null;
                Index++;
                return Type;
            }

            if (Buffer[Index] == '\'')
            {
                Type = TokenType.StringLiteral;
                Value = ReadEscapedString('\'');
                return Type;
            }

            if (Buffer[Index] == '\"')
            {
                Type = TokenType.Identifier;
                Value = ReadEscapedString('\"');
                return Type;
            }

            if (Buffer[Index] == '`')
            {
                Type = TokenType.Identifier;
                Value = ReadEscapedString('`');
                return Type;
            }

            if (Buffer[Index] == '[')
            {
                Type = TokenType.Identifier;
                Value = ReadTerminatedString(']');
                return Type;
            }

            if (Type == TokenType.UnknownSymbol)
            {
                Value = Buffer[Index].ToString();
                Index++;
                return Type;
            }

            if (char.IsLetter(Buffer[Index]))
            {
                var start = Index;
                var end = start + 1;
                while (end < Buffer.Length && (char.IsLetter(Buffer[end]) || Buffer[end] == '_')) end++;
                if (end >= Buffer.Length || Buffer[end] == ',' || Buffer[end] == ')' || char.IsWhiteSpace(Buffer[end]))
                {
                    Index = end;
                }
                else if (Type == TokenType.UnknownSymbol)
                {
                    Value = Buffer[Index].ToString();
                    Index++;
                    return Type;
                }
                else
                {
                    throw CreateSyntaxException("Unexpected sequence.");
                }
                Type = TokenType.StringToken;
                Value = Buffer.Substring(start, end - start);
                return Type;
            }

            Type = TokenType.UnknownSymbol;
            Value = Buffer[Index].ToString();
            Index++;
            return Type;
        }

        public List<SqliteSyntaxReader> ReadList()
        {
            if (Type != TokenType.ListStart)
            {
                throw new InvalidOperationException();
            }

            var result = new List<SqliteSyntaxReader>();

            var start = Index;
            while (Read() != TokenType.ListEnd)
            {
                if (Type == TokenType.End)
                {
                    throw CreateSyntaxException("Expected ListEnd first");
                }
                if (Type == TokenType.ListStart)
                {
                    SkipTillToken(TokenType.ListEnd);
                }
                else if (Type == TokenType.ListSeparator)
                {
                    result.Add(new SqliteSyntaxReader(Buffer.Substring(start, Index - start - 1)));
                    start = Index;
                }
            }

            if (Index >= start + 1)
            {
                result.Add(new SqliteSyntaxReader(Buffer.Substring(start, Index - start - 1)));
            }

            return result;
        }

        protected String ReadTerminatedString(Char terminator)
        {
            var start = Index + 1;
            var end = Buffer.IndexOf(terminator, Index);

            if (end == -1) throw new SyntaxErrorException();

            Index = end + 1;
            return Buffer.Substring(start, end - start);
        }

        protected String ReadEscapedString(Char escape)
        {
            var identifier = new StringBuilder();

            while (true)
            {
                var start = Index + 1;
                var end = Buffer.IndexOf(escape, start);

                if (end == -1) throw new SyntaxErrorException();

                Index = end + 1;
                identifier.Append(Buffer.Substring(start, end - start));

                if (Buffer[Index] != escape) break;

                identifier.Append(escape);
            }

            return identifier.ToString();
        }

        public SyntaxErrorException CreateSyntaxException(String message, params object[] args)
        {
            return new SyntaxErrorException(String.Format("{0}. Syntax Error near: {1}", String.Format(message, args), Buffer.Substring(_previousIndex)));
        }
    }
}
