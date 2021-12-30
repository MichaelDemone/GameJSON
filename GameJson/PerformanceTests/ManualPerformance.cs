using GameJSON.ManualParsing;
using GameJSON.ReflectionParsing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ManualPerformance : MonoBehaviour
{
    public void Start()
    {
        print("Starting manual cold test");
        RunSerializationTest();
        print("Starting manual hot test");
        RunSerializationTest();
    }

    [ContextMenu("RunTest")]
    public void RunSerializationTest()
    {
        List<TestPosition> testPositions = new List<TestPosition>();
        for (int i = 0; i < 1000; i++)
        {
            testPositions.Add(new TestPosition()
            {
                EntityName = $"Entity{i}",
                Position = new MyVector3(UnityEngine.Random.value * 100, UnityEngine.Random.value * 100, UnityEngine.Random.value * 100)
            });
        }
        GC.Collect();

        JSONSettings settings = new JSONSettings()
        {
            CustomSerializers = new Dictionary<Type, IJSONSerialize>()
                {
                    { typeof(List<TestPosition>), new ListTestPositionDeserialize() },
                },
            CustomDeserializers = new Dictionary<Type, IJSONDeserialize>()
                {
                    { typeof(List<TestPosition>), new ListTestPositionDeserialize() },
                }
        };

        Stopwatch sw = new Stopwatch();
        string gameJsonSerializeString;
        {
            
            sw.Start();


            gameJsonSerializeString = JSON.Serialize(testPositions, settings);

            sw.Stop();
            print($"Manual parsing took {sw.ElapsedMilliseconds}ms");
            //print(gameJsonSerializeString);

            sw.Reset();
            GC.Collect();
        }

        List<TestPosition> gameJsonDeserializationResult;
        {
            sw.Start();

            gameJsonDeserializationResult = JSON.Deserialize<List<TestPosition>>(gameJsonSerializeString, settings);

            sw.Stop();
            print($"Manual deserialization took {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            GC.Collect();
        }

        JsonSerializerSettings newtonsoftSettings = new JsonSerializerSettings();
        newtonsoftSettings.Converters.Add(new ListTestPositionJSONConvert());

        string newtonsoftSerializeResult;
        {
            sw.Start();

            newtonsoftSerializeResult = JsonConvert.SerializeObject(testPositions, newtonsoftSettings);

            sw.Stop();
            print($"Manual Newtonsoft took {sw.ElapsedMilliseconds}ms");
            //print(newtonsoftSerializeResult);
        }

        List<TestPosition> newtonsoftDeserializationResult;
        {
            sw.Start();

            newtonsoftDeserializationResult = JsonConvert.DeserializeObject<List<TestPosition>>(newtonsoftSerializeResult, newtonsoftSettings);

            sw.Stop();
            print($"Manual Newtonsoft deserialization took {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            GC.Collect();
        }
    }

    public class ListTestPositionDeserialize : IJSONSerialize, IJSONDeserialize
    {
        public object Deserialize(JSONReader reader, JSONSettings settings)
        {
            List<TestPosition> result = new List<TestPosition>();
            reader.ExpectArrayStart();
            {
                while(!reader.IsAtArrayEnd()) 
                {
                    TestPosition tp = new TestPosition();
                    reader.ExpectObjectStart();
                    {
                        reader.ConsumePropertyName();
                        tp.EntityName = reader.ConsumeStringValue();
                        reader.ConsumePropertyName();
                        reader.ExpectObjectStart();
                        {
                            reader.ConsumePropertyName();
                            tp.Position.x = (float) reader.ConsumeDoubleValue();
                            reader.ConsumePropertyName();
                            tp.Position.y = (float) reader.ConsumeDoubleValue();
                            reader.ConsumePropertyName();
                            tp.Position.z = (float) reader.ConsumeDoubleValue();
                        }
                        reader.ExpectObjectEnd();
                    }
                    reader.ExpectObjectEnd();
                    result.Add(tp);
                }
            }
            reader.ExpectArrayEnd();
            return result;
        }

        public void Serialize(object value, JSONWriter writer, JSONSettings settings)
        {
            List<TestPosition> positions = (List<TestPosition>)value;
            writer.BeginArray();
            {
                foreach (var pos in positions)
                {
                    writer.BeginArrayValue();
                    writer.BeginObject();
                    {
                        writer.WriteProperty(nameof(pos.EntityName), pos.EntityName);
                        writer.BeginProperty(nameof(pos.Position));
                        {
                            writer.BeginObject();
                            {
                                writer.WriteProperty(nameof(pos.Position.x), pos.Position.x);
                                writer.WriteProperty(nameof(pos.Position.y), pos.Position.y);
                                writer.WriteProperty(nameof(pos.Position.z), pos.Position.z);
                            }
                            writer.EndObject();
                        }
                        writer.EndProperty();
                    }
                    writer.EndObject();
                    writer.EndArrayValue();
                }
            }
            
            writer.EndArray();
        }
    }

    public class ListTestPositionJSONConvert : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<TestPosition>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<TestPosition> result = new List<TestPosition>();
            reader.Read(); // {
            while (reader.TokenType != JsonToken.EndArray)
            {
                TestPosition position = new TestPosition();
                reader.Read(); // EntityName
                position.EntityName = reader.ReadAsString();
                reader.Read(); // Position
                {
                    reader.Read(); // {
                    reader.Read(); // x
                    position.Position.x = (float)reader.ReadAsDouble();
                    reader.Read(); // y
                    position.Position.y = (float)reader.ReadAsDouble();
                    reader.Read(); // z
                    position.Position.z = (float)reader.ReadAsDouble();
                    reader.Read(); // }
                }
                reader.Read(); // }
                reader.Read(); // {
                result.Add(position);
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            List<TestPosition> list = (List<TestPosition>)value;

            writer.WriteStartArray();
            foreach(var val in list)
            {
                writer.WriteStartObject();
                {
                    writer.WritePropertyName(nameof(val.EntityName));
                    writer.WriteValue(val.EntityName);

                    writer.WritePropertyName(nameof(val.Position));
                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName(nameof(val.Position.x));
                        writer.WriteValue(val.Position.x);
                        writer.WritePropertyName(nameof(val.Position.y));
                        writer.WriteValue(val.Position.y);
                        writer.WritePropertyName(nameof(val.Position.z));
                        writer.WriteValue(val.Position.z);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
