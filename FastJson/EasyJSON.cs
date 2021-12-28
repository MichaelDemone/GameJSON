using System;
using System.Collections;
using System.Collections.Generic;

namespace FastJson
{
    public class EasyJSON 
    {
        public static string Serialize(object obj)
        {
            FastJson.FastJSONWriter writer = new FastJson.FastJSONWriter();
            Serialize(obj, writer);
            return writer.GetJSON();
        }

        public static void Serialize(object objVal, FastJSONWriter writer) {

            if(objVal == null) 
            {
                writer.RawWrite("null");
            }
            else if (objVal is bool bo) 
            {
                writer.RawWrite(bo);
            }
            else if (objVal is string str) {
                writer.RawWrite("\"");
                writer.RawWrite(str);
                writer.RawWrite("\"");
            }
            else if (objVal is DateTime dt) {
                writer.RawWrite(dt.ToString());
            }
            else if (objVal is byte by) {
                writer.RawWrite((double) by);
            }
            else if (objVal is sbyte sb) {
                writer.RawWrite((double) sb);
            }
            else if (objVal is char c) {
                writer.RawWrite("\"");
                writer.RawWrite(new string(new char[] {c}));
                writer.RawWrite("\"");

            }
            else if (objVal is decimal de) {
                // TODO: Special handling?
                writer.RawWrite((double)de);
            }
            else if (objVal is double d) {
                writer.RawWrite(d);
            }
            else if (objVal is float f) {
                writer.RawWrite((double) f);
            }
            else if (objVal is int i) {
                writer.RawWrite((double) i);
            } 
            else if (objVal is uint ui) {
                writer.RawWrite((double) ui);
            }
            else if (objVal is long l) {
                // TODO: Loss of precision
                writer.RawWrite((double) l);
            }
            else if (objVal is ulong ul) {
                writer.RawWrite((double) ul);
            }
            else if (objVal is short s) {
                writer.RawWrite((double) s);
            }
            else if (objVal is ushort us) {
                writer.RawWrite((double) us);
            }
            else if (objVal is IEnumerable enumer) 
            {
                writer.BeginArray();
                {
                    foreach(var val in enumer) {
                        writer.StartArrayValue();
                        Serialize(val, writer);
                        writer.EndArrayValue();
                    }
                }
                writer.EndArray();
            }
            else {
                writer.BeginObject();
                {
                    Type objType = objVal.GetType();
                    foreach(var field in objType.GetFields()) 
                    {
                        string name = field.Name;
                        object fieldVal = field.GetValue(objVal);
                        writer.BeginProperty(name);
                        {
                            Serialize(fieldVal, writer);
                        }
                        writer.EndProperty();
                    }
                }
                writer.EndObject();
            }
        }
    
        public static T Deserialize<T>(string s) {
            FastJSONReader reader = new FastJSONReader(s);
            return (T) Deserialize(typeof(T), reader);
        }

        private static readonly Type[] EmptyType = new Type[0];
        private static readonly object[] EmptyParamArray = new object[0];

        public static object Deserialize(Type ttype, FastJSONReader reader) {

            if(reader.IsNullToken()) {
                reader.ConsumeNull();
                return null;
            }
            if (ttype == typeof(bool)) {
                return reader.ConsumeBoolValue();
            }
            else if (ttype == typeof(string)) {
                return reader.ConsumeStringValue();
            }
            else if (ttype == typeof(DateTime)) {
                return reader.ConsumeDateTime();
            }
            else if (ttype == typeof(byte))     return (byte)   reader.ConsumeDoubleValue();
            else if (ttype == typeof(sbyte))    return (sbyte)  reader.ConsumeDoubleValue();
            else if (ttype == typeof(float))    return (float)  reader.ConsumeDoubleValue();
            else if (ttype == typeof(double))   return (double) reader.ConsumeDoubleValue();
            else if (ttype == typeof(int))      return (int)    reader.ConsumeDoubleValue();
            else if (ttype == typeof(uint))     return (uint)   reader.ConsumeDoubleValue();
            else if (ttype == typeof(long))     return (long)   reader.ConsumeDoubleValue();
            else if (ttype == typeof(ulong))    return (ulong)  reader.ConsumeDoubleValue();
            else if (ttype == typeof(short))    return (short)  reader.ConsumeDoubleValue();
            else if (ttype == typeof(ushort))   return (ushort) reader.ConsumeDoubleValue();
            else if (ttype == typeof(char)) {
                return reader.ConsumeCharValue();
            }
            else if (ttype == typeof(decimal)) {
                return (decimal) reader.ConsumeDoubleValue();
            }
            else {
                Type listType = ttype.GetInterface(typeof(IList<>).Name);
                if (ttype.IsArray) {
                    Type elementType = ttype.GetElementType();

                    reader.ExpectArrayStart();
                    var array = Array.CreateInstance(elementType, reader.GetArrayLength());
                    {
                        for(int i = 0; !reader.IsAtArrayEnd(); i++) 
                        {
                            var deserializedArrayObject = Deserialize(elementType, reader);
                            array.SetValue(deserializedArrayObject, i);
                        }
                    }
                    reader.ExpectArrayEnd();

                    return array;
                }
                else if (listType != null)
                {
                    Type elementType = listType.GetGenericArguments()[0];
                    var constructor = ttype.GetConstructor(EmptyType);
                    IList result = (IList) constructor.Invoke(EmptyParamArray);

                    reader.ExpectArrayStart();
                    {
                        for (int i = 0; !reader.IsAtArrayEnd(); i++)
                        {
                            var deserializedArrayObject = Deserialize(elementType, reader);
                            result.Add(deserializedArrayObject);
                        }
                    }
                    reader.ExpectArrayEnd();
                    return result;
                }
                else {
                    object result = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(ttype);
                    reader.ExpectObjectStart();
                    {
                        System.Reflection.FieldInfo[] fields = ttype.GetFields();
                        while(!reader.IsAtObjectEnd()) {
                            bool consumed = false;
                            foreach(var field in fields) {
                                if(reader.ConsumeIfProperyNameEquals(field.Name)) {
                                    UnityEngine.Debug.Log($"Consuming: {field.Name}");
                                    var fieldValue = Deserialize(field.FieldType, reader);
                                    field.SetValue(result, fieldValue);
                                    consumed = true;
                                }
                            }

                            if(!consumed) {
                                reader.ConsumeUnknownValue();
                            }
                        }
                    }
                    reader.ExpectObjectEnd();
                    return result;
                }
            }
        }
    }
}
