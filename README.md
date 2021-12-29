# Game JSON
A fast lower-level JSON library for C#, particularly aimed at games. This was built to satisfy the need that most JSON libraries cannot be sped up enough to (de)serialize files in a sufficient amount of time. 

# Structure
Comes in 2 parts
* GameJSON.ReflectionParsing - Reflection Library for serializing/deserializing objects to/from JSON. Contains JSON class.
* GameJSON.ManualParsing - A manual parsing library that ReflectionParsing is built on top of. If ReflectionParsing doesn't (de)serialize your JSON fast enough, using ManualParsing to manually (de)serialize will give you a massive performance boost. Contains JSONReader and JSONWriter class.

# Reflection Parsing Goals
* All fields on an object will be serialized and deserialized 
* Users can opt into serializing properties with SerializeProperty attribute
* Auto properties are serialized automatically (i.e. public int MyAutoProperty { get; set; })

# Manual Parsing Goals
* Have as few additional allocations as possible
* Give the user as much power over serializing the JSON as they want to achieve great performance

