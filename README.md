# Game JSON
A fast no dependency JSON library for C#, particularly aimed at games. This was built to satisfy the need that most JSON libraries cannot be sped up enough to (de)serialize files in a sufficient amount of time without incurring a framerate drop. Currently only deserializes strings and serializes to strings.

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

These are the performance test results. As a summary, this behaves significantly faster than Newtonsoft in most cases except when serializing data for the second time. This is most likely because GameJSON does absolutely no type caching. Most importantly though, GameJSON manually deserializes JSON 12x faster than Newtonsoft with a cold cache (first run) and 4x faster with a hot cache (second run). This means that files or server messages can be loaded with GameJSON in games without incurring a huge frame spike. 

## Tests
I created an object `TestPositions` with a `string` and a `Vector3`, and had each library serialize and deserialize these objects.
* Windows - i7-12700H

### 1000 Test Positions Cold Platform
This captures the first run of serializing/deserializing 1000 `TestPositions` on the respective Platform

### 1000 Test Positions Hot Platform
This captures the second run of serializing/deserializing of 1000 `TestPosition` on the respective Platform

### Results
![NewtonsoftVsGameJSON](https://user-images.githubusercontent.com/10680328/200100034-912e6142-5a72-4e3a-913f-5c4b49fbff0d.png)

