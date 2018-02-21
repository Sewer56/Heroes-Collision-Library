using HeroesCollisionLibrary.Geometry;
using HeroesCollisionLibrary.Geometry.Structures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static HeroesCollisionLibrary.Utilities;
using HeroesCollisionLibrary.Collision;
using HeroesCollisionLibrary.Collision.Bounding_Box;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Class used to generate a Sonic Heroes collision file.
    /// </summary>
    public class CollisionGenerator
    {
        /// <summary>
        /// Defines the properties to be undertaken for the collision generation process.
        /// </summary>
        public static CollisionGeneratorProperties Properties = new CollisionGeneratorProperties();

        /// <summary>
        /// Holds the final colliison file for exporting or writing to a file.
        /// </summary>
        public List<byte> ClFile;

        /// <summary>
        /// This class does anything and everything quadtrees, stores all of the quadtree data for node generation and the actual quadtree itself.
        /// </summary>
        private QuadtreeGenerator _quadtreeData;

        /// <summary>
        /// Defines the individual properties for the collision generation operation. 
        /// </summary>
        public class CollisionGeneratorProperties
        {
            /// <summary>
            /// Path to the .OBJ File we are converting to .cl.
            /// </summary>
            /// <remarks>This is not the only way </remarks>
            public string FilePath { get; set; }

            /// <summary>
            /// Defines whether finding adjacents for each triangle is enabled.
            /// </summary>
            public bool EnableAdjacents { get; set; }

            /// <summary>
            /// Defines whether finding neighbours for each quadnode (beyond the same depth) is enabled.
            /// </summary>
            public bool EnableNeighbours { get; set; }

            /// <summary>
            /// Defines the depth level of the quadnodes.
            /// </summary>
            public byte DepthLevel { get; set; }

            /// <summary>
            /// The base Offset Power Level for the quadnode structure. It is unknown what this does.
            /// </summary>
            public byte BasePower { get; set; }

            /// <summary>
            /// Increases the node size by this value (in floating point units) on every edge of each node, such that there is a small
            /// overlap between nodes when checking for colliding triangles. (Prevents falling through floors). 
            /// The recommended and default value is 10.
            /// </summary>
            public float NodeOverlapRegion { get; set; }
        }

        /// <summary>
        /// Constructor, sets defaults for the geometry extraction operation.
        /// </summary>
        public CollisionGenerator()
        {
            // Set defaults
            Properties.EnableAdjacents = true;
            Properties.EnableNeighbours = true;
            Properties.DepthLevel = 7; 
            Properties.BasePower = 13;
            Properties.NodeOverlapRegion = 25F;
        }

        /// <summary>
        /// Generates the collision with the properties set in the main file. Please first load an OBJ File using LoadOBJFile() 
        /// or manually supply the triangles and their vertices in geometryData.verticesArray, geometryData.triangleArray
        /// </summary>
        public void GenerateCollision()
        {
            // Obtain Quadtree Size & Center
            BenchmarkMethod(CalculateQuadtreeDimensions, "Calculate Quadtree Size & Center");

            // Calculate Triangle Bounding Boxes and set checking method to *fast*
            BenchmarkMethod(CalculateTriangleBoundingBoxes, "Calculate Triangle Bounding Boxes");

            // Calculating Normals of all Triangles
            BenchmarkMethod(CalculateTriangleNormals, "Calculating Normals for Each Triangle");

            // Generate the Quadtree!
            BenchmarkMethod(GenerateQuadtree, "Generating the Quadtree");

            // Finding Neighbours of all of the nodes and triangles.
            if (Properties.EnableNeighbours)
            {
                // Local Neighbours = Neighbours within same parent node.
                BenchmarkMethod(CalculateNodeNeighbours, "Calculating Extended (Other Depth) Node Neighbours");
            }

            // Calculating Adjacents of all Triangles
            if (Properties.EnableAdjacents) { BenchmarkMethod(CalculateTriangleAdjacents, "Calculating Adjacents for Each Triangle"); }
            else { BenchmarkMethod(CalculateTriangleAdjacents, "Setting Default Adjacents"); }

            // Generating Collision File
            BenchmarkMethod(GenerateCollisionFileBytes, "Generating Collision File");
        }

        /// <summary>
        /// Loads the OBJ File's vertices and triangles.
        /// </summary>
        public void LoadObjFile()
        {
            // Set Culture
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            // Read Contents of OBJ File
            BenchmarkMethod(ReadObjFile, "Reading Contents of OBJ File");
        }

        /// <summary>
        /// Writes the complete collision file to disk.
        /// </summary>
        public void WriteFile()
        {
            // Write the output of the file.
            File.WriteAllBytes(Properties.FilePath + ".cl", ClFile.ToArray());
        }

        /// <summary>
        /// Writes the complete collision file to disk, to the specified file path.
        /// </summary>
        public void WriteFile(string filePath)
        {
            // Write the output of the file.
            File.WriteAllBytes(filePath, ClFile.ToArray());
        }

        /// <summary>
        /// Generates the Quadtree node structure.
        /// </summary>
        private void GenerateQuadtree()
        {
            // Assign the quadnode property structure.
            _quadtreeData = new QuadtreeGenerator(Properties.DepthLevel, Properties.BasePower);

            // Generate all of the quadnodes.
            _quadtreeData.GenerateQuadnodes();
        }

        /// <summary>
        /// Calculates the adjacent triangles of all of the triangles.
        /// Uses quadnodes in order to speed up adjacent triangle search.
        /// </summary>
        private void CalculateTriangleAdjacents()
        {
            AdjacentTriangles.CalculateTriangleAdjacentsV2(_quadtreeData);
        }

        /// <summary>
        /// Calculate the quadtree size and center, necessary for both the file but also for searching triangle intersections/
        /// </summary>
        private void CalculateQuadtreeDimensions()
        {
            HeroesHeader.BasePower = Properties.BasePower;
            HeroesHeader.GetQuadtreeSizeCenter(GeometryData.Vertices);
        }

        /// <summary>
        /// Calculates the bounding boxes of each of the triangles, used for collision checking against nodes.
        /// </summary>
        private void CalculateTriangleBoundingBoxes()
        {
            // Calculate all of the triangles' bounding boxes.
            CalculateBoundingBox.CalculateTriangleBoundingBoxes();
        }

        /// <summary>
        /// Calculates all of the neighbours for each individual node.
        /// </summary>
        private void CalculateNodeNeighbours()
        {
            _quadtreeData.CalculateNodeNeighbours();
        }

        /// <summary>
        /// Reads the entirety of the OBJ File and returns it to the main program.
        /// </summary>
        private void ReadObjFile()
        {
            // Define new instance of the OBJ Utility Class with the OBJ Target set in sight.
            ObjParser objFile = new ObjParser(Properties.FilePath);

            // Calculate all of the collision properties.
            objFile.ReadObjFile();
        }

        /// <summary>
        /// Calculates the normal unit vector of each individual triangle by performing a cross
        /// product of two of the vertices (relative to a specific point of vertex 1 "simulated 0,0").
        /// </summary>
        private void CalculateTriangleNormals()
        {
            TriangleUtilities.CalculateTriangleNormals();
        }

        /// <summary>
        /// Generates a collision file for writing out to disk.
        /// </summary>
        private void GenerateCollisionFileBytes()
        {
            ClFile = CLFileGenerator.GenerateFile(ClFile, _quadtreeData);
        }
    }
}