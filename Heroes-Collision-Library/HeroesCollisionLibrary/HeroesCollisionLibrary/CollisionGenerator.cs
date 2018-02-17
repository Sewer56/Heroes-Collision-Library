using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Class used to generate a Sonic Heroes collision file.
    /// </summary>
    public class CollisionGenerator
    {
        /// <summary>
        /// Geometry data read from the OBJ File, Contains Triangles & Vertices.
        /// </summary>
        public Geometry_Properties geometryData = new Geometry_Properties();

        /// <summary>
        /// This class does anything and everything quadtrees, stores all of the quadtree data for node generation and the actual quadtree itself.
        /// </summary>
        Quadtree_Properties quadtreeData;

        /// <summary>
        /// Stores the file header contents, including information about the actual collision file.
        /// </summary>
        Collision_Header collisionData = new Collision_Header(); 

        /// <summary>
        /// Defines the properties to be undertaken for the collision generation process.
        /// </summary>
        public Collision_Generator_Properties collisionGeneratorProperties = new Collision_Generator_Properties();

        /// <summary>
        /// Holds the final colliison file for exporting or writing to a file.
        /// </summary>
        public List<byte> CLFile;

        /// <summary>
        /// Defines the individual properties for the collision generation operation. 
        /// </summary>
        public struct Collision_Generator_Properties
        {
            /// <summary>
            /// Path to the .OBJ File we are converting to .cl
            /// </summary>
            public string filePath;

            /// <summary>
            /// Defines whether finding adjacents for each triangle is enabled.
            /// </summary>
            public bool adjacentsEnabled;

            /// <summary>
            /// Defines whether finding neighbours for each quadnode is enabled.
            /// </summary>
            public bool neighboursEnabled;

            /// <summary>
            /// Defines the depth level of the quadnodes.
            /// </summary>
            public byte depthLevel;

            /// <summary>
            /// The base Offset Power Level for the quadnode structure. It is unknown what this does.
            /// </summary>
            public byte basePower;

            /// <summary>
            /// Scales the size of each quadnode to prevent potential missing triangles in a node. (Prevents falling through floors)
            /// </summary>
            public float nodeScale;

            /// <summary>
            /// Uses Bounding Box collision checking between triangles and nodes. Fast but less optimal.
            /// </summary>
            public bool useAABB;
        }

        /// <summary>
        /// Constructor, sets defaults for the geometry extraction operation.
        /// </summary>
        public CollisionGenerator()
        {
            // Set defaults
            collisionGeneratorProperties.adjacentsEnabled = true;
            collisionGeneratorProperties.neighboursEnabled = true;
            collisionGeneratorProperties.depthLevel = 7; 
            collisionGeneratorProperties.basePower = 13;
            collisionGeneratorProperties.nodeScale = 1.05F;
            collisionGeneratorProperties.useAABB = false;
        }

        /// <summary>
        /// Generates the collision with the properties set in the main file. Please first load an OBJ File using LoadOBJFile() 
        /// or manually supply the triangles and their vertices in geometryData.verticesArray, geometryData.triangleArray
        /// </summary>
        public void GenerateCollision()
        {
            // Obtain Quadtree Size & Center
            BenchmarkMethod(CalculateQuadtreeDimensions, "Calculate Quadtree Size & Center");

            // Generate the Quadtree!
            BenchmarkMethod(GenerateQuadtree, "Generating the Quadtree Nodes");

            // Calculate Node Bounding Boxes
            BenchmarkMethod(CalculateNodeBoundingBoxes, "Calculate Quadnode Bounding Boxes");

            // Calculate Triangle Bounding Boxes and set checking method to *fast*
            BenchmarkMethod(CalculateTriangleBoundingBoxes, "Calculate Triangle Bounding Boxes");

            // Finding intersections between nodes and triangles.
            BenchmarkMethod(CalculateNodeTriangleIntersections, "Finding Node-Triangle Intersections");

            // Calculating Adjacents of all Triangles
            if (collisionGeneratorProperties.adjacentsEnabled) { BenchmarkMethod(CalculateTriangleAdjacentsV2, "Calculating Adjacents for Each Triangle"); }
            else { BenchmarkMethod(CalculateTriangleAdjacentsV2, "Setting Default Adjacents"); }

            // Find Empty Nodes
            BenchmarkMethod(AnnihilateEmptyNodes, "Annihilating Empty Nodes");

            // Finding Neighbours of all of the nodes and triangles.
            if (collisionGeneratorProperties.neighboursEnabled) 
            { 
                // Local Neighbours = Neighbours within same parent node.
                BenchmarkMethod(CalculateLocalNodeNeighbours, "Calculating Local Node Neighbours");
                BenchmarkMethod(CalculateNodeNeighbours, "Calculating Non-Local Node Neighbours"); 
            }

            // Write the file
            BenchmarkMethod(GenerateFile, "Generating File");
        }

        /// <summary>
        /// Loads the OBJ File's vertices and triangles.
        /// </summary>
        public void LoadOBJFile()
        {
            // Read Contents of OBJ File
            BenchmarkMethod(ReadOBJFile, "Reading Contents of OBJ File");

            // Calculating Normals of all Triangles
            BenchmarkMethod(CalculateTriangleNormals, "Calculating Normals for Each Triangle");
        }

        /// <summary>
        /// Writes the complete collision file to disk.
        /// </summary>
        public void WriteFile()
        {
            // Write the output of the file.
            File.WriteAllBytes(collisionGeneratorProperties.filePath + ".cl", CLFile.ToArray());
        }

        /// <summary>
        /// Writes the complete collision file to disk, to the specified file path.
        /// </summary>
        public void WriteFile(string filePath)
        {
            // Write the output of the file.
            File.WriteAllBytes(filePath, CLFile.ToArray());
        }

        ///////////////////////////////////////////
        ///////////////////////////////////////////
        /////////////////////////////////////////// 
        /// The code below is stored Here as it manipulates elements stored across different classes.!--
        /// This is to avoid parameter hell.

        /// <summary>
        /// Struct containing the offsets each CL file section.
        /// </summary>
        public struct CLFileOffsets
        {
            public int nodeSectionOffset;
            public int triangleSectionOffset;
            public int vertexSectionOffset;
            public int totalFileSize;
        }

        /// <summary>
        /// Prints the statistics for the generated collision file.
        /// </summary>
        public void PrintStatistics()
        {
            float minNodeSize = float.MaxValue;
            for (int x = 0; x < quadtreeData.nodeBoxes.Length; x++)
            {
                if (quadtreeData.nodeBoxes[x].MaxX - quadtreeData.nodeBoxes[x].MinX < minNodeSize) 
                { 
                    minNodeSize = quadtreeData.nodeBoxes[x].MaxX - quadtreeData.nodeBoxes[x].MinX; 
                }
            }

            int triangleCount = 0;
            for (int x = 0; x < quadtreeData.quadNodes.Count; x++)
            {
                triangleCount += quadtreeData.quadNodes[x].trianglesInNode.Count;
            }

            Console.WriteLine("Min Node Size: " + minNodeSize);
            Console.WriteLine("Avg Triangles Per Node: " + ((float)triangleCount / (float)quadtreeData.quadNodes.Count));
        }

        /// <summary>
        /// Writes the collision file out to a local file.
        /// </summary>
        private void GenerateFile()
        {
            // Allocate a buffer for the collision file.
            // The buffer size is 2MiB
            CLFile = new List<byte>(2097152);

            // Calculate the file offsets.
            CLFileOffsets fileOffsets = CalculateCLOffsets();

            // Retrieve the header.
            collisionData = CalculateCollisionHeader(collisionData, fileOffsets);

            // Write the file header.
            WriteCollisionFileHeader(CLFile);

            // Write the triangle reference list.
            WriteTriangleList(CLFile);

            // Write the quadnodes
            WriteQuadnodeList(CLFile);

            // Write all of the triangle entries.
            WriteTriangles(CLFile);

            // Write the vertices.
            for (int x = 0; x < geometryData.verticesArray.Length; x++)
            {
                CLFile.AddRange(BitConverter.GetBytes((float)geometryData.verticesArray[x].X).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((float)geometryData.verticesArray[x].Y).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((float)geometryData.verticesArray[x].Z).Reverse());
            }
        }
        
        /// <summary>
        /// Writes the triangle list of a collision CL file.
        /// </summary>
        private void WriteTriangles(List<byte> CLFile)
        {
            // Write the triangles
            for (int x = 0; x < geometryData.triangleArray.Length; x++)
            {
                // Add each of the triangle vertices.
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleArray[x].vertexOne).Reverse());
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleArray[x].vertexTwo).Reverse());
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleArray[x].vertexThree).Reverse());

                // Add each of the triangle's adjacents.
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleAdjacents[x].triangleIndices[0]).Reverse());
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleAdjacents[x].triangleIndices[1]).Reverse());
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleAdjacents[x].triangleIndices[2]).Reverse());

                // Write the vertex normals to the file
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleNormals[x].X).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((float)geometryData.triangleNormals[x].Y).Reverse()); 
                CLFile.AddRange(BitConverter.GetBytes((float)geometryData.triangleNormals[x].Z).Reverse());

                // Add dummies for collision flags.
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleArray[x].triangleFlagsI));
                CLFile.AddRange(BitConverter.GetBytes(geometryData.triangleArray[x].triangleFlagsII));
            }
        }

        /// <summary>
        /// Writes the header of a collision CL file.
        /// </summary>
        private void WriteCollisionFileHeader(List<byte> CLFile)
        {
            // Append the offsets of each of the individual file sections.
            CLFile.AddRange(BitConverter.GetBytes(collisionData.numberOfBytes).Reverse());
            CLFile.AddRange(BitConverter.GetBytes(collisionData.quadtreeOffset).Reverse());
            CLFile.AddRange(BitConverter.GetBytes(collisionData.triangleOffset).Reverse());
            CLFile.AddRange(BitConverter.GetBytes(collisionData.vertexOffset).Reverse());

            // Append the quadtree center and the size to the header.
            CLFile.AddRange(BitConverter.GetBytes(collisionData.quadtreeCenter.X).Reverse());
            CLFile.AddRange(BitConverter.GetBytes(collisionData.quadtreeCenter.Y).Reverse());
            CLFile.AddRange(BitConverter.GetBytes(collisionData.quadtreeCenter.Z).Reverse());
            CLFile.AddRange(BitConverter.GetBytes(collisionData.quadtreeSize).Reverse());

            // Append the quadtree base power to the header as well as the length of each file section.
            CLFile.AddRange(BitConverter.GetBytes(collisionData.basePower).Reverse());
            CLFile.AddRange(BitConverter.GetBytes((ushort)geometryData.triangleArray.Length).Reverse());
            CLFile.AddRange(BitConverter.GetBytes((ushort)geometryData.verticesArray.Length).Reverse());
            CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes.Count).Reverse());
        }

        /// <summary>
        /// Writes the triangle list to the output CL File.
        /// </summary>
        private void WriteTriangleList(List<byte> CLFile)
        {
            // Retrieve all nodes at bottom level.
            Quadtree_Properties.Quadnode[] quadnodesAtDepthLevel = quadtreeData.GetQuadnodesAtDepth(quadtreeData.depthLevel);

            // Loop over all bottom level nodes, add triangles.
            for (int x = 0; x < quadnodesAtDepthLevel.Length; x++)
            {
                // Set triangle list offset.
                quadnodesAtDepthLevel[x].triangleListOffset = (uint)CLFile.Count;

                // Add triangles.
                foreach (Geometry_Properties.Triangle triangle in quadnodesAtDepthLevel[x].trianglesInNode) 
                { CLFile.AddRange(BitConverter.GetBytes((ushort)triangle.triangleIndex).Reverse()); }
                
                // Replace the original quadnode list element
                quadtreeData.quadNodes[quadnodesAtDepthLevel[x].nodeIndex] = quadnodesAtDepthLevel[x]; 
            }
        }


        /// <summary>
        /// Writes the quadnode list to the output CL File.
        /// </summary>
        /// <param name="CLFile"></param>
        private void WriteQuadnodeList(List<byte> CLFile)
        {
            // For each node.
            for (int x = 0; x < quadtreeData.quadNodes.Count; x++)
            {
                // Append the Node, Parent and Child node indices.
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].nodeIndex).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].nodeParent).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].nodeChild).Reverse());

                // Appens the calculated neighbour indices to the node.
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].rightNeightbourIndex).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].leftNeightbourIndex).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].bottomNeighbourIndex).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].topNeighbourIndex).Reverse());

                // Append the amount of triangles present within this quadnode.
                if (quadtreeData.quadNodes[x].triangleListOffset == 0) { 
                    CLFile.AddRange(BitConverter.GetBytes((ushort)0)); 
                }
                else { 
                    CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].trianglesInNode.Count).Reverse()); 
                }
                
                // Append the offset to the triangle list within the file.
                CLFile.AddRange(BitConverter.GetBytes((uint)quadtreeData.quadNodes[x].triangleListOffset).Reverse());

                // Append the horizontal and vertical quadnode offset as well as the depth level.
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].positioningHorizontalOffset).Reverse());
                CLFile.AddRange(BitConverter.GetBytes((ushort)quadtreeData.quadNodes[x].positioningVerticalOffset).Reverse());
                CLFile.Add((byte)(quadtreeData.quadNodes[x].depthLevel));

                // Dummy bytes that are not used in the original struct.
                CLFile.AddRange(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00} );
            }
        }

        /// <summary>
        /// Complete the file header for the output colliison struct based off of the already known data.
        /// </summary>
        private Collision_Header CalculateCollisionHeader(Collision_Header currentHeader, CLFileOffsets fileOffsets)
        {
            // Fill in the necessary header data from all of the subclasses.
            currentHeader.basePower = collisionGeneratorProperties.basePower;
            currentHeader.numberOfBytes = (uint)fileOffsets.totalFileSize;
            currentHeader.numberOfNodes = (ushort)quadtreeData.quadNodes.Count;
            currentHeader.numberOfTriangles = (ushort)geometryData.triangleArray.Length;
            currentHeader.numberOfVertices = (ushort)geometryData.verticesArray.Length;
            currentHeader.quadtreeOffset = (uint)fileOffsets.nodeSectionOffset;
            currentHeader.triangleOffset = (uint)fileOffsets.triangleSectionOffset;
            currentHeader.vertexOffset = (uint)fileOffsets.vertexSectionOffset;

            // Return header.
            return currentHeader;
        }

        /// <summary>
        /// Calculates the offsets for the CL File.
        /// </summary>
        /// <returns></returns>
        private CLFileOffsets CalculateCLOffsets()
        {
            // File offsets.
            CLFileOffsets fileOffsets = new CLFileOffsets();
            
            // Current pointer in the nonexisting (yet) output file.
            int currentCursorPointer = Collision_Header.HEADER_LENGTH;

            // Retrieve all nodes at bottom level.
            Quadtree_Properties.Quadnode[] quadnodesAtDepthLevel = quadtreeData.GetQuadnodesAtDepth(quadtreeData.depthLevel);

            // Calculate offset for node section.
            for (int x = 0; x < quadnodesAtDepthLevel.Length; x++) { currentCursorPointer += (quadnodesAtDepthLevel[x].trianglesInNode.Count * 2); }

            // Set node section offset.
            fileOffsets.nodeSectionOffset = currentCursorPointer;

            // Calculate offset for triangle section.
            currentCursorPointer = currentCursorPointer + (0x20 * quadtreeData.quadNodes.Count);

            // Set triangle section offset.
            fileOffsets.triangleSectionOffset = currentCursorPointer;

            // Calculate Vertex Section Offset.
            currentCursorPointer = currentCursorPointer + (0x20 * geometryData.triangleArray.Length);

            // Set vertex section offset.
            fileOffsets.vertexSectionOffset = currentCursorPointer;

            // Calculate end of file
            currentCursorPointer = currentCursorPointer + (0x0C * geometryData.verticesArray.Length);

            // Set EOF.
            fileOffsets.totalFileSize = currentCursorPointer;

            // Return the file offsets.
            return fileOffsets;
        }

        /// <summary>
        /// Reads the entirety of the OBJ File and returns it to the main program.
        /// </summary>
        private void ReadOBJFile()
        {
            // Define new instance of the OBJ Utility Class with the OBJ Target set in sight.
            OBJ_Utilities objFile = new OBJ_Utilities(collisionGeneratorProperties.filePath);

            // Calculate all of the collision properties.
            objFile.CalculateCollisionFile();

            // Retrieve the Collision Properties
            geometryData = objFile.GetCollisionFile();
        }

        /// <summary>
        /// Calculates Normal Unit Vector of each given triangle.
        /// </summary>
        private void CalculateTriangleNormals()
        {
            // Allocate room for triangle normals.
            geometryData.triangleNormals = new Geometry_Properties.Vertex[geometryData.triangleArray.Length];

            // Calculate all triangle normals.
            for (int x = 0; x < geometryData.triangleArray.Length; x++)
            {
                // Grab the vertices from the vertices array onto the 
                Geometry_Properties.Vertex vertexOne = geometryData.verticesArray[geometryData.triangleArray[x].vertexOne];
                Geometry_Properties.Vertex vertexTwo = geometryData.verticesArray[geometryData.triangleArray[x].vertexTwo];
                Geometry_Properties.Vertex vertexThree = geometryData.verticesArray[geometryData.triangleArray[x].vertexThree];

                geometryData.triangleNormals[x] = Triangle_Utilities.CalculateNormal(vertexOne, vertexTwo, vertexThree);
            }
        }

        /// <summary>
        /// Identifies all of the intersections betweeen nodes and triangles.
        /// </summary>
        private void CalculateNodeTriangleIntersections()
        {
            // Assign all of the triangles to the very root node.
            Quadtree_Properties.Quadnode quadNode = quadtreeData.quadNodes[0];
            quadNode.trianglesInNode = geometryData.triangleArray.ToList();
            quadtreeData.quadNodes[0] = quadNode;

            // Recursively find the triangles that intersect with all of the root nodes.
            // Runs the operation on the 4 child nodes of the individual parent node.
            RecursiveFindTriangles(0, 1);
            RecursiveFindTriangles(0, 2);
            RecursiveFindTriangles(0, 3);
            RecursiveFindTriangles(0, 4);
        }

        /// <summary>
        /// Method which recursively calls itself for each quadnode until there are no children left, finds the intersecting triangles based off of the parent's triangles.
        /// </summary>
        private void RecursiveFindTriangles(int parentNodeIndex, int ownNodeIndex)
        {
            // Retrieve the quadnode.
            Quadtree_Properties.Quadnode quadNode = quadtreeData.quadNodes[ownNodeIndex];

            // Allocate memory for this node's triangle list. Approximate 1/4th of parent.
            quadNode.trianglesInNode = new List<Geometry_Properties.Triangle>(quadtreeData.quadNodes[parentNodeIndex].trianglesInNode.Count / 4);

            // Retrieve parent's triangle list.
            List<Geometry_Properties.Triangle> parentTriangle = quadtreeData.quadNodes[parentNodeIndex].trianglesInNode;

            // Check for collision against parent's triangle list.
            for (int x = 0; x < parentTriangle.Count; x++)
            {
                // Index of the individual triangle within the array of triangle boxes.
                int triangleIndex = parentTriangle[x].triangleIndex;

                // If using AABB Collision Checking, Compare the Boxes of the Triangles
                if (collisionGeneratorProperties.useAABB)
                {
                    // If Two Rectangles Collide, Add onto this node's list.
                    if (BoundingBoxIntersect(quadtreeData.nodeBoxes[quadNode.nodeIndex], geometryData.triangleBoxes[triangleIndex]))  { quadNode.trianglesInNode.Add(geometryData.triangleArray[triangleIndex]); }
                }
                else
                {
                    // Otherwise Compare Triangle-Node intersect Accurately.
                    if (CheckCollisionAccurate(geometryData.triangleArray[triangleIndex], quadtreeData.nodeBoxes[quadNode.nodeIndex]))  { quadNode.trianglesInNode.Add(geometryData.triangleArray[triangleIndex]); }
                }
            }

            // Return the node to the list
            quadtreeData.quadNodes[ownNodeIndex] = quadNode;

            // Decide if the recursive function should be ended.
            if (quadNode.nodeChild != 0) 
            { 
                RecursiveFindTriangles(quadNode.nodeIndex, quadNode.nodeChild); 
                RecursiveFindTriangles(quadNode.nodeIndex, quadNode.nodeChild + 1); 
                RecursiveFindTriangles(quadNode.nodeIndex, quadNode.nodeChild + 2); 
                RecursiveFindTriangles(quadNode.nodeIndex, quadNode.nodeChild + 3); 
            } 
            else { return; }
        }

        /// <summary>
        /// Alternative version, finds all triangles adjacent to the currently registered set of triangles.
        /// </summary>
        private void CalculateTriangleAdjacentsV2()
        {
            // Allocate Memory
            geometryData.triangleAdjacents = new Geometry_Properties.AdjacentTriangleProperties[geometryData.triangleArray.Length];

            // Assign null adjacent indices to each triangle
            for (int x = 0; x < geometryData.triangleArray.Length; x++) { geometryData.triangleAdjacents[x].triangleIndices = new ushort[3] { 0xFFFF, 0xFFFF, 0xFFFF };  }
            
            // If adjacents are enabled, find them.
            if (collisionGeneratorProperties.adjacentsEnabled)
            {
                // Return all quadnodes at max depth level.
                Quadtree_Properties.Quadnode[] quadnodesBottomLevel = quadtreeData.GetQuadnodesAtDepth(collisionGeneratorProperties.depthLevel);

                // Iterate over all bottom level nodes.
                for (int x = 0; x < quadnodesBottomLevel.Length; x++)
                {
                    // Find all adjacent triangles within each individual node.
                    QuadnodeFindTriangleAdjacents(quadnodesBottomLevel[x]);
                }
            }
        }

        /// <summary>
        /// Finds each adjacent triangle within a quadnode.
        /// </summary>
        private void QuadnodeFindTriangleAdjacents(Quadtree_Properties.Quadnode quadNode)
        {
            // Retrieve triangle list for the quadnodes.
            List<Geometry_Properties.Triangle> triangleList = quadNode.trianglesInNode;

            // For each triangle in node, find another triangle sharing the same two vertices.
            // Source of Comparison: Triangles in node.
            for (int y = 0; y < triangleList.Count; y++)
            {
                // Comparison Target: Triangles in node.
                for (int z = 0; z < triangleList.Count; z++)
                {
                    // Disallow triangle comparison with self.
                    if (z == y) { continue; }

                    // Check each set of edges, 1-2, 2-3, 3-1 for common vertices used.
                    CheckAdjacent(triangleList[y].vertexOne, triangleList[y].vertexTwo, triangleList[y].triangleIndex, triangleList[z], triangleList[z].triangleIndex, 0);
                    CheckAdjacent(triangleList[y].vertexTwo, triangleList[y].vertexThree, triangleList[y].triangleIndex, triangleList[z], triangleList[z].triangleIndex, 1);
                    CheckAdjacent(triangleList[y].vertexThree, triangleList[y].vertexOne, triangleList[y].triangleIndex, triangleList[z], triangleList[z].triangleIndex, 2);
                }
            }
        }

        /// <summary>
        /// Checks if destination triangle (triangleArray) contains source triangle vertices vertexOne, vertexTwo, if so, assign the adjacent triangles accordingly.
        /// </summary>
        /// <param name="triangleIndexSource">Index of the source triangle wehre vertexOne, vertexTwo come from</param>
        /// <param name="triangleArray">The triangle we are comparing against.</param>
        /// <param name="triangleIndexDestination">The index of the destination triangle we are comparing against.</param>
        /// <param name="adjacentIndex">The index of adjacent triangle entry for the specified triangle.</param>
        private void CheckAdjacent(int vertexOne, int vertexTwo, int triangleIndexSource, Geometry_Properties.Triangle triangle, int triangleIndexDestination, int adjacentIndex)
        {
            // Check whether the triangle has any vertices which share the supplied vertices.
            // If so, set the adjacent triangle.
            if 
            (
                triangle.vertexOne == vertexOne &&
                triangle.vertexTwo == vertexTwo
                ||
                triangle.vertexTwo == vertexOne &&
                triangle.vertexOne == vertexTwo

                ||

                triangle.vertexTwo == vertexOne &&
                triangle.vertexThree == vertexTwo
                ||
                triangle.vertexThree == vertexOne &&
                triangle.vertexTwo == vertexTwo

                ||

                triangle.vertexThree == vertexOne &&
                triangle.vertexOne == vertexTwo
                ||
                triangle.vertexOne == vertexOne &&
                triangle.vertexThree == vertexTwo
            )
            {
                // Assign the adjacent triangles.
                geometryData.triangleAdjacents[triangleIndexSource].triangleIndices[adjacentIndex] = (ushort)triangleIndexDestination; 
            }

        }

        ///////////////////////////////////////////
        ///////////////////////////////////////////
        /////////////////////////////////////////// 

        /// <summary>
        /// Benchmarks an individual method call.
        /// </summary>
        private void BenchmarkMethod(Action method, String actionText)
        {
            // Stopwatch to benchmark every action.
            Stopwatch performanceWatch = new Stopwatch();

            // Print out the action
            Console.Write(actionText + " | ");

            // Start the stopwatch.
            performanceWatch.Start();

            // Run the method.
            method();

            // Stop the stopwatch
            performanceWatch.Stop();

            // Print the results.
            Console.WriteLine(performanceWatch.ElapsedMilliseconds + "ms");
        }

        /// <summary>
        /// Generates the Quadtree node structure.
        /// </summary>
        private void GenerateQuadtree()
        {
            // Assign the quadnode property structure.
            quadtreeData = new Quadtree_Properties(collisionGeneratorProperties.depthLevel, collisionGeneratorProperties.basePower);

            // Generate all of the quadnodes.
            quadtreeData.GenerateQuadnodes();
        }

        /// <summary>
        /// Calculate the quadtree size and center, necessary for both the file but also for searching triangle intersections/
        /// </summary>
        private void CalculateQuadtreeDimensions()
        {
            collisionData.basePower = collisionGeneratorProperties.basePower;
            collisionData.GetQuadtreeSizeCenter(geometryData.verticesArray);
        }

        /// <summary>
        /// Calculates the bounding boxes of each of the triangles, used for collision checking against nodes.
        /// </summary>
        private void CalculateTriangleBoundingBoxes()
        {
            // Calculate all of the triangles' bounding boxes.
            geometryData.CalculateTriangleBoundingBoxes();
        }


        /// <summary>
        /// Calculates the bounding boxes of each of the quadNodes, used for collision checking against triangles.
        /// </summary>
        private void CalculateNodeBoundingBoxes()
        {
            // Calculate all of the node bounding boxes.
            quadtreeData.CalculateAllNodeBoundingBoxes(collisionGeneratorProperties.basePower, collisionData, collisionGeneratorProperties.nodeScale);
        }

        /// <summary>
        /// Calculates all of the neighbours for each individual node.
        /// </summary>
        private void CalculateNodeNeighbours()
        {
            quadtreeData.CalculateNodeNeighbours();
        }

        /// <summary>
        /// Calculates all of the local node neighbours. (Sets neighbour relationship between nodes of each parent node)
        /// </summary>
        private void CalculateLocalNodeNeighbours()
        {
            quadtreeData.CalculateLocalNodeNeighbours();
        }

        /// <summary>
        /// Annihilates the empty nodes from the quadtree structure.
        /// </summary>
        private void AnnihilateEmptyNodes()
        {
            quadtreeData.RemoveEmptyNodes();
        }

        /// <summary>
        /// Checks collision between two boxes, if they collide, return true.
        /// The checks are performed on the edges of the boxes.
        /// </summary>
        private bool BoundingBoxIntersect(Quadtree_Properties.NodeRectangle thisRectangle, Quadtree_Properties.NodeRectangle nodeRectangle)
        {
            if (thisRectangle.MaxX < nodeRectangle.MinX) return false; // if a is left of b
            if (thisRectangle.MinX > nodeRectangle.MaxX) return false; // if a is right of b
            if (thisRectangle.MaxZ < nodeRectangle.MinZ) return false; // if a is above b
            if (thisRectangle.MinZ > nodeRectangle.MaxZ) return false; // if a is below b
            return true; // boxes overlap
        }

        /// <summary>
        /// Accurately checks node-triangle collisions, down to the individual vertices.
        /// </summary>
        /// <param name="triangle">The triangle to check passed in node collision against.</param>
        /// <param name="nodeRectangle">The bounding box square representing the current node.</param>
        /// <returns>True if there is an intersection, otherwise false.</returns>
        private bool CheckCollisionAccurate (Geometry_Properties.Triangle triangle, Quadtree_Properties.NodeRectangle nodeRectangle)
        {
            // First check if the bounding boxes intersect, if they don't, discard the operation.
            if (! BoundingBoxIntersect(nodeRectangle, geometryData.triangleBoxes[triangle.triangleIndex])) { return false; }

            // Check all vertices for presence in rectangle for possible fast return.
            // If any of the vertices is inside the node, then there must be a collision.
            if (IsVertexInRectangle(geometryData.verticesArray[triangle.vertexOne], nodeRectangle)) { return true; }
            if (IsVertexInRectangle(geometryData.verticesArray[triangle.vertexTwo], nodeRectangle)) { return true; }
            if (IsVertexInRectangle(geometryData.verticesArray[triangle.vertexThree], nodeRectangle)) { return true; }

            // Define vertices of the Rectangles.
            Geometry_Properties.Vertex topLeftNodeEdge = new Geometry_Properties.Vertex(nodeRectangle.MinX, 0, nodeRectangle.MinZ);
            Geometry_Properties.Vertex topRightNodeEdge = new Geometry_Properties.Vertex(nodeRectangle.MaxX, 0, nodeRectangle.MinZ);
            Geometry_Properties.Vertex bottomRightNodeEdge = new Geometry_Properties.Vertex(nodeRectangle.MaxX, 0, nodeRectangle.MaxZ);
            Geometry_Properties.Vertex bottomLeftNodeEdge = new Geometry_Properties.Vertex(nodeRectangle.MaxX, 0, nodeRectangle.MaxZ);

            // Define triangle vertices.
            Geometry_Properties.Vertex triangleVertexOne = geometryData.verticesArray[triangle.vertexOne];
            Geometry_Properties.Vertex triangleVertexTwo = geometryData.verticesArray[triangle.vertexTwo];
            Geometry_Properties.Vertex triangleVertexThree = geometryData.verticesArray[triangle.vertexThree];

            // Then check if any of the node line segments intersect the triangle line segments.
            // We basically check if there are intersections between any of the three line segments of the triangle
            // i.e. A=>B, B=>C and C=>A and the four edges of the rectangle.
            #region Line Intersection Tests: Triangle line segments (vertex X to vertex Y) against node up down left right edges.
            
            // Check the top edge of the against the triangle line segments.
            if ( LineIntersectionTest(triangleVertexOne, triangleVertexTwo, topLeftNodeEdge, topRightNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexTwo, triangleVertexThree, topLeftNodeEdge, topRightNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexThree, triangleVertexOne, topLeftNodeEdge, topRightNodeEdge) ) { return true; }

            // Check the left edge of the against the triangle line segments.
            if ( LineIntersectionTest(triangleVertexOne, triangleVertexTwo, topLeftNodeEdge, bottomLeftNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexTwo, triangleVertexThree, topLeftNodeEdge, bottomLeftNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexThree, triangleVertexOne, topLeftNodeEdge, bottomLeftNodeEdge) ) { return true; }

            // Check the bottom edge of the against the triangle line segments.
            if ( LineIntersectionTest(triangleVertexOne, triangleVertexTwo, bottomLeftNodeEdge, bottomRightNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexTwo, triangleVertexThree, bottomLeftNodeEdge, bottomRightNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexThree, triangleVertexOne, bottomLeftNodeEdge, bottomRightNodeEdge) ) { return true; }

            // Check the right edge of the against the triangle line segments.
            if ( LineIntersectionTest(triangleVertexOne, triangleVertexTwo, topRightNodeEdge, bottomRightNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexTwo, triangleVertexThree, topRightNodeEdge, bottomRightNodeEdge) ) { return true; }
            if ( LineIntersectionTest(triangleVertexThree, triangleVertexOne, topRightNodeEdge, bottomRightNodeEdge) ) { return true; }

            #endregion Line Intersection Tests: Triangle line segments (vertex X to vertex Y) against node up down left right edges.

            // Understanding the code below:
            // Consider the triangle as three vectors, in a fixed rotation order A=>B, B=>C, C=>A
            // Now consider each vertex of the triangle and each square vertex, compute the cross product between each triangle edge and rectangle
            // vector (3*4=12 comparisons in total). If all of the cross products are of the same sign, or zero, the triangle is inside.

            // Conceptually we are determining whether the vertices are on the left or right side of the line, albeit the side is not cared.
            // We do not care whether it is left or right specifically, or whether it is in clockwise or anticlockwise order, only that all of the vertices
            // of the square are on the same side of the lines.
            // i.e. if all of the vertices are on the same side of each line, the vertices are "trapped" between the 3 lines, meaning that they must be
            // inside the triangle.
            #region Node Inside Triangle Tests: Determine if all of the node vertices are on the right side of the line, with vertices going clockwise.
            
            // Vertex One   = A
            // Vertex Two   = B
            // Vertex Three = C
            // Note: Variable names are completely arbitrary, to make code less confusing/long

            // Compare line A=>B with all edge vertices.
            bool v1 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, topLeftNodeEdge);
            bool v2 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, topRightNodeEdge);
            bool v3 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, bottomRightNodeEdge);
            bool v4 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, bottomLeftNodeEdge);

            // Compare line B=>C with all edge vertices.
            bool v5 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, topLeftNodeEdge);
            bool v6 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, topRightNodeEdge);
            bool v7 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, bottomRightNodeEdge);
            bool v8 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, bottomLeftNodeEdge);

            // Compare line C=>A with all edge vertices.
            bool v9  = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, topLeftNodeEdge);
            bool v10 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, topRightNodeEdge);
            bool v11 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, bottomRightNodeEdge);
            bool v12 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, bottomLeftNodeEdge);

            // Check whether the node is inside the triangle.
            if (v1.AllEqual(v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12)) 
            { 
                return true; 
            }

            #endregion Node Inside Triangle Tests: Determine if all of the node vertices are on the right side of the line, with vertices going clockwise.

            // Else return false;
            return false;
        }

        /// <summary>
        /// Returns true if a vertex is contained within the boundaries of a 2D Rectangle.
        /// </summary>
        private bool IsVertexInRectangle(Geometry_Properties.Vertex vertex, Quadtree_Properties.NodeRectangle rectangle)
        {
            if 
            (
                (vertex.X >= rectangle.MinX && vertex.X <= rectangle.MaxX) &&
                (vertex.Z >= rectangle.MinZ && vertex.Z <= rectangle.MaxZ) 
            ) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Verifies whether two lines intersect with each other. (Only X & Z are used)
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex1">The vertex which marks the start of the second line.</param>
        /// <param name="targetVertex2">The vertex which marks the end of the second line.</param>
        /// <returns></returns>
        private bool LineIntersectionTest(Geometry_Properties.Vertex lineVertex1, Geometry_Properties.Vertex lineVertex2, Geometry_Properties.Vertex targetVertex1, Geometry_Properties.Vertex targetVertex2)
        {
            // Check for bounding box intersections of lines first.
            Quadtree_Properties.NodeRectangle boundingBox1 = GetBoundingBox(lineVertex1, lineVertex2);
            Quadtree_Properties.NodeRectangle boundingBox2 = GetBoundingBox(targetVertex1, targetVertex2);

            // If the bounding boxes do not intersect, return false.
            if (! BoundingBoxIntersect(boundingBox1, boundingBox2)) { return false; }

            // Check if the line segments defining the triangle's edges and the square's edges intersect.
            if (! LineSegmentTouchesOrCrossesLine(lineVertex1, lineVertex2, targetVertex1, targetVertex2)) { return false; }

            // Lines intersect.
            return true;
        }

        /// <summary>
        /// Retrieves a bounding box for a set of two supplied vertices.
        /// </summary>
        /// <returns></returns>
        private Quadtree_Properties.NodeRectangle GetBoundingBox(Geometry_Properties.Vertex lineVertex1, Geometry_Properties.Vertex lineVertex2)
        {
            // Define bounding box
            Quadtree_Properties.NodeRectangle boundingBox = new Quadtree_Properties.NodeRectangle();

            // Determine Max X
            if (lineVertex1.X > lineVertex2.X) { boundingBox.MaxX = lineVertex1.X; boundingBox.MinX = lineVertex2.X; }
            else { boundingBox.MaxX = lineVertex2.X; boundingBox.MinX = lineVertex1.X; }

            // Determine Max Z
            if (lineVertex1.Z > lineVertex2.Z) { boundingBox.MaxZ = lineVertex1.Z; boundingBox.MinZ = lineVertex2.Z; }
            else { boundingBox.MaxZ = lineVertex2.Z; boundingBox.MinZ = lineVertex1.Z; }

            // Return bounding box.
            return boundingBox;
        }

        /// <summary>
        /// Calculates the Cross Product of Two Vertices/Points, using their X & Z Coordinates.
        /// Note: Cross product of two 2D vectors is ill defined, this is much rather the determinant.
        /// </summary>
        /// <param name="firstVertex"></param>
        /// <param name="secondVertex"></param>
        /// <returns></returns>
        private double CrossProduct(Geometry_Properties.Vertex firstVertex, Geometry_Properties.Vertex secondVertex) 
        {
            return firstVertex.X * secondVertex.Z - secondVertex.X * firstVertex.Z;
        }

        /// <summary>
        /// Returns true if the line segment touches or crosses a line.
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex">The vertex which we are checking against.</param>
        private bool LineSegmentTouchesOrCrossesLine(Geometry_Properties.Vertex lineVertex1, Geometry_Properties.Vertex lineVertex2, Geometry_Properties.Vertex targetLineVertex1, Geometry_Properties.Vertex targetLineVertex2)
        {
            // Check if either of the points we are checking of the second line are on the line (just in case).
            // Confirm if the two vertices are on the opposite end of the line.

            return IsPointOnLine(lineVertex1, lineVertex2, targetLineVertex1) ||
                   IsPointOnLine(lineVertex1, lineVertex2, targetLineVertex2) ||
                   (IsPointRightOfLine(lineVertex1, lineVertex2, targetLineVertex1) ^ IsPointRightOfLine(lineVertex1, lineVertex2, targetLineVertex2));;
        }

        /// <summary>
        /// Checks if there the supplied point targetVertex is on the line defined by lineVertex1 & lineVertex2
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex">The vertex which we are checking against.</param>
        private bool IsPointOnLine(Geometry_Properties.Vertex lineVertex1, Geometry_Properties.Vertex lineVertex2, Geometry_Properties.Vertex targetVertex) 
        {
            // Move the line defined by lineVertex1, lineVertex2 such that the vertex defines the vector of a line that crosses the point 0,0.
            // More specifically: Move the end vertex to a location that is an offset of target vertex - source vertex. i.e. make it a vector relative to 0,0
            Geometry_Properties.Vertex tempVector = new Geometry_Properties.Vertex(lineVertex2.X - lineVertex1.X, 0, lineVertex2.Z - lineVertex1.Z);

            // Move the point we are comparing defined by targetVertex such that it is an offset/vector from 0,0 (another line)
            // Our assumption is that linevertex1 (original start point of first line segment) is located at 0,0 , and we want to define everything
            // relative to that specific point, thus both offsetting the end of the line segment and our target vertex.
            Geometry_Properties.Vertex tempPoint = new Geometry_Properties.Vertex(targetVertex.X - lineVertex1.X, 0, targetVertex.Z - lineVertex1.Z);

            // Obtain the Cross Product, if it is very close, within a certain range then the point is on the line.
            double crossProduct = CrossProduct(tempVector, tempPoint);

            // Extremely small margin of error.
            return Math.Abs(crossProduct) < 0.000001;
        }

        /// <summary>
        /// Checks if there the supplied point targetVertex is on the line defined by lineVertex1 & lineVertex2
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex">The vertex which we are checking against.</param>
        private bool IsPointRightOfLine(Geometry_Properties.Vertex lineVertex1, Geometry_Properties.Vertex lineVertex2, Geometry_Properties.Vertex targetVertex) 
        {
            // Move the line defined by lineVertex1, lineVertex2 such that the vertex defines the vector of a line that crosses the point 0,0.
            // More specifically: Move the end vertex to a location that is an offset of target vertex - source vertex. i.e. make it a vector relative to 0,0
            Geometry_Properties.Vertex tempVector = new Geometry_Properties.Vertex(lineVertex2.X - lineVertex1.X, 0, lineVertex2.Z - lineVertex1.Z);

            // Move the point we are comparing defined by targetVertex such that it is an offset/vector from 0,0 (another line)
            // Our assumption is that linevertex1 (original start point of first line segment) is located at 0,0 , and we want to define everything
            // relative to that specific point, thus both offsetting the end of the line segment and our target vertex.
            Geometry_Properties.Vertex tempPoint = new Geometry_Properties.Vertex(targetVertex.X - lineVertex1.X, 0, targetVertex.Z - lineVertex1.Z);

            // Obtain the Cross Product, if it is very close, within a certain range then the point is on the line.
            double crossProduct = CrossProduct(tempVector, tempPoint);

            // Extremely small margin of error.
            return crossProduct < 0;
        }
    }
}