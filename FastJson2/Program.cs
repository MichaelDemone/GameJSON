using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastJson2
{
    class Program
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
            public double DoubleValueProperty0;
            public string ValueProperty1;
            public bool BoolProperty10;
            public bool BoolProperty11;
            public object[] ArrayProperty1;
            public double[] IntArray;
            public bool[] BoolArray;
            public NestedStruct ObjectProperty1;
            
            public struct NestedStruct
            {
                public bool InNestedObject;
            }
        }

        static void Main(string[] args)
        {
            // Tasks:
            //  Should this be able to parse objects? 
            //  Should this be able to parse arrays?

            // What do I want this to be exactly? I don't really think I want to make a library because 
            // libraries by very nature need to be general. 

            FloatTest();
            
            TestStruct test = default;

            FastJSON json = new FastJSON(Test);
            json.Expect('{');

            while (json.CurrentChar != '}') {
                if(json.ConsumeIfProperyNameEquals(nameof(test.DoubleValueProperty0)))
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
                    // TODO: Improve this API
                    json.Expect('{');
                    json.ConsumeIfProperyNameEquals("MyNestedObject");
                    json.Expect('{');
                    json.ConsumeIfProperyNameEquals("InNestedObject");
                    test.ObjectProperty1.InNestedObject = json.ConsumeBoolValue();
                    json.Expect('}');
                    json.Expect('}');
                    json.Accept(',');
                }
                else if (json.ConsumeIfProperyNameEquals(nameof(test.IntArray)))
                {
                    //test.IntArray = json.
                    json.ConsumeUnknownValue();
                }
                else
                {
                    json.ConsumePropertyName();
                    json.ConsumeUnknownValue();
                }
            }

            json.Expect('}');
            System.Diagnostics.Debug.Assert(json.IsDone(), $"Json is not done reading. {json.Position} read out of {json.Json.Length}");

            System.Diagnostics.Debug.Assert(test.DoubleValueProperty0 == 142.010, $"{nameof(test.DoubleValueProperty0)}: {test.DoubleValueProperty0} does not equal 142.010");
            System.Diagnostics.Debug.Assert(test.ValueProperty1 == "142.010", $"{nameof(test.ValueProperty1)}: {test.ValueProperty1} does not equal 142.010");
            System.Diagnostics.Debug.Assert(test.BoolProperty10 == true, $"{nameof(test.BoolProperty10)}: {test.BoolProperty10} does not equal true");
            System.Diagnostics.Debug.Assert(test.BoolProperty11 == false, $"{nameof(test.BoolProperty11)}: {test.BoolProperty11} does not equal true");
            System.Diagnostics.Debug.Assert(test.ObjectProperty1.InNestedObject == true, $"{nameof(test.ObjectProperty1.InNestedObject)}: {test.ObjectProperty1.InNestedObject} does not equal true");

            System.Diagnostics.Debug.Print("Done!");
        }

        public static void FloatTest()
        {
            string testInput = "1234.54321";
            double f = InPlaceParsing.ParseDouble("1234.54321", 0, testInput.Length);
            System.Diagnostics.Debug.Assert(f == 1234.54321, $"f: {f} does not equal 1234.54321");
        }
    }
}
