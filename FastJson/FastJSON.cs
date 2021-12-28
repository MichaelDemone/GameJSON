using System;
using System.Text;

namespace FastJson
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
    public class FastJSONReader
    {
        private const int LAST_ASCII_WHITESPACE = 32;

        public int Position = 0;
        public string Json;

        public char CurrentChar => Json[Position];
        public string RestOfJson => Json.Substring(Position, Json.Length - Position);

        public FastJSONReader(string json)
        {
            Json = json;
        }

        public bool ConsumeIfProperyNameEquals(string property)
        {
            int position = Position;
            ExpectAt('"', position++);

            int propIndex = 0;
            while (propIndex < property.Length && position < Json.Length-1) 
            { 
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
            if(IsNullToken()) {
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
                    if (Json[Position] == '\"')        smallStringBuilder.Append('\"');
                    else if (Json[Position] == '\\')   smallStringBuilder.Append('\\');
                    else if (Json[Position] == '/')    smallStringBuilder.Append('/');
                    else if (Json[Position] == 'b')    smallStringBuilder.Append('\b');
                    else if (Json[Position] == 'f')    smallStringBuilder.Append('\f');
                    else if (Json[Position] == 'n')    smallStringBuilder.Append('\n');
                    else if (Json[Position] == 'r')    smallStringBuilder.Append('\r');
                    else if (Json[Position] == 't')    smallStringBuilder.Append('\t');
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

        public char ConsumeCharValue() 
        {
            Expect('"');
            char c = CurrentChar;
            Position++;
            Expect('"');
            Accept(',');
            return c;
        }

        public DateTime ConsumeDateTime() 
        {
            throw new NotImplementedException();
        }

        #region Consume Unknowns
        public void ConsumeUnknownValue()
        {
            if(IsNullToken()) {
                ConsumeNull();
            }
            else if(Json[Position] == '{')
            {
                ConsumeUnknownObject();
            }
            else if(Json[Position] == '[')
            {
                ConsumeUnknownArray();
            }
            else if(Json[Position] == '"')
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
            while(Json[Position] != ']')
            {
                ConsumeUnknownValue();
            }
            Expect(']');
            Accept(',');
        }

        public void ConsumeUnknownObject()
        {
            if(IsNullToken())
            {
                ConsumeNull();
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

        public bool IsNullToken() 
        {
            return Json[Position] == 'n' || Json[Position] == 'N';
        }

        public void ConsumeNull() 
        {
            ExpectAny('n', 'N');
            ExpectAny('u', 'U');
            ExpectAny('l', 'L');
            ExpectAny('l', 'L');
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

        public void ConsumeUnknownStringValue()
        {
            Expect('"');
            while(Json[Position] != '"')
            {
                Position++;
                if (Json[Position] == '\\') Position++; // Skip escaped character in case it's "
            }
            Expect('"');
            Accept(',');
        }
        #endregion

        #region JSON Expects
        public void ExpectObjectStart() {
            Expect('{');
        }

        public bool IsAtObjectEnd() {
            return CurrentChar == '}';
        }

        public bool IsAtArrayEnd() {
            return CurrentChar == ']';
        }

        public int GetArrayLength() {
            int length = 0;
            int originalPosition = Position;
            while(!IsAtArrayEnd()) {
                ConsumeUnknownValue();
                length++;
            }
            Position = originalPosition;
            return length;
        }

        public void ExpectObjectEnd() {
            Expect('}');
            Accept(',');
        }

        public void ExpectArrayStart() {
            Expect('[');
        }

        public void ExpectArrayEnd() {
            Expect(']');
            Accept(',');
        }
        #endregion

        #region Predictive Parsing
        private void Expect(bool expect)
        {
            System.Diagnostics.Debug.Assert(expect);
        }

        private void Expect(char c)
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

        private void Accept(char c)
        {
            if(Position == Json.Length) return;

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

    public class FastJSONWriter 
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
        StringBuilder sb = new StringBuilder();
        int nesting = 0;
        private Token lastToken;

        private System.Collections.Generic.Stack<Token> requiredTokens = new System.Collections.Generic.Stack<Token>(50);

        public void BeginObject() 
        {
            requiredTokens.Push(Token.EndObject);

            Add('{');
            nesting++;
            lastToken = Token.BeginObject;
        }

        public void EndObject() 
        {
            Token requiredToken = requiredTokens.Pop();
            if(requiredToken != Token.EndObject) {
                throw new Exception($"Was trying to end an object but needed a {requiredToken}");
            }

            nesting--;
            Add('\n');
            AddIndents();
            Add('}');
            lastToken = Token.EndObject;
        }

        public void BeginArray() 
        {
            requiredTokens.Push(Token.EndArray);
            Add('[');
            nesting++;
            lastToken = Token.BeginArray;
        }

        public void EndArray() 
        {
            Token requiredToken = requiredTokens.Pop();
            if(requiredToken != Token.EndArray) {
                throw new Exception($"Was trying to end an array but needed a {requiredToken}");
            }

            nesting--;
            Add('\n');
            AddIndents();
            Add(']');
            lastToken = Token.EndArray;
        }

        public void StartArrayValue() {
            if(lastToken == Token.EndArrayValue) {
                Add(',');
            }

            Add('\n');
            AddIndents();

            requiredTokens.Push(Token.EndArrayValue);
        }

        public void WriteArrayValue(string value) 
        {
            StartArrayValue();
            Add('"');
            AddEscaped(value);
            Add('"');
            EndArrayValue();
        }

        public void WriteArrayValue(bool value) => WriteArrayRawString(value ? "true" : "false");

        public void WriteArrayValue(double d) => WriteArrayRawString(d.ToString());

        public void WriteArrayRawString(string value) {
            StartArrayValue();
            Add(value);
            EndArrayValue();
        }

        public void EndArrayValue() 
        {
            Token requiredToken = requiredTokens.Pop();
            if(requiredToken != Token.EndArrayValue) {
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
        
        public void WritePropertyRawString(string name, string value) {
            BeginProperty(name);
            AddEscaped(value);
            EndProperty();
        }

        public void BeginProperty(string name) {
            requiredTokens.Push(Token.EndProperty);
            if(lastToken == Token.EndProperty) {
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

        public void EndProperty() {
            Token requiredToken = requiredTokens.Pop();
            if(requiredToken != Token.EndProperty) {
                throw new Exception($"Was trying to end a property but needed a {requiredToken}");
            }
            lastToken = Token.EndProperty;
        }

        public void RawWrite(string s) {
            Add(s);
        }

        public void RawWriteEscapedString(string s)
        {
            Add('"');
            AddEscaped(s);
            Add('"');
        }

        public void RawWrite(bool b) {
            Add(b ? "true" : "false");
        }

        public void RawWrite(double d) {
            Add(d.ToString());
        }

        public string GetJSON() 
        {
            return sb.ToString();
        }

        private void AddIndents() 
        {
            for(int i = 0; i < nesting; i++) {
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
            for(int i = 0; i < s.Length; i++)
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

    public static class InPlaceParsing
    {
        public static double ParseDouble(string toParse, int start, int length)
        {
            bool negative = false;
            if(toParse[start] == '-') {
                negative = true;
                start++;
                length--;
            }

            int exponent = 0;
            {
                for(int i = start; i < start+length; i++) 
                {
                    if(toParse[i] == 'e' || toParse[i] == 'E') 
                    {
                        int eIndex = i;
                        bool expNegative = false;
                        if(toParse[i+1] == '+') 
                        {
                            i++;
                        } 
                        else if (toParse[i+1] == '-') 
                        {
                            expNegative = true;
                            i++;
                        }
                        
                        int tensSlot = 1;
                        for(int exponentIndex = start+length-1; exponentIndex > i; exponentIndex--) 
                        {
                            int val = toParse[exponentIndex] - '0';
                            exponent += val*tensSlot;
                            tensSlot *= 10;
                        }

                        if(expNegative) 
                        {
                            exponent *= -1;
                        }

                        length = eIndex - start;
                        break;
                    }
                }
            }

            if(toParse[start] < '0' || toParse[start] > '9') 
            {
                throw new Exception($"Expected first character to be either a - or 1 to 9 and got {toParse[start]}");
            }

            int decimalPointPos = start + length; 
            {
                for(int i = start; i < start+length; i++) 
                {
                    if(toParse[i] == '.') 
                    {
                        decimalPointPos = i;
                        break;
                    }
                }
            }

            double value = 0;

            // Calculate values above decimal place
            {
                double tensSlot = 1;
                for(int i = decimalPointPos-1; i >= start; i--) 
                {
                    double slotVal = (double) (toParse[i] - '0');
                    value += tensSlot * slotVal;
                    tensSlot *= 10d;
                }
            }

            // Calculate values below decimal place
            {
                double tensSlot = 0.1d;
                for(int i = decimalPointPos+1; i < start+length; i++) 
                {
                    double slotVal = (double) (toParse[i] - '0');
                    value += tensSlot * slotVal;
                    tensSlot /= 10d;
                }
            }
            
            value *= Math.Pow(10, exponent);
            if(negative) 
            {
                value *= -1;
            } 
            return value;
        }
    }
}
