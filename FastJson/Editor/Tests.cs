using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FastJson
{
    class Tests
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
    ""BoolArray""  :  [true, True, TRUE, false, False, FALSE]
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

            public static TestStruct MakeDefault() {
                TestStruct ts = default;
                ts.CharVal = 'a';
                return ts;
            }
        }

        public class FastJSONTests
        {
            [Test]
            public void ReadTests()
            {
                TestStruct test = default;

                FastJSONReader json = new FastJSONReader(Test);

                json.ExpectObjectStart();
                while (!json.IsAtObjectEnd())
                {
                    if (json.ConsumeIfProperyNameEquals(nameof(test.DoubleValueProperty0)))
                    {
                        test.DoubleValueProperty0 = json.ConsumeDoubleValue();
                    }
                    else if (json.ConsumeIfProperyNameEquals(nameof(test.ValueProperty1)))
                    {
                        test.ValueProperty1 = json.ConsumeStringValue();
                    }
                    else if (json.ConsumeIfProperyNameEquals(nameof(test.BoolProperty10)))
                    {
                        test.BoolProperty10 = json.ConsumeBoolValue();
                    }
                    else if (json.ConsumeIfProperyNameEquals(nameof(test.BoolProperty11)))
                    {
                        test.BoolProperty11 = json.ConsumeBoolValue();
                    }
                    else if (json.ConsumeIfProperyNameEquals(nameof(test.ObjectProperty1)))
                    {
                        json.ExpectObjectStart();
                        {
                            json.ConsumeIfProperyNameEquals("MyNestedObject");
                            json.ExpectObjectStart();
                            {
                                json.ConsumeIfProperyNameEquals("InNestedObject");
                                test.ObjectProperty1.InNestedObject = json.ConsumeBoolValue();
                            }
                            json.ExpectObjectEnd();
                        }
                        json.ExpectObjectEnd();
                    }
                    else if (json.ConsumeIfProperyNameEquals(nameof(test.IntArray)))
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

                Print("Done!");
            }

            [Test]
            public void WriteTests()
            {
                TestStruct ts = new TestStruct();
                ts.DoubleValueProperty0 = 142.010;
                ts.ValueProperty1 = "142.010";
                ts.ObjectProperty1.InNestedObject = true;
                ts.IntArray = new int[] { 1, 2, 3, 4, 5, 78, 9 };
                ts.BoolProperty10 = false;
                ts.BoolProperty11 = true;

                FastJSONWriter writer = new FastJSONWriter();
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

            public class InPlaceParserTest
            {
                [Test]
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
        }

        public class EasyJSONTests
        {
            [Test]
            public void DefaultReflectionTest()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.BoolProperty10 = true;
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.BoolProperty10, "Did not properly deserialize");
            }

            [Test]
            public void StringReflectionTest()
            {
                TestStruct ts = TestStruct.MakeDefault();
                string escapeString = "Escape characters lets gooo \"\\ how about \n or maybe \t or how about \r??";
                ts.ValueProperty1 = escapeString;
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.ValueProperty1 == ts.ValueProperty1, "Did not properly deserialize");
            }

            [Test]
            public void IntArrayReflectionTest()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.IntArray = new int[10];
                ts.IntArray[0] = 5;
                ts.IntArray[8] = 10;
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                for (int i = 0; i < ts.IntArray.Length; i++)
                {
                    Assert(deserializeRes.IntArray[i] == ts.IntArray[i], "Did not properly deserialize");
                }
            }

            [Test]
            public void StructArrays()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.Structs = new TestStruct.NestedStruct[10];
                TestStruct.NestedStruct ns = ts.Structs[0];
                ns.RandomValue = 1;
                ts.Structs[0] = ns;
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.Structs.Length == 10, "Did not properly deserialize");
                for (int i = 0; i < ts.Structs.Length; i++)
                {
                    Assert(deserializeRes.Structs[i].RandomValue == ts.Structs[i].RandomValue, "Did not properly deserialize");
                }
            }

            [Test]
            public void StructList()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.ListOfStructs = new List<TestStruct.NestedStruct>(10);
                TestStruct.NestedStruct ns = new TestStruct.NestedStruct();
                ns.RandomValue = 1;
                ts.ListOfStructs.Add(ns);
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.ListOfStructs.Count == 1, "Did not properly deserialize");
                for (int i = 0; i < ts.ListOfStructs.Count; i++)
                {
                    Assert(deserializeRes.ListOfStructs[i].RandomValue == ts.ListOfStructs[i].RandomValue, "Did not properly deserialize");
                }
            }

            [Test]
            public void ClassArrays()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.NestedConcreteClassArray = new TestStruct.NestedClass[10];
                TestStruct.NestedClass nc = new TestStruct.NestedClass();
                nc.MyString = "TESTSSSSS";
                ts.NestedConcreteClassArray[0] = nc;
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.NestedConcreteClassArray[0].MyString == nc.MyString, "Did not properly deserialize");
                for (int i = 1; i < ts.NestedConcreteClassArray.Length; i++)
                {
                    Assert(deserializeRes.NestedConcreteClassArray[i] == null, "Did not properly deserialize");
                }
            }

            [Test]
            public void ClassList()
            {
                TestStruct ts = TestStruct.MakeDefault();
                ts.ListOfNestedConcreteClass = new List<TestStruct.NestedClass>(10);
                var ns = new TestStruct.NestedClass();
                ns.MyString = "TEST2";
                ts.ListOfNestedConcreteClass.Add(ns);
                string s = EasyJSON.Serialize(ts);
                Print(s);
                var deserializeRes = EasyJSON.Deserialize<TestStruct>(s);
                Assert(deserializeRes.ListOfNestedConcreteClass.Count == 1, "Did not properly deserialize");
                for (int i = 0; i < ts.ListOfNestedConcreteClass.Count; i++)
                {
                    Assert(deserializeRes.ListOfNestedConcreteClass[i].MyString == ts.ListOfNestedConcreteClass[i].MyString, "Did not properly deserialize");
                }
            }
        }

        private static void Assert(bool assertion, string message) {
            UnityEngine.Debug.Assert(assertion, message);
        }

        private static void Print(string message) {
            UnityEngine.Debug.Log(message);
        }
    }
}
