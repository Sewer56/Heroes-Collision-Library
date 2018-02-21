# Heroes Collision Library

Heroes Collision Library is self explanatory, it is library for the manipulation of Sonic Heroes' collision files written in .NET Core 2.0, offering fast collision generation speeds while being (at the time of writing), the only library offering accurate collision checking.

Provided with the library is also a simple importer/exporer, HeroesCollisionTool, which can be used for the exporting and importing of collision files.

# Usage

Note: HeroesCollisionTool is a simple example application, it can be run with `dotnet HeroesCollisionTool.dll` in the command line.

First you need to set the instantiate and set up properties for the collision generator.

## Instantiation
```csharp
/// <summary>
/// Creates an instance of the generator that is part of library for Heroes Collision Generation.
/// </summary>
private static CollisionGenerator collisionGenerator;

void SetupCollisionGenerator()
{
    // Instantiate the collision generator
    collisionGenerator = new CollisionGenerator();    
    
    // Setup generator properties
    SetupCollisionGenerator();    
}
```

## Setting Properties
```csharp
/// <summary>
/// Sets up properties for the collision generator.
/// </summary>
void SetupCollisionGenerator()
{
    // Properties
    // Note that only setting the file path is necessary
    // setting up other properties overrides their defaults.
    CollisionGenerator.Properties.FilePath = "";
    CollisionGenerator.Properties.DepthLevel = 7;
    CollisionGenerator.Properties.NodeOverlapRegion = 25.0F;
    CollisionGenerator.Properties.EnableNeighbours = true;
    CollisionGenerator.Properties.EnableAdjacents = true;
    CollisionGenerator.Properties.BasePower = 13;
}
```

## Loading OBJ File

This part is optional and you can specify your own vertex and/or triangle list.

```csharp
// Assumes that the file path has already been set up in properties.
collisionGenerator.LoadObjFile();
```

## Supplying own Triangle List

Useful for specifying own collision flags and/or importing geometry manually in your own program from other formats.

```csharp
// You should see the structure of Vertex and HeroesTriangle.
// Alas, they are the same as the actual structures in the end file.
// See our wiki'd documentation at: http://info.sonicretro.org/SCHG:Sonic_Heroes/Collision_Format
// (The exporter will auto calculate override normals, adjacents during generation)
GeometryData.Vertices = <Your Vertex List>
GeometryData.Triangles = <Your Triangle List> 
```

## Generating file and writing to Disk
```csharp
// Generate the Collision File
collisionGenerator.GenerateCollision();

// Write the file to disk
collisionGenerator.WriteFile();
```

## Converting CL to OBJ

The library can also convert existing CL collision files to the OBJ format.

```csharp
// Create Collision Exporter
CollisionExporter collisionExporter = new CollisionExporter();

// Read the collision file.
collisionExporter.ReadColliison(filePath);

// Write to disk/retrieve raw bytes
collisionExporter.WriteCollision(filePath + ".obj");
collisionExporter.GetCollision();
```
