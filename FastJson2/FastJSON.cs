using System;

namespace FastJson2
{

    // Goals:
    //  Fast deserialization (how fast?)
    //  Be able to serialize
    //  Give user full control (make implementation transparent)
    //  Zero allocations (unless required for object, like a string or an array)
    //  Support only a narrow definition of well formatted JSON in english
    //  Support large doubles and small doubles
    //  Read DoubleArray, BoolArray
    //  Objects must always be read manually
    //
    //  Usage Goals:
    //      Based on usage - Read properties in any order
    //      Done - Be able to ignore arrays or objects
    class FastJSON
    {
        private const int LAST_ASCII_WHITESPACE = 32;
        public const bool Debugging = true;

        public int Position = 0;
        public int CurrentTokenStart = -1;
        public int CurrentTokenEnd = -1;
        public string Json;

        public char CurrentChar => Json[Position];
        public string RestOfJson => Json.Substring(Position, Json.Length - Position);

        public FastJSON(string json)
        {
            Json = json;
        }

        public bool ConsumeIfProperyNameEquals(string property)
        {
            int position = Position;
            ExpectAt('"', position++);

            int propIndex = 0;
            while (propIndex < property.Length && position < Json.Length-1) { 
                if (property[propIndex++] != Json[position++])
                {
                    return false;
                }
            }
            
            if(propIndex != property.Length || Json[position] != '"')
            {
                return false;
            }

            Position = ++position;
            Expect(':');
            return true;

        }

        public double ConsumeDoubleValue()
        {
            int endOfFloat = Position;
            {
                while (endOfFloat < Json.Length &&
                    (
                        (Json[endOfFloat] >= '0' && Json[endOfFloat] <= '9') ||
                        Json[endOfFloat] == 'e' ||
                        Json[endOfFloat] == 'E' ||
                        Json[endOfFloat] == '+' ||
                        Json[endOfFloat] == '-' ||
                        Json[endOfFloat] == '.')
                    )
                {
                    endOfFloat++;
                }
            }

            double res = InPlaceParsing.ParseDouble(Json, Position, endOfFloat - Position);

            Position = endOfFloat;
            Accept(',');

            return res;
        }

        public string ConsumeStringValue()
        {
            int length = 0;
            Expect('"');
            int start = Position;
            while(Json[Position] != '"')
            {
                length++;
                Position++;
            }
            Expect('"');
            Accept(',');
            return Json.Substring(start, length);
        }

        public bool ConsumeBoolValue()
        {
            if(Json[Position] == 't' || Json[Position] == 'T')
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

        #region Consume Unknowns
        public void ConsumeUnknownValue()
        {
            if(Json[Position] == '{' || Json[Position] == 'n' || Json[Position] == 'N')
            {
                ConsumeUnknownObject();
            }
            else if(Json[Position] == '[')
            {
                ConsumeUnknownArray();
            }
            else if(Json[Position] == '"')
            {
                ConsumeStringValue(); // Caution: Allocates.
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
            while(Json[Position] != ']')
            {
                ConsumeUnknownValue();
            }
            Expect(']');
            Accept(',');
        }

        public void ConsumeUnknownObject()
        {
            if(Json[Position] == 'n' || Json[Position] == 'N')
            {
                ExpectAny('n', 'N');
                ExpectAny('u', 'U');
                ExpectAny('l', 'L');
                ExpectAny('l', 'L');
                Accept(',');
                return;
            }

            Expect('{');
            while(Json[Position] != '}')
            {
                ConsumePropertyName();
                ConsumeUnknownValue();
            }
            Expect('}');
            Accept(',');
        }

        public void ConsumePropertyName()
        {
            Expect('"');
            while(Json[Position++] != '"')
            {
            }
            Expect(':');
        }
        #endregion

        #region Predictive Parsing
        private void Expect(bool expect)
        {
            System.Diagnostics.Debug.Assert(expect);
        }

        public void Expect(char c)
        {
            SkipWhiteSpace();
            char actual = Json[Position];
            if(actual != c)
            {
                throw new Exception($"Expected {c} and got {actual}");
            }

            Position++;
            SkipWhiteSpace();
        }

        public void ExpectAny(char c1, char c2)
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

        public void ExpectAt(char c, int position)
        {
            char actual = Json[position];
            if (actual != c)
            {
                throw new Exception($"Expected {c} and got {actual}");
            }
        }

        public void Accept(char c)
        {
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

        public bool IsDone() => Position >= Json.Length;
    }

    static class InPlaceParsing
    {
        public static double ParseDouble(string toParse, int start, int length)
        {
            string doubleString = toParse.Substring(start, length);
            if (!double.TryParse(doubleString, out var res))
            {
                throw new Exception($"Unable to parse float {doubleString}");
            }
            return res;
        }
    }
}
