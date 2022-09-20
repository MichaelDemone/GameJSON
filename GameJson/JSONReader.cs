using GameJSON.ManualParsing.Utils;
using System;
using System.Text;

namespace GameJSON.ManualParsing
{
    public class JSONReader
    {
        private const int LAST_ASCII_WHITESPACE = 32;

        public int Position = 0;
        public string Json;

        public char CurrentChar => Json[Position];
        private string RestOfJson => Json.Substring(Position, Json.Length - Position);

        public JSONReader(string json)
        {
            Json = json;
        }

        public bool TryConsumeProperty(string propertyName)
        {
            int position = Position;
            if (!TryExpectAt('"', position++))
            {
                return false;
            }

            int propIndex = 0;
            while (propIndex < propertyName.Length && position < Json.Length - 1)
            {
                if (propertyName[propIndex++] != Json[position++])
                {
                    return false;
                }
            }

            if (propIndex != propertyName.Length || Json[position] != '"')
            {
                return false;
            }

            Position = ++position;
            Expect(':');
            return true;
        }

        public double ConsumeDoubleValue()
        {
            int endOfNum = Position;
            {
                while (endOfNum < Json.Length &&
                    (
                        (Json[endOfNum] >= '0' && Json[endOfNum] <= '9') ||
                        Json[endOfNum] == 'e' ||
                        Json[endOfNum] == 'E' ||
                        Json[endOfNum] == '+' ||
                        Json[endOfNum] == '-' ||
                        Json[endOfNum] == '.')
                    )
                {
                    endOfNum++;
                }
            }

            double res = InPlaceParsing.ParseDouble(Json, Position, endOfNum - Position);

            Position = endOfNum;
            Accept(',');

            return res;
        }

        StringBuilder smallStringBuilder = new StringBuilder();
        public string ConsumeStringValue()
        {
            if (IsNullToken())
            {
                ConsumeNull();
                return null;
            }
            Expect('"');
            // TODO(Perf): Could scan array first to figure out its size and then create string rather than reusing string builder.
            smallStringBuilder.Clear();

            while (Json[Position] != '"')
            {
                if (Json[Position] == '\\')
                {
                    Position++;
                    if (Json[Position] == '\"') smallStringBuilder.Append('\"');
                    else if (Json[Position] == '\\') smallStringBuilder.Append('\\');
                    else if (Json[Position] == '/') smallStringBuilder.Append('/');
                    else if (Json[Position] == 'b') smallStringBuilder.Append('\b');
                    else if (Json[Position] == 'f') smallStringBuilder.Append('\f');
                    else if (Json[Position] == 'n') smallStringBuilder.Append('\n');
                    else if (Json[Position] == 'r') smallStringBuilder.Append('\r');
                    else if (Json[Position] == 't') smallStringBuilder.Append('\t');
                    //else if Json[Position] == 'u')   smallStringBuilder.Append('\"'); // Don't do hex parsing, not sure what its use case is
                    else
                    {
                        smallStringBuilder.Append('\\');
                        smallStringBuilder.Append(Json[Position]);
                    }
                }
                else
                {
                    smallStringBuilder.Append(Json[Position]);
                }

                Position++;
            }
            Expect('"');
            Accept(',');
            return smallStringBuilder.ToString();
        }

        public bool ConsumeBoolValue()
        {
            if (Json[Position] == 't' || Json[Position] == 'T')
            {
                ExpectAny('t', 'T');
                ExpectAny('r', 'R');
                ExpectAny('u', 'U');
                ExpectAny('e', 'E');
                Accept(',');
                return true;
            }
            else if (Json[Position] == 'f' || Json[Position] == 'F')
            {
                ExpectAny('f', 'F');
                ExpectAny('a', 'A');
                ExpectAny('l', 'L');
                ExpectAny('s', 'S');
                ExpectAny('e', 'E');
                Accept(',');
                return false;
            }
            else
            {
                throw new Exception($"Expected t or f character for boolean and got {CurrentChar}");
            }
        }

        public char ConsumeCharValue()
        {
            Expect('"');
            char c = CurrentChar;
            Position++;
            Expect('"');
            Accept(',');
            return c;
        }

