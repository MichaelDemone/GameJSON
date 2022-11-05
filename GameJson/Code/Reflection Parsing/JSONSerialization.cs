using GameJSON.ManualParsing;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameJSON.ReflectionParsing
{
    public class SerializePropertyAttribute : Attribute { }
    public class JSONIgnoreAttribute : Attribute { }

    public interface IJSONSerialize
    {
        void Serialize(object value, JSONWriter writer, JSONSettings settings);
    }

    public interface IJSONDeserialize
    {
        object Deserialize(JSONReader reader, JSONSettings settings);
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
}
