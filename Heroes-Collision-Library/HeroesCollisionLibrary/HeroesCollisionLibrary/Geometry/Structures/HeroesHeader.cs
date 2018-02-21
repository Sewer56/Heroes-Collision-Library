using System.Collections.Generic;
using HeroesCollisionLibrary.Geometry.Structures;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Defines the header of a Sonic Heroes Collision file, contains data on the overall structure of the file.
    /// </summary>
    public static class HeroesHeader
    {
        /// <summary>
        /// Defines the number of bytes stored by the collision file, the length of the file.
        /// </summary>
        public static uint NumberOfBytes;

        /// <summary>
        /// Set the length of the header.
        /// </summary>
        public const int HeaderLength = 0x28;

        /// <summary>
        /// Defines the offset at which the quadtree section starts in the file.
        /// </summary>
        public static uint QuadtreeOffset;

        /// <summary>
        /// Offset to the triangle section which defines all of the triangles and their vertices.
        /// </summary>
        public static uint TriangleOffset;

        /// <summary>
        /// Offset to the vertex section which defines all of the vertices stored in the file.
        /// </summary>
        public static uint VertexOffset;

        /// <summary>
        /// Defines the center position of the top level quadtree.
        /// </summary>
        public static Vertex QuadtreeCenter = new Vertex(0,0,0);

        /// <summary>
        /// Defines the size of the quadtree (a quadtree is a square), equal to the two most displaced vertices in any axis.
        /// </summary>
        public static float QuadtreeSize;

        /// <summary>
        /// The base Offset Power Level for the quadnode structure. It is unknown what this does.
        /// </summary>
        public static ushort BasePower;

        /// <summary>
        /// Defines the number of triangles used in the collision structure, the amount of entries in the triangle section.
        /// </summary>
        public static ushort NumberOfTriangles;

        /// <summary>
        /// Defines the number of vertices used in the collision structure, the amount of entries in the vertex section.
        /// </summary>
        public static ushort NumberOfVertices;

        /// <summary>
        /// Defines the number of quadnodes used in the collision structure, the amount of entries in the quadnode section.
        /// </summary>
        public static ushort NumberOfNodes;

        /// <summary>
        /// Determines the overall size and center of the quadtree structure.
        /// </summary>
        /// <param name="vertices">The vertices of the collision file</param>
        public static void GetQuadtreeSizeCenter(List<Vertex> vertices)
        {
            // Provide area of storage of the current maximum and minimum XYZ values.
            Vertex maximumXyz = new Vertex(0,0,0);
            Vertex minimumXyz = new Vertex(0,0,0);

            // Iterate over all of the vertices to find the maximum and/or minimum values for the vertices.
            for (int x = 0; x < vertices.Count; x++)
            {
                if (vertices[x].X > maximumXyz.X) { maximumXyz.X = vertices[x].X; }
                if (vertices[x].Y > maximumXyz.Y) { maximumXyz.Y = vertices[x].Y; }
                if (vertices[x].Z > maximumXyz.Z) { maximumXyz.Z = vertices[x].Z; }

                if (vertices[x].X < minimumXyz.X) { minimumXyz.X = vertices[x].X; }
                if (vertices[x].Y < minimumXyz.Y) { minimumXyz.Y = vertices[x].Y; }
                if (vertices[x].Z < minimumXyz.Z) { minimumXyz.Z = vertices[x].Z; }
            }

            // Calculate the center of the quadtree from the known minimum and maximum vertices.
            QuadtreeCenter.X = ((maximumXyz.X + minimumXyz.X) / 2.0F);
            QuadtreeCenter.Y = ((maximumXyz.Y + minimumXyz.Y) / 2.0F);
            QuadtreeCenter.Z = ((maximumXyz.Z + minimumXyz.Z) / 2.0F);

            // Obtain the size of the quadtree.
            // Check if there is a greater difference in the X axis min/max than Z axis.
            // If bottom line is uncommented, make quadtree size divisible by base power.
            if ((maximumXyz.X - minimumXyz.X) > (maximumXyz.Z - minimumXyz.Z))  
            { 
                // If true, X axis difference is the size.
                QuadtreeSize = maximumXyz.X - minimumXyz.X;
                //QuadtreeSize = (int)(((int)(maximumXyz.X - minimumXyz.X) / BasePower) * BasePower);  
            }
            else 
            { 
                // If false, Z axis difference is the size.
                QuadtreeSize = maximumXyz.Z - minimumXyz.Z;
                //QuadtreeSize = (int)(((int)(maximumXyz.Z - minimumXyz.Z) / BasePower) * BasePower);  
            }
        }
    }
}