namespace HeroesCollisionLibrary.Geometry.Structures
{
    /// <summary>
    /// Represents a generic rectangle structure used for collision checking.
    /// Defines the top, left, right and botton edge positions.
    /// </summary>
    public class Rectangle
    {
        /// <summary>
        /// The minimum X coordinate which defines the rectangle.
        /// </summary>
        public float MinX;

        /// <summary>
        /// The minimum Z coordinate which defines the rectangle.
        /// </summary>
        public float MinZ;

        /// <summary>
        /// The minimum X coordinate which defines the rectangle.
        /// </summary>
        public float MaxX;

        /// <summary>
        /// The maximum Z coordinate which defines the rectangle.
        /// </summary>
        public float MaxZ;
    }
}
