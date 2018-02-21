# Heroes Collision Library

Heroes Collision Library is self explanatory, it is library for the manipulation of Sonic Heroes' collision files written in .NET Standard 2.0, offering fast collision generation speeds while being (at the time of writing), the only library offering accurate collision checking.

Provided with the library is also a simple importer/exporer, HeroesCollisionTool, which can be used for the exporting and importing of collision files.

# Usage

Note: HeroesCollisionTool is a simple example application, it can be run with `dotnet HeroesCollisionTool.dll` in the command line.

This project is a library built with .NET Standard 2.0, thus it can be included in all .NET Framework, .NET Core, and Xamarin projects. 

It may be either added via right clicking "Dependencies" (or References) and adding the individual library as a reference or copying over the code from HeroesCollisionLibrary into your project.

If you would want to manually compile the project, open up the Visual Studio solution inside the Heroes-Collision-Library directory (for HeroesCollisionTool you will also be required to change the path to the HeroesCollisionLibrary DLL dependency). There are also configurations for VSCode from earlier stages of development should that be your preferred choice of editor.

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

### Reminder

You should always playtest the collision in your level before publishing the stage, by walking the entirety of the level contents.

It is not recommended to export collision files with depths ("nodelevel") greater than 7. The reason is that random locations within levels still crash on every custom files from every single collision exporter, inclusive of this one. Despite going great depths in searching, the reason after even half a year remains unknown.

If you find a spot where the game randomly crashes, consider manually playing around with the depth level (typically you simply need to unfortunately set it lower).
