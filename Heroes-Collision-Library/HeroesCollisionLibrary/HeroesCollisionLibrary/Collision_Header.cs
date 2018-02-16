namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Defines the header of a Sonic Heroes Collision file, contains data on the overall structure of the file.
    /// </summary>
    public class Collision_Header
    {
        /// <summary>
        /// Constructor for the class.
        /// </summary>
        public Collision_Header() {}

        /// <summary>
        /// Defines the number of bytes stored by the collision file, the length of the file.
        /// </summary>
        public uint numberOfBytes;

        /// <summary>
        /// Set the length of the header.
        /// </summary>
        public const int HEADER_LENGTH = 0x28;

        /// <summary>
        /// Defines the offset at which the quadtree section starts in the file.
        /// </summary>
        public uint quadtreeOffset;

        /// <summary>
        /// Offset to the triangle section which defines all of the triangles and their vertices.
        /// </summary>
        public uint triangleOffset;

        /// <summary>
        /// Offset to the vertex section which defines all of the vertices stored in the file.
        /// </summary>
        public uint vertexOffset;

        /// <summary>
        /// Defines the center position of the top level quadtree.
        /// </summary>
        public Geometry_Properties.Vertex quadtreeCenter = new Geometry_Properties.Vertex(0,0,0);

        /// <summary>
        /// Defines the size of the quadtree (a quadtree is a square), equal to the two most displaced vertices in any axis.
        /// </summary>
        public float quadtreeSize;

        /// <summary>
        /// The base Offset Power Level for the quadnode structure. It is unknown what this does.
        /// </summary>
        public ushort basePower;

        /// <summary>
        /// Defines the number of triangles used in the collision structure, the amount of entries in the triangle section.
        /// </summary>
        public ushort numberOfTriangles;

        /// <summary>
        /// Defines the number of vertices used in the collision structure, the amount of entries in the vertex section.
        /// </summary>
        public ushort numberOfVertices;

        /// <summary>
        /// Defines the number of quadnodes used in the collision structure, the amount of entries in the quadnode section.
        /// </summary>
        public ushort numberOfNodes;

        /// <summary>
        /// Determines the overall size and center of the quadtree structure.
        /// </summary>
        public void GetQuadtreeSizeCenter(Geometry_Properties.Vertex[] verticesArray)
        {
            // Provide area of storage of the current maximum and minimum XYZ values.
            Geometry_Properties.Vertex maximumXYZ = new Geometry_Properties.Vertex(0,0,0);
            Geometry_Properties.Vertex minimumXYZ = new Geometry_Properties.Vertex(0,0,0);

            // Iterate over all of the vertices to find the maximum and/or minimum values for the vertices.
            for (int x = 0; x < verticesArray.Length; x++)
            {
                if (verticesArray[x].X > maximumXYZ.X) { maximumXYZ.X = verticesArray[x].X; }
                if (verticesArray[x].Y > maximumXYZ.Y) { maximumXYZ.Y = verticesArray[x].Y; }
                if (verticesArray[x].Z > maximumXYZ.Z) { maximumXYZ.Z = verticesArray[x].Z; }

                if (verticesArray[x].X < minimumXYZ.X) { minimumXYZ.X = verticesArray[x].X; }
                if (verticesArray[x].Y < minimumXYZ.Y) { minimumXYZ.Y = verticesArray[x].Y; }
                if (verticesArray[x].Z < minimumXYZ.Z) { minimumXYZ.Z = verticesArray[x].Z; }
            }

            // Calculate the center of the quadtree from the known minimum and maximum vertices.
            quadtreeCenter.X = ((maximumXYZ.X + minimumXYZ.X) / 2.0F);
            quadtreeCenter.Y = ((maximumXYZ.Y + minimumXYZ.Y) / 2.0F);
            quadtreeCenter.Z = ((maximumXYZ.Z + minimumXYZ.Z) / 2.0F);

            // Obtain the size of the quadtree.
            // Check if there is a greater difference in the X axis min/max than Z axis.
            if ((maximumXYZ.X - minimumXYZ.X) > (maximumXYZ.Z - minimumXYZ.Z))  
            { 
                // If true, X axis difference is the size.
                quadtreeSize = maximumXYZ.X - minimumXYZ.X;  
                //quadtreeSize = (int)(((int)(maximumXYZ.X - minimumXYZ.X) / basePower) * basePower);  
            }
            else 
            { 
                // If false, Z axis difference is the size.
                quadtreeSize = maximumXYZ.Z - minimumXYZ.Z;  
                //quadtreeSize = (int)(((int)(maximumXYZ.Z - minimumXYZ.Z) / basePower) * basePower);  
            }
        }
    }
}