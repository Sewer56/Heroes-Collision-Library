using HeroesCollisionLibrary.Geometry.Structures;
using HeroesCollisionLibrary.Geometry;

namespace HeroesCollisionLibrary.Collision.Bounding_Box
{
    /// <summary>
    /// Calculates the bounding boxes for a triangle.
    /// </summary>
    public static class CalculateBoundingBox
    {
        /// <summary>
        /// Calculates all of the triangles' bounding boxes, providing rectangle representations for each triangle.
        /// </summary>
        public static void CalculateTriangleBoundingBoxes()
        {
            // Allocate the array of boxes for the triangles.
            GeometryData.TriangleBoxes = new Rectangle[GeometryData.Triangles.Count];

            // Iterating over each triangle
            for (int x = 0; x < GeometryData.Triangles.Count; x++)
            {
                // Assign index to each triangle.
                GeometryData.Triangles[x].TriangleIndex = x;

                // Retrieve the vertices of the triangle.
                Vertex[] triangleVertices = new Vertex[3];
                triangleVertices[0] = GeometryData.Vertices[GeometryData.Triangles[x].VertexOne];
                triangleVertices[1] = GeometryData.Vertices[GeometryData.Triangles[x].VertexTwo];
                triangleVertices[2] = GeometryData.Vertices[GeometryData.Triangles[x].VertexThree];

                // Calculate the maximum and minimums in the X and Z axis.
                // Provide area of storage of the current maximum and minimum XYZ values.
                Vertex maxXyz = new Vertex(triangleVertices[0].X, triangleVertices[0].Y, triangleVertices[0].Z);
                Vertex minXyz = new Vertex(triangleVertices[0].X, triangleVertices[0].Y, triangleVertices[0].Z);

                // Iterate over all of the vertices to find the maximum and/or minimum values for the vertices.
                for (int z = 1; z < triangleVertices.Length; z++)
                {
                    if (triangleVertices[z].X > maxXyz.X) { maxXyz.X = triangleVertices[z].X; }
                    if (triangleVertices[z].Z > maxXyz.Z) { maxXyz.Z = triangleVertices[z].Z; }

                    if (triangleVertices[z].X < minXyz.X) { minXyz.X = triangleVertices[z].X; }
                    if (triangleVertices[z].Z < minXyz.Z) { minXyz.Z = triangleVertices[z].Z; }
                }

                // Write the minimum and maximum onto the bounding box.
                GeometryData.TriangleBoxes[x] = new Rectangle();
                GeometryData.TriangleBoxes[x].MinX = minXyz.X;
                GeometryData.TriangleBoxes[x].MinZ = minXyz.Z;
                GeometryData.TriangleBoxes[x].MaxX = maxXyz.X;
                GeometryData.TriangleBoxes[x].MaxZ = maxXyz.Z;
            }
        }
    }
}
