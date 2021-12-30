# Game JSON
A fast lower-level JSON library for C#, particularly aimed at games. This was built to satisfy the need that most JSON libraries cannot be sped up enough to (de)serialize files in a sufficient amount of time without incurring a framerate drop. 

# Structure
Comes in 2 parts
* GameJSON.ReflectionParsing - Reflection Library for serializing/deserializing objects to/from JSON. Contains `JSON` class.
* GameJSON.ManualParsing - A manual parsing library that ReflectionParsing is built on top of. If ReflectionParsing doesn't (de)serialize your JSON fast enough, using ManualParsing to manually (de)serialize will give you a massive performance boost. Contains `JSONReader` and `JSONWriter` class.

# Reflection Parsing Goals
* All fields on an object will be serialized and deserialized 
* Users can opt into serializing properties with `SerializeProperty` attribute
* Auto properties are serialized automatically (i.e. `public int MyAutoProperty { get; set; }`)

# Manual Parsing Goals
* Have as few additional allocations as possible
* Give the user as much power over serializing the JSON as they want to achieve great performance

# Performance

These are the performance test results on my machine (i5-9600K CPU). As a summary, this behaves significantly faster than Newtonsoft in most cases except when loading using reflection with a hot cache. This is most likely because GameJSON does absolutely no caching. Most importantly though, GameJSON manually deserializes JSON at 5ms cold (i.e. first run with data type) and 2ms hot (i.e. second run with data type), whereas Newtonsoft manually deserializes in 95ms cold and 11ms hot. This means that files can be loaded with GameJSON in games without incurring a huge frame spike.

## Tests
I created an object `TestPositions` with a `string` and a `Vector3`, and had each library serialize and deserialize these objects.

### 1000 Test Positions Cold Cache
This captures the first run of serializing/deserializing 1000 `TestPositions`

### 1000 Test Positions Hot Cache
This captures the second run of serializing/deserializing of 1000 `TestPosition`

### Results
| TestName                      | 1000 Test Positions Cold Cache | 1000 Test Positions Hot Cache |
| ----------------------------- | ------------------------------ | ----------------------------- |
| Reflection Serialize          | 14ms                           | 12ms                          |
| Reflection Deserialize        | 23ms                           | 17ms                          |
| Newtonsoft Serialize          | 142ms                          | 11ms                          |
| Newtonsoft Deserialize        | 180ms                          | 30ms                          |
| Manual Serialize              | 7ms                            | 4ms                           |
| Manual Deserialize            | 5ms                            | 2ms                           |
| Newtonsoft Manual Serialize   | 79ms                           | 5ms                           |
| Newtonsoft Manual Deserialize | 95ms                           | 11ms                          |
