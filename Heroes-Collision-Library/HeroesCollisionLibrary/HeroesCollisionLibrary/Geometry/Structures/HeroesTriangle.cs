namespace HeroesCollisionLibrary.Geometry.Structures
{
    /// <summary>
    /// Defines a Triangle as seen in Sonic Heroes' CL Collision Structure.
    /// Contains both the details such as triangle vertex indices, adjacent triangles,
    /// normals and primary as well as secondary flags.
    /// </summary>
    public class HeroesTriangle
    {
        /// <summary>
        /// Defines the first vertex of the triangle.
        /// The triangle vertices have no specific order.
        /// </summary>
        public int VertexOne;

        /// <summary>
        /// Defines the second vertex of the triangle.
        /// The triangle vertices have no specific order.
        /// </summary>
        public int VertexTwo;

        /// <summary>
        /// Defines the third vertex of the triangle.
        /// The triangle vertices have no specific order.
        /// </summary>
        public int VertexThree;

        /// <summary>
        /// Specifies the TriangleIndex(es) of triangles that are adjacent to this triangle.
        /// Adjacent triangles share 2 vertices with the current triangle. Default value is 0xFFFF.
        /// </summary>
        public int[] AdjacentTriangles;

        /// <summary>
        /// Array of all triangles' normals contained in Geometry_Properties.triangleArray.
        /// These are the normals to the faces of the triangles.
        /// </summary>
        public Vertex Normals;

        /// <summary>
        /// The primary flags for the object, specifying the collision properties of the object.
        /// The flags specified here are believed to override Secondary Flags (FlagsSecondary) 
        /// when applicable.
        /// </summary>
        public byte[] FlagsPrimary;

        /// <summary>
        /// The secondary flags for the object, specifying the collision properties of the object.
        /// The flags specified here are believed to override Secondary Flags (FlagsSecondary) 
        /// when applicable.
        /// </summary>
        public byte[] FlagsSecondary;

        /// <summary>
        /// [Not for developers]
        /// The index of the current triangle. 
        /// This value should not be set, it is automatically generated during collision 
        /// exporting and is used by the collision exporter for generating node triangle lists.
        /// </summary>
        public int TriangleIndex;
    }
}
