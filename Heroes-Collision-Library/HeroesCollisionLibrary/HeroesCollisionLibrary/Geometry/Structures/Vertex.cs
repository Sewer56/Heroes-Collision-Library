namespace HeroesCollisionLibrary.Geometry.Structures
{
    /// <summary>
    /// This struct defines a vertex within three dimensional world space.
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
        /// Constructor fot the Vertex class, defining a three dimensional vertex.
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
}
