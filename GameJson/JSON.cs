using GameJSON.ManualParsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace GameJSON.ReflectionParsing
{
    public class SerializePropertyAttribute : Attribute {}

    public interface IJSONSerialize
    {
        public void Serialize(object value, JSONWriter writer, JSONSettings settings);
    }

    public interface IJSONDeserialize
    {
        public object Deserialize(JSONReader reader, JSONSettings settings);
    }

    public class JSONSettings
    {
        public IDictionary<Type, IJSONSerialize> CustomSerializers = null;
        public IDictionary<Type, IJSONDeserialize> CustomDeserializers = null;

        /// <summary>
        /// Auto properties (i.e. public int MyProperty { get; set; }) create private fields
        /// and if this is set to false, it won't serialize them, otherwise it will.
        /// Only applies if FieldFlags & BindingFlags.NonPublic != 0;
        /// </summary>
        public bool SerializeAutoPropertyFields = true;
        
        /// <summary>
        /// The flags passed into Type.GetFields when deciding what to serialize or deserialize.
        /// </summary>
        public BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        /// <summary>
        /// Attempt to serialize properties with SerializeProperty tag. Tag is not necessary for auto properties
        /// </summary>
        public bool AttemptToSerializePropertiesWithTag = false;
    }

    public class JSON
    {
        public static JSONSettings DefaultSettings = new JSONSettings();

        public static string Serialize(object obj, JSONSettings settings = null)
        {
            var writer = new JSONWriter();
            if(settings == null) settings = DefaultSettings;
            Serialize(obj, writer, settings);
            return writer.GetJSON();
        }

        public static void Serialize(object objVal, JSONWriter writer, JSONSettings settings = null) {
            if (settings == null) settings = DefaultSettings;

            if(objVal == null) 
            {
                writer.RawWrite("null");
            }
            else if (settings.CustomSerializers != null && settings.CustomSerializers.TryGetValue(objVal.GetType(), out var serializer))
            {
                serializer.Serialize(objVal, writer, settings);
            }
            else if (objVal is bool bo) 
            {
                writer.RawWrite(bo);
            }
            else if (objVal is string str) {
                writer.RawWriteEscapedString(str);
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
            else if (objVal is IList enumer) 
            {
                writer.BeginArray();
                {
                    foreach(var val in enumer) {
                        writer.BeginArrayValue();
                        Serialize(val, writer, settings);
                        writer.EndArrayValue();
                    }
                }
                writer.EndArray();
            }
            else {
                writer.BeginObject();
                {
                    Type objType = objVal.GetType();
                    foreach (var field in objType.GetFields(settings.FieldFlags))
                    {
                        string name = field.Name;
                        if(!settings.SerializeAutoPropertyFields && name.Contains("<"))
                        {
                            continue;
                        }
                        object fieldVal = field.GetValue(objVal);
                        writer.BeginProperty(name);
                        {
                            Serialize(fieldVal, writer, settings);
                        }
                        writer.EndProperty();
                    }

                    if(settings.AttemptToSerializePropertiesWithTag)
                    {
                        // Properties are just weird wrappers for methods, we shouldn't be trying to serializing them, but here we are.
                        var properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (var property in properties)
                        {
                            bool hasSerializePropertyAttribute = false;
                            foreach(var attr in property.CustomAttributes)
                            {
                                if(attr.AttributeType == typeof(SerializePropertyAttribute))
                                {
                                    hasSerializePropertyAttribute = true;
                                    break;
                                }
                            }
                            if (!hasSerializePropertyAttribute) continue;
                            if (property.SetMethod == null || property.GetMethod == null) continue;

                            string name = property.Name;
                            object propertyVal = property.GetValue(objVal);
                            writer.BeginProperty(name);
                            {
                                Serialize(propertyVal, writer, settings);
                            }
                            writer.EndProperty();
                        }
                    }
                }
                writer.EndObject();
            }
        }

        public static T Deserialize<T>(string s, JSONSettings settings = null) {
            var reader = new JSONReader(s);
            if(settings == null) settings = DefaultSettings;
            return (T) Deserialize(typeof(T), reader, settings);
        }

        private static readonly Type[] EmptyType = new Type[0];
        private static readonly object[] EmptyParamArray = new object[0];
        private static readonly PropertyInfo[] EmptyPropertyTypes = new PropertyInfo[0];
        private static readonly FieldInfo[] EmptyFieldTypes = new FieldInfo[0];

        public static object Deserialize(Type ttype, JSONReader reader, JSONSettings settings = null) {
            if (settings == null) settings = DefaultSettings;

            if (settings.CustomDeserializers != null && settings.CustomDeserializers.TryGetValue(ttype, out var customDeserializer))
            {
                return customDeserializer.Deserialize(reader, settings);
            }
            else if (reader.IsNullToken()) {
                reader.ConsumeNull();
                return null;
            }
            else if (ttype == typeof(bool)) {
                return reader.ConsumeBoolValue();
            }
            else if (ttype == typeof(string)) {
                return reader.ConsumeStringValue();
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
                    object result = constructor.Invoke(EmptyParamArray);
                    var addMethod = ttype.GetMethod("Add", new Type[] { elementType });

                    reader.ExpectArrayStart();
                    {
                        object[] param = new object[1];
                        for (int i = 0; !reader.IsAtArrayEnd(); i++)
                        {
                            var deserializedArrayObject = Deserialize(elementType, reader);
                            param[0] = deserializedArrayObject;
                            addMethod.Invoke(result, param);
                        }
                    }
                    reader.ExpectArrayEnd();
                    return result;
                }
                else {
                    object result = FormatterServices.GetUninitializedObject(ttype);
                    reader.ExpectObjectStart();
                    {
                        PropertyInfo[] properties = settings.AttemptToSerializePropertiesWithTag ? ttype.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : EmptyPropertyTypes;
                        FieldInfo[] fields = ttype.GetFields(settings.FieldFlags);

                        while(!reader.IsAtObjectEnd()) {
                            bool consumed = false;
                            foreach(var field in fields) {
                                if (!settings.SerializeAutoPropertyFields && field.Name.Contains("<")) continue;

                                if(reader.TryConsumeProperty(field.Name)) {
                                    var fieldValue = Deserialize(field.FieldType, reader);
                                    field.SetValue(result, fieldValue);
                                    consumed = true;
                                }
                            }

                            if(settings.AttemptToSerializePropertiesWithTag)
                            {
                                foreach (var property in properties)
                                {
                                    if (property.SetMethod == null) continue;
                                    bool hasSerializePropertyAttribute = false;
                                    foreach (var attr in property.CustomAttributes)
                                    {
                                        if (attr.AttributeType == typeof(SerializePropertyAttribute))
                                        {
                                            hasSerializePropertyAttribute = true;
                                            break;
                                        }
                                    }
                                    if (!hasSerializePropertyAttribute) continue;

                                    if (reader.TryConsumeProperty(property.Name))
                                    {
                                        var fieldValue = Deserialize(property.PropertyType, reader);
                                        property.SetValue(result, fieldValue);
                                        consumed = true;
                                    }
                                }
                            }

                            if (!consumed) {
                                reader.ConsumePropertyName();
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
