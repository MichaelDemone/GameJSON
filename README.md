# Game JSON
A fast lower-level JSON library for C#, particularly aimed at games. This was built to satisfy the need that most JSON libraries cannot be sped up enough to (de)serialize files in a sufficient amount of time. 

# Structure
Comes in 2 parts
* GameJSON.ReflectionParsing - Reflection Library for serializing/deserializing JSON
* GameJSON.ManualParsing - A manual parsing library that ReflectionParsing is built on top of. If ReflectionParsing doesn't deserialize your JSON fast enough, using ManualParsing to manually deserialize will give you a massive performance boost

# Reflection Parsing Goals
* Have the same functionality as most JSON libraries.

# Manual Parsing Goals
* Have as few additional allocations as possible
* Give the user as much power over serializing the JSON as they want to achieve great performance