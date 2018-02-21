using System.Collections.Generic;
using HeroesCollisionLibrary.Geometry.Structures;

namespace HeroesCollisionLibrary.Geometry
{
    /// <summary>
    /// Class that defines the individual properties required to generate a collision file.
    /// </summary>
    public static class GeometryData
    {
        /// <summary>
        /// Array of all vertices of the collision file to be generated.
        /// The individual triangles, of struct HeroesTriangle reference these by index.
        /// </summary>
        public static List<Vertex> Vertices = new List<Vertex>(65535);

        /// <summary>
        /// Array of all triangles that will be used for the generation of collision.
        /// Triangles contain information such as collision flags.
        /// </summary>
        public static List<HeroesTriangle> Triangles = new List<HeroesTriangle>(65535); 

        /// <summary>
        /// An array of a list of rectangles which define the triangle bounding boxes, used for fast collision checking.
        /// </summary>
        public static Rectangle[] TriangleBoxes;
    }
}