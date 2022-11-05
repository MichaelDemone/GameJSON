using System.Diagnostics;
using Newtonsoft.Json;
using System.Diagnostics;
using GameJSON.ReflectionParsing;
using static ManualPerformance;

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
        var data = GetData();

        Console.WriteLine("\nGame Json reflection cold test");
        RunGameJSONTest(data);
        Console.WriteLine("\nGame Json reflection hot test");
        RunGameJSONTest(data);

        
        Console.WriteLine("\nNewtonsoft reflection cold test");
        RunNetwtonsoftTest(data);
        Console.WriteLine("\nNewtonsoft reflection hot test");
        RunNetwtonsoftTest(data);
    }

    public static List<TestPosition> GetData()
    {
        List<TestPosition> testPositions = new List<TestPosition>();
        Random r = new Random(1);
        for (int i = 0; i < 1000; i++)
        {
            testPositions.Add(new TestPosition()
            {
                EntityName = $"Entity{i}",
                Position = new MyVector3(r.NextSingle() * 100, r.NextSingle() * 100, r.NextSingle() * 100)
            });
        }
        GC.Collect();
        return testPositions;
    }

    public static void RunGameJSONTest(List<TestPosition> positions)
    {
        Stopwatch sw = new Stopwatch();
        string gameJsonSerializeString;
        {
            sw.Start();

            gameJsonSerializeString = JSON.Serialize(positions);

            sw.Stop();
            Console.WriteLine($"Reflection serializing took {sw.ElapsedTicks} ticks");

            sw.Reset();
            GC.Collect();
        }

        List<TestPosition> gameJsonDeserializationResult;
        {
            sw.Start();

            gameJsonDeserializationResult = JSON.Deserialize<List<TestPosition>>(gameJsonSerializeString);

            sw.Stop();
            Console.WriteLine($"Reflection deserialization took {sw.ElapsedTicks} ticks");

            sw.Reset();
            GC.Collect();
        }
    }

    public static void RunNetwtonsoftTest(List<TestPosition> positions)
    {
        Stopwatch sw = new Stopwatch();
        string newtonsoftSerializeResult;
        {
            sw.Start();

            newtonsoftSerializeResult = JsonConvert.SerializeObject(positions);

            sw.Stop();
            Console.WriteLine($"Reflection Newtonsoft serializing took {sw.ElapsedTicks} ticks");
        }

        List<TestPosition> newtonsoftDeserializationResult;
        {
            sw.Start();

            newtonsoftDeserializationResult = JsonConvert.DeserializeObject<List<TestPosition>>(newtonsoftSerializeResult);

            sw.Stop();
            Console.WriteLine($"Reflection Newtonsoft deserialization took {sw.ElapsedTicks} ticks");

            sw.Reset();
            GC.Collect();
        }
    }
}