        public void ConsumeNull()
        {
            ExpectAny('n', 'N');
            ExpectAny('u', 'U');
            ExpectAny('l', 'L');
            ExpectAny('l', 'L');
            Accept(',');
        }

        #region Consume Unknowns
        public void ConsumePropertyName()
        {
            Expect('"');
            while (Json[Position++] != '"')
            {
            }
            Expect(':');
        }

        public void ConsumeUnknownValue()
        {
            if (IsNullToken())
            {
                ConsumeNull();
            }
            else if (Json[Position] == '{')
            {
                ConsumeUnknownObject();
            }
            else if (Json[Position] == '[')
            {
                ConsumeUnknownArray();
            }
            else if (Json[Position] == '"')
            {
                ConsumeUnknownStringValue();
            }
            else if (char.IsNumber(Json[Position]) || Json[Position] == '-' || Json[Position] == '+')
            {
                ConsumeDoubleValue();
            }
            else if (Json[Position] == 't' || Json[Position] == 'f' || Json[Position] == 'T' || Json[Position] == 'F')
            {
                ConsumeBoolValue();
            }
            else
            {
                throw new Exception($"Unsupported unknown value. Rest of JSON: {RestOfJson}");
            }
        }

        public void ConsumeUnknownArray()
        {
            Expect('[');
            while (Json[Position] != ']')
            {
                ConsumeUnknownValue();
            }
            Expect(']');
            Accept(',');
        }

        public void ConsumeUnknownObject()
        {
            if (IsNullToken())
            {
                ConsumeNull();
                return;
            }

            Expect('{');
            while (Json[Position] != '}')
            {
                ConsumePropertyName();
                ConsumeUnknownValue();
            }
            Expect('}');
            Accept(',');
        }

        public void ConsumeUnknownStringValue()
        {
            Expect('"');
            while (Json[Position] != '"')
            {
                Position++;
                if (Json[Position] == '\\') Position += 2; // Skip escaped character in case it's "
            }
            Expect('"');
            Accept(',');
        }
        #endregion

        #region JSON Expects
        public void ExpectObjectStart()
        {
            Expect('{');
        }

        public void ExpectObjectEnd()
        {
            Expect('}');
            Accept(',');
        }

        public void ExpectArrayStart()
        {
            Expect('[');
        }

        public void ExpectArrayEnd()
        {
            Expect(']');
            Accept(',');
        }
        #endregion

        #region JSON Queries
        public bool IsAtObjectEnd()
        {
            return CurrentChar == '}';
        }

        public bool IsAtArrayEnd()
        {
            return CurrentChar == ']';
        }

        public bool IsNullToken()
        {
            return Json[Position] == 'n' || Json[Position] == 'N';
        }

        public int GetArrayLength()
        {
            int length = 0;
            int originalPosition = Position;
            while (!IsAtArrayEnd())
            {
                ConsumeUnknownValue();
                length++;
            }
            Position = originalPosition;
            return length;
        }

        public bool IsDone() => Position >= Json.Length;
        #endregion

        #region Predictive Parsing
        private void Expect(char c)
        {
            SkipWhiteSpace();
            char actual = Json[Position];
            if (actual != c)
            {
                throw new Exception($"Expected {c} and got {actual}");
            }

            Position++;
            SkipWhiteSpace();
        }

        private void ExpectAny(char c1, char c2)
        {
            SkipWhiteSpace();
            char actual = Json[Position];
            if (actual != c1 && actual != c2)
            {
                throw new Exception($"Expected {c1} or {c2} and got {actual}");
            }

            Position++;
            SkipWhiteSpace();
        }

        private void ExpectAt(char c, int position)
        {
            char actual = Json[position];
            if (actual != c)
            {
                throw new Exception($"Expected {c} and got {actual}");
            }
        }

        private bool TryExpectAt(char c, int position)
        {
            char actual = Json[position];
            return actual == c;
        }

        private void Accept(char c)
        {
            if (Position == Json.Length) return;

            SkipWhiteSpace();
            char actual = Json[Position];
            if (actual != c)
            {
                return;
            }

            Position++;
            SkipWhiteSpace();
        }
        #endregion

        private void SkipWhiteSpace()
        {
            if (Position >= Json.Length) return;
            while (Json[Position] <= LAST_ASCII_WHITESPACE && ++Position < Json.Length) { }
        }
    }
}
