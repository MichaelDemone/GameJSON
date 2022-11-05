using System;
using System.Collections.Generic;
using System.Text;

namespace GameJSON.ManualParsing
{
    public class JSONWriter
    {
        enum Token
        {
            Begin,
            BeginObject,
            EndObject,
            BeginProperty,
            EndProperty,
            BeginArray,
            EndArray,
            BeginArrayValue,
            EndArrayValue,
        }

        private readonly Stack<Token> requiredTokens = new Stack<Token>(50);
        private readonly StringBuilder sb = new StringBuilder(100000);
        private Token lastToken= Token.Begin;
        private int tabNesting = 0;

        public void BeginObject()
        {
            requiredTokens.Push(Token.EndObject);

            Add('{');
            tabNesting++;
            lastToken = Token.BeginObject;
        }

        public void EndObject()
        {
            Token requiredToken = requiredTokens.Pop();
            if (requiredToken != Token.EndObject)
            {
                throw new Exception($"Was trying to end an object but needed a {requiredToken}");
            }

            tabNesting--;
            Add('\n');
            AddIndents();
            Add('}');
            lastToken = Token.EndObject;
        }

        public void BeginArray()
        {
            requiredTokens.Push(Token.EndArray);
            Add('[');
            tabNesting++;
            lastToken = Token.BeginArray;
        }

        public void EndArray()
        {
            Token requiredToken = requiredTokens.Pop();
            if (requiredToken != Token.EndArray)
            {
                throw new Exception($"Was trying to end an array but needed a {requiredToken}");
            }

            tabNesting--;
            Add('\n');
            AddIndents();
            Add(']');
            lastToken = Token.EndArray;
        }

        public void BeginArrayValue()
        {
            if (lastToken == Token.EndArrayValue)
            {
                Add(',');
            }

            Add('\n');
            AddIndents();

            requiredTokens.Push(Token.EndArrayValue);
        }

        public void WriteArrayValue(string value)
        {
            BeginArrayValue();
            Add('"');
            AddEscaped(value);
            Add('"');
            EndArrayValue();
        }

        public void WriteArrayValue(bool value) => WriteArrayRawString(value ? "true" : "false");

        public void WriteArrayValue(double d) => WriteArrayRawString(d.ToString());

        public void WriteArrayRawString(string value)
        {
            BeginArrayValue();
            Add(value);
            EndArrayValue();
        }

        public void EndArrayValue()
        {
            Token requiredToken = requiredTokens.Pop();
            if (requiredToken != Token.EndArrayValue)
            {
                throw new Exception($"Was trying to end an array value but needed a {requiredToken}");
            }

            lastToken = Token.EndArrayValue;
        }

        public void WriteProperty(string name, string value)
        {
            BeginProperty(name);
            Add('"');
            AddEscaped(value);
            Add('"');
            EndProperty();
        }

        public void WriteProperty(string name, double d) => WritePropertyRawString(name, d.ToString());
        public void WriteProperty(string name, bool val) => WritePropertyRawString(name, val ? "true" : "false");

        public void WritePropertyRawString(string name, string value)
        {
            BeginProperty(name);
            AddEscaped(value);
            EndProperty();
        }

        public void BeginProperty(string name)
        {
            requiredTokens.Push(Token.EndProperty);
            if (lastToken == Token.EndProperty)
            {
                Add(',');
            }

            Add('\n');
            AddIndents();
            Add('"');
            Add(name); // TODO: Name Validation
            Add('"');
            Add(':');
            Add(' ');

            lastToken = Token.BeginProperty;
        }

        public void EndProperty()
        {
            Token requiredToken = requiredTokens.Pop();
            if (requiredToken != Token.EndProperty)
            {
                throw new Exception($"Was trying to end a property but needed a {requiredToken}");
            }
            lastToken = Token.EndProperty;
        }

        public void RawWrite(string s)
        {
            Add(s);
        }

        public void RawWriteEscapedString(string s)
        {
            Add('"');
            AddEscaped(s);
            Add('"');
        }

        public void RawWrite(bool b)
        {
            Add(b ? "true" : "false");
        }

        public void RawWrite(double d)
        {
            Add(d.ToString());
        }

        public string GetJSON()
        {
            return sb.ToString();
        }

        private void AddIndents()
        {
            for (int i = 0; i < tabNesting; i++)
            {
                sb.Append('\t');
            }
        }

        private void Add(char c)
        {
            sb.Append(c);
        }

        private void Add(string s)
        {
            sb.Append(s);
        }

        private void AddEscaped(string s)
        {
            void Escape(char c)
            {
                sb.Append('\\');
                sb.Append(c);
            }
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\') Escape('\\');
                else if (s[i] == '"') Escape('"');
                else if (s[i] == '/') Escape('/');
                else if (s[i] == '\b') Escape('b');
                else if (s[i] == '\f') Escape('f');
                else if (s[i] == '\n') Escape('n');
                else if (s[i] == '\r') Escape('r');
                else if (s[i] == '\t') Escape('t');
                else
                {
                    sb.Append(s[i]);
                }
            }
        }
    }
}
