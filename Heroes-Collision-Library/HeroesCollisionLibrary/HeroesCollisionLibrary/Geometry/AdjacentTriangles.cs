using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using HeroesCollisionLibrary.Collision;
using HeroesCollisionLibrary.Geometry.Structures;

namespace HeroesCollisionLibrary.Geometry
{
    public static class AdjacentTriangles
    {

        /// <summary>
        /// Alternative version, finds all triangles adjacent to the currently registered set of triangles.
        /// </summary>
        /// <param name="quadtreeGenerator">The quadtree containing the nodes which hold triangle references.</param>
        public static void CalculateTriangleAdjacentsV2(QuadtreeGenerator quadtreeGenerator)
        {
            // Assign null adjacent indices to each triangle
            foreach (var triangle in GeometryData.Triangles)
            {
                triangle.AdjacentTriangles = new int[3] { -1, -1, -1 };
            }

            // If adjacents are disabled, return.
            if (!CollisionGenerator.Properties.EnableAdjacents) return;

            // Retrieve all nodes with triangles.
            Quadnode[] quadnodesWithTriangles = QuadtreeGenerator.QuadNodes.Where(x => x.TrianglesInNode.Count > 0).ToArray();

            // Iterate over all bottom level nodes.
            foreach (var quadNode in quadnodesWithTriangles)
            {
                // Find all adjacent triangles within each individual node.
                QuadnodeFindTriangleAdjacents(quadNode);
            }
        }

        /// <summary>
        /// Finds each adjacent triangle within a quadnode.
        /// </summary>
        private static void QuadnodeFindTriangleAdjacents(Quadnode quadNode)
        {
            // Retrieve triangle list for the quadnodes.
            HeroesTriangle[] triangleList = quadNode.TrianglesInNode.ToArray();

            // For each triangle in node, find another triangle sharing the same two vertices.
            // Source of Comparison: Triangles in node.
            for (int y = 0; y < triangleList.Length; y++)
            {
                // Comparison Target: Triangles in node.
                for (int z = 0; z < triangleList.Length; z++)
                {
                    // Disallow triangle comparison with self.
                    if (z == y) { continue; }

                    // Check each set of edges, 1-2, 2-3, 3-1 for common vertices used.
                    CheckAdjacent(triangleList[z], triangleList[y].VertexOne, triangleList[y].VertexTwo, triangleList[y].TriangleIndex, triangleList[z].TriangleIndex, 0);
                    CheckAdjacent(triangleList[z], triangleList[y].VertexTwo, triangleList[y].VertexThree, triangleList[y].TriangleIndex, triangleList[z].TriangleIndex, 1);
                    CheckAdjacent(triangleList[z], triangleList[y].VertexThree, triangleList[y].VertexOne, triangleList[y].TriangleIndex, triangleList[z].TriangleIndex, 2);
                }
            }
        }

        /// <summary>
        /// Checks if destination triangle (triangleArray) contains source triangle vertices vertexOne, vertexTwo, if so, assign the adjacent triangles accordingly.
        /// </summary>
        /// <param name="triangleIndexSource">Index of the source triangle wehre vertexOne, vertexTwo come from</param>
        /// <param name="triangleIndexDestination">The index of the destination triangle we are comparing against.</param>
        /// <param name="adjacentIndex">The index of adjacent triangle entry for the specified triangle.</param>
        /// <param name="vertexOne">The first vertex of the triangle.</param>
        /// <param name="vertexTwo">The second vertex of the trangle.</param>
        /// <param name="triangle">The triangle object we are going to use for the search of common vertices.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckAdjacent(HeroesTriangle triangle, int vertexOne, int vertexTwo, int triangleIndexSource, int triangleIndexDestination, int adjacentIndex)
        {
            // Check whether the triangle has any vertices which share the supplied vertices.
            // If so, set the adjacent triangle.
            if
            (
                triangle.VertexOne == vertexOne &&
                triangle.VertexTwo == vertexTwo
                ||
                triangle.VertexTwo == vertexOne &&
                triangle.VertexOne == vertexTwo

                ||

                triangle.VertexTwo == vertexOne &&
                triangle.VertexThree == vertexTwo
                ||
                triangle.VertexThree == vertexOne &&
                triangle.VertexTwo == vertexTwo

                ||

                triangle.VertexThree == vertexOne &&
                triangle.VertexOne == vertexTwo
                ||
                triangle.VertexOne == vertexOne &&
                triangle.VertexThree == vertexTwo
            )
            {
                // Assign the adjacent triangles.
                GeometryData.Triangles[triangleIndexSource].AdjacentTriangles[adjacentIndex] = (ushort)triangleIndexDestination;
            }
        }
    }
}
