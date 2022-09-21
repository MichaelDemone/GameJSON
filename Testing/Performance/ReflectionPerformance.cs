using System.Diagnostics;
using Newtonsoft.Json;
using SimpleTester.Assertions;

public struct MyVector3
{
    public float x;
    public float y;
    public float z;
    public MyVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static bool operator ==(MyVector3 this1, MyVector3 other)
    {
        return this1.x == other.x &&
            this1.y == other.y &&
            this1.z == other.z;
    }

    public static bool operator !=(MyVector3 this1, MyVector3 other)
    {
        return !(this1 == other);
    }
}
public class TestPosition
{
    public string EntityName;
    public MyVector3 Position;
}

public class ReflectionPerformance
{
    public static void Start()
    {
        Console.WriteLine("Starting Reflection Cold Test");
        RunSerializationTest();
        Console.WriteLine("Starting Reflection Hot Test");
        RunSerializationTest();
    }

    public static void RunSerializationTest()
    {
        List<TestPosition> testPositions = new List<TestPosition>();
        Random r = new Random(1);

        for (int i = 0; i < 1000; i++)
        {
            testPositions.Add(new TestPosition() {
                EntityName = $"Entity{i}",
                Position = new MyVector3(r.NextSingle() * 100, r.NextSingle() * 100, r.NextSingle() * 100)
            });
        }
        GC.Collect();

        Stopwatch sw = new Stopwatch();
        string gameJsonSerializeString;
        {
            sw.Start();

            gameJsonSerializeString = GameJSON.ReflectionParsing.JSON.Serialize(testPositions);

            sw.Stop();
            Console.WriteLine($"Reflection parsing took {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            GC.Collect();
        }

        List<TestPosition> gameJsonDeserializationResult;
        {
            sw.Start();

            gameJsonDeserializationResult = GameJSON.ReflectionParsing.JSON.Deserialize<List<TestPosition>>(gameJsonSerializeString);

            sw.Stop();
            Console.WriteLine($"Reflection deserialization took {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            GC.Collect();
        }

        string newtonsoftSerializeResult;
        {
            sw.Start();

            newtonsoftSerializeResult = JsonConvert.SerializeObject(testPositions);

            sw.Stop();
            Console.WriteLine($"Newtonsoft took {sw.ElapsedMilliseconds}ms");
        }

        List<TestPosition> newtonsoftDeserializationResult;
        {
            sw.Start();

            newtonsoftDeserializationResult = JsonConvert.DeserializeObject<List<TestPosition>>(newtonsoftSerializeResult);

            sw.Stop();
            Console.WriteLine($"Newtonsoft deserialization took {sw.ElapsedMilliseconds}ms");

            sw.Reset();
            GC.Collect();
        }

        for(int i = 0; i < 1000; i++)
        {
            Asserts.Assert(testPositions[i].EntityName == gameJsonDeserializationResult[i].EntityName);
            Asserts.Assert(testPositions[i].Position == gameJsonDeserializationResult[i].Position);

            Asserts.Assert(testPositions[i].EntityName == newtonsoftDeserializationResult[i].EntityName);
            Asserts.Assert(testPositions[i].Position == newtonsoftDeserializationResult[i].Position);
        }
    }
}
