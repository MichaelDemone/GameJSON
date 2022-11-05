using GameJSON.ManualParsing;

namespace GameJSON.ReflectionParsing
{
    public interface IJSONSerialize
    {
        void Serialize(object value, JSONWriter writer, JSONSettings settings);
    }

    public interface IJSONDeserialize
    {
        object Deserialize(JSONReader reader, JSONSettings settings);
    }
}
