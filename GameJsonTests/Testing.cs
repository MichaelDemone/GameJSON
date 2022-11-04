using GameJSON.ManualParsing;
using GameJSON.ReflectionParsing;
using GameJSON.ManualParsing.Utils;

namespace GameJSONTests
{
    [TestClass]
    public class Tests
    {
        private const string Test = @"
{
	""DoubleValueProperty0"" :   142.010,
	""ValueProperty1"": ""142.010"",
	""ValueProperty2"": ""142"",
	""DoubleValueProperty3"": 0.00000123,
	""DoubleValueProperty4"": 14.452E-23,
	""DoubleValueProperty5"": 14.452E+23,
	""DoubleValueProperty6"": 14.452E23,
	""DoubleValueProperty7"": 14.452e-23,
	""DoubleValueProperty8"": 14.452e+23,
	""DoubleValueProperty9"": 14.452e23,
	""BoolProperty10"": true,
	""BoolProperty11"": false,
	""ArrayProperty1"": [
		""Object"",
		132,
		{
			""InAnArray"": true

        },
		{
			""InAnArrayToo"": true
		}
	],
	""ObjectProperty1"": {
    ""MyNestedObject"": {
        ""InNestedObject"" : true

        }
},
	""ObjectProperty2"": null,
    ""TotalUnknown"": [],
    ""TotalUnknown"": [],
    ""IntArray""  :  [1,2,3,4 , 5 ,78, 9],
    ""BoolArray""  :  [true, True, TRUE, false, False, FALSE],
    ""UnknownString"" : ""lmao get ready for escaped characters \""""
}
";

        public struct TestStruct
        {
            public bool BoolVal;
            public byte ByteVal;
            public sbyte SByteVal;
            public char CharVal;
            public decimal DecimalVal;
            public double DoubleVal;
            public float FloatVal;
            public int IntVal;
            public uint UIntVal;
            public long LongVal;
            public ulong ULongVal;
            public short ShortVal;
            public ushort UShortVal;


            public double DoubleValueProperty0;
            public int IntValueProperty0;
            public short ShortValueProperty0;
            public float FloatValueProperty0;
            public string ValueProperty1;
            public bool BoolProperty10;
            public bool BoolProperty11;
            public object[] ArrayProperty1;
            public int[] IntArray;
            public bool[] BoolArray;
            public List<int> ListOfInts;
            public NestedStruct ObjectProperty1;

            public NestedStruct[] Structs;
            public List<NestedStruct> ListOfStructs;

            public NestedClass NestedConcreteClass;
            public List<NestedClass> ListOfNestedConcreteClass;
            public NestedClass[] NestedConcreteClassArray;

            public NestedGenericClass<bool> NestedGeneric;
            public struct NestedStruct
            {
                public bool InNestedObject;
                public int RandomValue;
            }

            public class NestedClass
            {
                public bool InNestedClass;
                public string MyString;
            }

            public class NestedGenericClass<T>
            {
                public T OnlyObject;
                public bool InGenericClass;
            }

            public static TestStruct MakeDefault()
            {
                TestStruct ts = default;
                ts.CharVal = 'a';
                return ts;
            }
        }

        [TestClass]
        public class InPlaceParserTest
        {
            [TestMethod]
            public static void FloatTests()
            {
                FloatTest("1234.54321");
                FloatTest("1234.54321e21");
                FloatTest("1234.54321e+21");
                FloatTest("1234.54321e-21");

                FloatTest("-1234.54321");
                FloatTest("-1234.54321e21");
                FloatTest("-1234.54321e+21");
                FloatTest("-1234.54321e-21");
            }

            private static void FloatTest(string testInput)
            {
                double actual = InPlaceParsing.ParseDouble(testInput, 0, testInput.Length);
                double expected = double.Parse(testInput);
                Assert(Math.Abs(actual - expected) < 0.01d, $"float test: {actual} does not equal {expected}");
            }
        }

        [TestClass]
        public class ManualJSONTests
        {
            [TestMethod]
            public void ReadTests()
            {
                TestStruct test = default;

                var json = new JSONReader(Test);

                json.ExpectObjectStart();
                while (!json.IsAtObjectEnd())
                {
                    if (json.TryConsumeProperty(nameof(test.DoubleValueProperty0)))
                    {
                        test.DoubleValueProperty0 = json.ConsumeDoubleValue();
                    }
                    else if (json.TryConsumeProperty(nameof(test.ValueProperty1)))
                    {
                        test.ValueProperty1 = json.ConsumeStringValue();
                    }
                    else if (json.TryConsumeProperty(nameof(test.BoolProperty10)))
                    {
                        test.BoolProperty10 = json.ConsumeBoolValue();
                    }
                    else if (json.TryConsumeProperty(nameof(test.BoolProperty11)))
                    {
                        test.BoolProperty11 = json.ConsumeBoolValue();
                    }
                    else if (json.TryConsumeProperty(nameof(test.ObjectProperty1)))
                    {
                        json.ExpectObjectStart();
                        {
                            json.TryConsumeProperty("MyNestedObject");
                            json.ExpectObjectStart();
                            {
                                json.TryConsumeProperty("InNestedObject");
                                test.ObjectProperty1.InNestedObject = json.ConsumeBoolValue();
                            }
                            json.ExpectObjectEnd();
                        }
                        json.ExpectObjectEnd();
                    }
                    else if (json.TryConsumeProperty(nameof(test.IntArray)))
                    {
                        json.ConsumeUnknownValue();
                    }
                    else
                    {
                        json.ConsumePropertyName();
                        json.ConsumeUnknownValue();
                    }
                }
                json.ExpectObjectEnd();

                Assert(json.IsDone(), $"Json is not done reading. {json.Position} read out of {json.Json.Length}");

                Assert(test.DoubleValueProperty0 == 142.010, $"{nameof(test.DoubleValueProperty0)}: {test.DoubleValueProperty0} does not equal 142.010");
                Assert(test.ValueProperty1 == "142.010", $"{nameof(test.ValueProperty1)}: {test.ValueProperty1} does not equal 142.010");
                Assert(test.BoolProperty10 == true, $"{nameof(test.BoolProperty10)}: {test.BoolProperty10} does not equal true");
                Assert(test.BoolProperty11 == false, $"{nameof(test.BoolProperty11)}: {test.BoolProperty11} does not equal true");
                Assert(test.ObjectProperty1.InNestedObject == true, $"{nameof(test.ObjectProperty1.InNestedObject)}: {test.ObjectProperty1.InNestedObject} does not equal true");
            }

            [TestMethod]
            public void WriteTests()
            {
                var ts = new TestStruct
                {
                    DoubleValueProperty0 = 142.010,
                    ValueProperty1 = "142.010",
                    IntArray = new int[] { 1, 2, 3, 4, 5, 78, 9 },
                    BoolProperty10 = false,
                    BoolProperty11 = true
                };
                ts.ObjectProperty1.InNestedObject = true;

                var writer = new JSONWriter();
                writer.BeginObject();
                {
                    writer.WriteProperty(nameof(ts.DoubleValueProperty0), ts.DoubleValueProperty0);
                    writer.WriteProperty(nameof(ts.ValueProperty1), ts.ValueProperty1);
                    //writer.WriteProperty(nameof(ts.DoubleValueProperty4), ts.DoubleValueProperty4);
                    writer.BeginProperty(nameof(ts.ObjectProperty1));
                    writer.BeginObject();
                    {
                        writer.WriteProperty(nameof(ts.ObjectProperty1.InNestedObject), true);
                    }
                    writer.EndObject();
                    writer.EndProperty();

                    writer.WriteProperty(nameof(ts.BoolProperty10), ts.BoolProperty10);
                    writer.WriteProperty(nameof(ts.BoolProperty11), ts.BoolProperty11);

                    writer.BeginProperty(nameof(ts.IntArray));
                    writer.BeginArray();
                    {
                        foreach (var val in ts.IntArray)
                        {
                            writer.WriteArrayValue(val);
                        }
                    }
                    writer.EndArray();
                    writer.EndProperty();
                }
                writer.EndObject();
                Print(writer.GetJSON());
            }
        }

        [TestClass]
        public class ReflectionJSONTests
        {
            [TestMethod]
            public void DefaultReflectionTest()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.BoolProperty10 = true;
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.BoolProperty10, "Did not properly deserialize");
            }

            private void TestString(string testString)
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.ValueProperty1 = testString;
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                Print(testString);
                Print(deserializeRes.ValueProperty1);

                Assert(testString == deserializeRes.ValueProperty1 && deserializeRes.ValueProperty1 == ts.ValueProperty1, "Did not properly deserialize");
            }

            [TestMethod]
            public void StringEscapedTest()
            {
                TestString("Escape characters lets gooo. NewLine: \n. Quote: \". Slash \\. Backward slash /. Backspace \b. Formfeed \f. carridge return \r. Tab \t.");
            }

            [TestMethod]
            public void StringDoubleEscapedTest()
            {
                TestString("Deserializing something that escapes characters already \\\"");
            }

            [TestMethod]
            public void IntArrayReflectionTest()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.IntArray = new int[10];
                ts.IntArray[0] = 5;
                ts.IntArray[8] = 10;
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                for (int i = 0; i < ts.IntArray.Length; i++)
                {
                    Assert(deserializeRes.IntArray[i] == ts.IntArray[i], "Did not properly deserialize");
                }
            }

            [TestMethod]
            public void StructArrays()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.Structs = new TestStruct.NestedStruct[10];
                TestStruct.NestedStruct ns = ts.Structs[0];
                ns.RandomValue = 1;
                ts.Structs[0] = ns;
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.Structs.Length == 10, "Did not properly deserialize");
                for (int i = 0; i < ts.Structs.Length; i++)
                {
                    Assert(deserializeRes.Structs[i].RandomValue == ts.Structs[i].RandomValue, "Did not properly deserialize");
                }
            }

            [TestMethod]
            public void StructList()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.ListOfStructs = new List<TestStruct.NestedStruct>(10);
                var ns = new TestStruct.NestedStruct
                {
                    RandomValue = 1
                };
                ts.ListOfStructs.Add(ns);
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.ListOfStructs.Count == 1, "Did not properly deserialize");
                for (int i = 0; i < ts.ListOfStructs.Count; i++)
                {
                    Assert(deserializeRes.ListOfStructs[i].RandomValue == ts.ListOfStructs[i].RandomValue, "Did not properly deserialize");
                }
            }

            [TestMethod]
            public void ClassArrays()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.NestedConcreteClassArray = new TestStruct.NestedClass[10];
                var nc = new TestStruct.NestedClass
                {
                    MyString = "TESTSSSSS"
                };
                ts.NestedConcreteClassArray[0] = nc;
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.NestedConcreteClassArray[0].MyString == nc.MyString, "Did not properly deserialize");
                for (int i = 1; i < ts.NestedConcreteClassArray.Length; i++)
                {
                    Assert(deserializeRes.NestedConcreteClassArray[i] == null, "Did not properly deserialize");
                }
            }

            [TestMethod]
            public void ClassList()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.ListOfNestedConcreteClass = new List<TestStruct.NestedClass>(10);
                var ns = new TestStruct.NestedClass
                {
                    MyString = "TEST2"
                };
                ts.ListOfNestedConcreteClass.Add(ns);
                string s = JSON.Serialize(ts);
                Print(s);
                var deserializeRes = JSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.ListOfNestedConcreteClass.Count == 1, "Did not properly deserialize");
                for (int i = 0; i < ts.ListOfNestedConcreteClass.Count; i++)
                {
                    Assert(deserializeRes.ListOfNestedConcreteClass[i].MyString == ts.ListOfNestedConcreteClass[i].MyString, "Did not properly deserialize");
                }
            }

            [TestMethod]
            public void GenericReflectionTest()
            {
                var ds = new GenericAsHeck<TestStruct>
                {
                    InstanceOfThing = TestStruct.MakeDefault()
                };
                ds.InstanceOfThing.BoolProperty11 = true;
                string s = JSON.Serialize(ds);
                Print(s);
                var deserializeRes = JSON.Deserialize<GenericAsHeck<TestStruct>>(s);
                Assert(deserializeRes.InstanceOfThing.BoolProperty11, "Did not properly deserialize");
            }

            private class GenericAsHeck<T>
            {
                public T InstanceOfThing;
            }

            public struct Vector3
            {
                public float x, y, z;

                public static bool operator ==(Vector3 rhs, Vector3 lhs)
                {
                    return rhs.x == lhs.x && rhs.y == lhs.y && rhs.z == lhs.z;
                }

                public static bool operator !=(Vector3 rhs, Vector3 lhs)
                {
                    return rhs.x != lhs.x || rhs.y != lhs.y || rhs.z != lhs.z;
                }

                public override bool Equals(object? obj)
                {
                    if (obj is Vector3 v)
                    {
                        return v == this;
                    }
                    return false;
                }

                public override int GetHashCode()
                {
                    return (x, y, z).GetHashCode();
                }
            }

            JSONSettings customSettings = new JSONSettings()
            {
                CustomSerializers = new Dictionary<Type, IJSONSerialize>()
                    {
                        {typeof(Vector3), new Vector3Serializer() }
                    },
                CustomDeserializers = new Dictionary<Type, IJSONDeserialize>()
                    {
                        {typeof(Vector3), new Vector3Serializer() }
                    }
            };

            [TestMethod]
            public void CustomSerializer()
            {
                var v3 = new Vector3
                {
                    x = 1,
                    y = 2,
                    z = 3
                };
                string s = JSON.Serialize(v3);
                Print($"Pre converter {s}");
                string s2 = JSON.Serialize(v3, customSettings);
                Print($"Post converter {s2}");
                Assert(s != s2, "Custom serializer did not change output");
            }

            [TestMethod]
            public void CustomDeserializer()
            {
                var v3 = new Vector3
                {
                    x = 1,
                    y = 2,
                    z = 3
                };

                string s2 = JSON.Serialize(v3, customSettings);

                var v3_deserialize = JSON.Deserialize<Vector3>(s2, customSettings);
                Assert(v3 == v3_deserialize, "Vectors must be equal");
            }

            private class Vector3Serializer : IJSONSerialize, IJSONDeserialize
            {
                public object Deserialize(JSONReader reader, JSONSettings settings)
                {
                    var val = new Vector3();
                    reader.ExpectArrayStart();
                    {
                        val.x = (float)reader.ConsumeDoubleValue();
                        val.y = (float)reader.ConsumeDoubleValue();
                        val.z = (float)reader.ConsumeDoubleValue();
                    }
                    reader.ExpectArrayEnd();

                    return val;
                }

                public void Serialize(object value, JSONWriter writer, JSONSettings settings)
                {
                    var vval = (Vector3)value;
                    writer.BeginArray();
                    {
                        writer.WriteArrayValue(vval.x);
                        writer.WriteArrayValue(vval.y);
                        writer.WriteArrayValue(vval.z);
                    }
                    writer.EndArray();
                }
            }

            public class PropertySerializationTester
            {
                public static int StaticGetterProperty { get { return 1; } }
                public static int StaticSetterProperty { set { } }
                public static int StaticProperty { get; set; }

                public int InstanceGetterProperty { get { return 1; } }
                public int InstanceSetterProperty { set { } }

                [SerializeProperty]
                public int SerializedPropertyProperty { get { return privateProperty; } set { privateProperty = value; } }
                public int PublicAutoProperty { get; set; }
                public int PublicAutoGetProperty { get; }
                private int privateProperty { get; set; }
            }

            [TestMethod]
            public void AutoPropertySerializationTest()
            {
                var objToSerialize = new PropertySerializationTester()
                {
                    PublicAutoProperty = 1,
                };

                var noAutoSerializeSettings = new JSONSettings()
                {
                    SerializeAutoPropertyFields = false
                };

                string s = JSON.Serialize(objToSerialize);
                string s2 = JSON.Serialize(objToSerialize, noAutoSerializeSettings);

                Print($"With auto props\n{s}");
                Print($"Without auto props\n{s2}");

                var res = JSON.Deserialize<PropertySerializationTester>(s);
                Assert(res.PublicAutoProperty == objToSerialize.PublicAutoProperty, "Auto property not set properly");

                var res2 = JSON.Deserialize<PropertySerializationTester>(s, noAutoSerializeSettings);
                Assert(res2.PublicAutoProperty != objToSerialize.PublicAutoProperty, $"Auto property set when it shouldn't have been. Res2: {res.PublicAutoProperty}");

                var res3 = JSON.Deserialize<PropertySerializationTester>(s2, noAutoSerializeSettings);
                Assert(res3.PublicAutoProperty != objToSerialize.PublicAutoProperty, $"Auto properties set when it shouldn't have been. Res3: {res.PublicAutoProperty}");
            }

            [TestMethod]
            public void PropertyTagSerializationTest()
            {
                var objToSerialize = new PropertySerializationTester()
                {
                    SerializedPropertyProperty = 1,
                };

                var dontSerializeAuto = new JSONSettings()
                {
                    SerializeAutoPropertyFields = false
                };

                var serializeWithTags = new JSONSettings()
                {
                    AttemptToSerializePropertiesWithTag = true,
                    SerializeAutoPropertyFields = false
                };

                string serializedWithoutProp = JSON.Serialize(objToSerialize, dontSerializeAuto);
                string serializedWithProp = JSON.Serialize(objToSerialize, serializeWithTags);

                Print($"Without serialization tag\n{serializedWithoutProp}");
                Print($"With serialization tag\n{serializedWithProp}");

                var shouldntSerialize = JSON.Deserialize<PropertySerializationTester>(serializedWithoutProp);
                Assert(shouldntSerialize.SerializedPropertyProperty != objToSerialize.SerializedPropertyProperty, "Shouldn't have deserialized 1");

                var shouldntSerialize2 = JSON.Deserialize<PropertySerializationTester>(serializedWithoutProp, serializeWithTags);
                Assert(shouldntSerialize2.SerializedPropertyProperty != objToSerialize.SerializedPropertyProperty, $"Shouldn't have deseserialized 2. Res2: {shouldntSerialize.SerializedPropertyProperty}");

                var shouldSerialize = JSON.Deserialize<PropertySerializationTester>(serializedWithProp, serializeWithTags);
                Assert(shouldSerialize.SerializedPropertyProperty == objToSerialize.SerializedPropertyProperty, $"Failed to deserialize. Res3: {shouldntSerialize.SerializedPropertyProperty}");

                var shouldntSerialize3 = JSON.Deserialize<PropertySerializationTester>(serializedWithProp);
                Assert(shouldntSerialize3.SerializedPropertyProperty != objToSerialize.SerializedPropertyProperty, "Shouldn't have deserialized 3");
            }
        }

        private static void Assert(bool assertion, string message)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(assertion, message);
        }

        private static void Print(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
