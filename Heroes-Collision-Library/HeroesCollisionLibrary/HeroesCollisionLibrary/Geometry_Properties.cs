using System.Collections.Generic;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Class that defines the individual properties required to generate a collision file.
    /// </summary>
    public class Geometry_Properties
    {
        /// <summary>
        /// Array of all vertices of the OBJ File we are reading.
        /// </summary>
        public Vertex[] verticesArray;

        /// <summary>
        /// Array of all triangles of the OBJ File we are reading.
        /// </summary>
        public Triangle[] triangleArray; 

        /// <summary>
        /// Array of all triangles' normals contained in Geometry_Properties.triangleArray.
        /// </summary>
        public Vertex[] triangleNormals; 

        /// <summary>
        /// Array of all triangles' adjacents contained in Geometry_Properties.triangleArray.
        /// </summary>
        public AdjacentTriangleProperties[] triangleAdjacents;

        /// <summary>
        /// An array of a list of rectangles which define the triangle bounding boxes, used for fast collision checking.
        /// </summary>
        public Quadtree_Properties.NodeRectangle[] triangleBoxes;

        /// <summary>
        /// Struct that defines a vertex within 3D World Space.
        /// </summary>
        public struct Vertex
        {
            /// <summary>
            /// The X Coordinate Position of the Vertex.
            /// </summary>
            public float X;
            /// <summary>
            /// The Y Coordinate Position of the Vertex.
            /// </summary>
            public float Y;
            /// <summary>
            /// The Z Coordinate Position of the Vertex.
            /// </summary>
            public float Z;

            /// <summary>
            /// Constructor fot the Vertex class.
            /// </summary>
            /// <param name="x">The X Coordinate Position of the Vertex.</param>
            /// <param name="y">The Y Coordinate Position of the Vertex.</param>
            /// <param name="z">The Z Coordinate Position of the Vertex.</param>
            public Vertex(float x, float y, float z)
            { 
                X = x; 
                Y = y; 
                Z = z; 
            }
        }

        /// <summary>
        /// Struct that defines three vertices used for the rendering of a triangle.
        /// </summary>
        public struct Triangle
        {
            // Triangle Indices
            public ushort triangleIndex;
            public ushort vertexOne;
            public ushort vertexTwo;
            public ushort vertexThree;

            // Triangle Flags
            public uint triangleFlagsI;
            public uint triangleFlagsII;
        }

        /// <summary>
        /// Struct that defines entries for adjacent triangles which share at least two vertices.
        /// </summary>
        public struct AdjacentTriangleProperties
        {
            /// <summary>
            /// Defines the individual indexes of triangles in triangleArray of triangles that are adjacent to the current triangle.
            /// </summary>
            public ushort[] triangleIndices;
        }

        /// <summary>
        /// Calculates all of the triangles' bounding boxes, providing rectangle representations for each triangle.
        /// </summary>
        public void CalculateTriangleBoundingBoxes()
        {
            // Allocate the array of boxes for the triangles.
            triangleBoxes = new Quadtree_Properties.NodeRectangle[triangleArray.Length];

            // Iterating over each triangle
            for (int x = 0; x < triangleArray.Length; x++)
            {
                // Assign index to each triangle.
                triangleArray[x].triangleIndex = (ushort)x;

                // Retrieve the vertices of the triangle.
                Vertex[] triangleVertices = new Vertex[3];
                triangleVertices[0] = verticesArray[triangleArray[x].vertexOne];
                triangleVertices[1] = verticesArray[triangleArray[x].vertexTwo];
                triangleVertices[2] = verticesArray[triangleArray[x].vertexThree];

                // Calculate the maximum and minimums in the X and Z axis.
                // Provide area of storage of the current maximum and minimum XYZ values.
                Geometry_Properties.Vertex maxXYZ = new Geometry_Properties.Vertex(triangleVertices[0].X,triangleVertices[0].Y,triangleVertices[0].Z);
                Geometry_Properties.Vertex minXYZ = new Geometry_Properties.Vertex(triangleVertices[0].X,triangleVertices[0].Y,triangleVertices[0].Z);

                // Iterate over all of the vertices to find the maximum and/or minimum values for the vertices.
                for (int z = 1; z < triangleVertices.Length; z++)
                {
                    if (triangleVertices[z].X > maxXYZ.X) { maxXYZ.X = triangleVertices[z].X; }
                    if (triangleVertices[z].Z > maxXYZ.Z) { maxXYZ.Z = triangleVertices[z].Z; }

                    if (triangleVertices[z].X < minXYZ.X) { minXYZ.X = triangleVertices[z].X; }
                    if (triangleVertices[z].Z < minXYZ.Z) { minXYZ.Z = triangleVertices[z].Z; }
                }

                // Write the minimum and maximum onto the bounding box.
                triangleBoxes[x].MinX = minXYZ.X;
                triangleBoxes[x].MinZ = minXYZ.Z;
                triangleBoxes[x].MaxX = maxXYZ.X;
                triangleBoxes[x].MaxZ = maxXYZ.Z;
            }
        }
    }
}